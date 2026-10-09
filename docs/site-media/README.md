# Site media: demo database, walkthrough videos and screenshots

These scripts produce the project website's walkthrough videos (`site/assets/video/`) and screenshots
(`site/assets/screens/`). They run the community edition against a local SQL Server container, with a demo
database written by `IndTrace.DemoSeed` and a simulated production line.

They live under `docs/` because the export sync never touches `docs/`, and nothing here is published by the
Pages workflow (it deploys `site/` only). Recordings, screenshots and the gateway console go to
`artifacts/site-media/`, which git ignores.

## Requirements

- Docker, the .NET 10 SDK, Node.js 18 or later, and ffmpeg with `libx264` and `libwebp`.
- Linux or WSL. The run scripts pass `-r linux-x64`, because the projects default to the `win-x64` runtime.
- A Chromium for Playwright: either `npx playwright install chromium` in `record/`, or a system browser through
  `CHROMIUM_PATH` (for example `/usr/bin/chromium`).

## Settings

`env.sh` holds every setting; all scripts source it. Only `DEMO_PASSWORD` is required. It is both the SQL
Server `sa` password and the demo user's password, so it must meet SQL Server's complexity rules.

| Variable | Default | Meaning |
|---|---|---|
| `DEMO_PASSWORD` | none (required) | `sa` and demo-user password |
| `DEMO_SQL_CONTAINER` | `indtrace-demo-sql` | Docker container name |
| `DEMO_SQL_PORT` | `14333` | Host port for SQL Server |
| `DEMO_DB` / `DEMO_IDENTITY_DB` | `IndTraceData` / `IndTraceIdentity` | Database names |
| `DEMO_USER` | `demo@example.com` | Demo sign-in user (Administrator role) |
| `MONITOR_URL` / `HUB_URL` | `http://localhost:5100` / `http://localhost:5200` | App addresses |
| `MEDIA_WORK` | `artifacts/site-media` | Work directory for recordings and the gateway console |
| `CHROMIUM_PATH` | unset | System Chromium for Playwright |

## 1. Build the demo database

```bash
export DEMO_PASSWORD='<choose one>'
source docs/site-media/env.sh

docker run -d --name "$DEMO_SQL_CONTAINER" -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD="$DEMO_PASSWORD" \
    -p "$DEMO_SQL_PORT:1433" mcr.microsoft.com/mssql/server:2022-latest

dotnet run -r linux-x64 --project Src/Code/Infrastructure/IndTrace.DemoSeed -- --reset
```

`IndTrace.DemoSeed` reads the connection strings and `DEMO_PASSWORD` that `env.sh` exports. With `--reset` it
deletes and recreates `DEMO_DB` and `DEMO_IDENTITY_DB`. Without it, it refuses a database that already holds
stations. It writes a small, fictitious line and creates the demo user `DEMO_USER` in the Administrator role:

- **Stations:** WS100 to WS900, each with an enabled simulated PLC (PLC id = machine id) and the tags the
  simulated controller needs.
- **Products:** six products with unique part numbers, each with a linear route, a label rule at WS100, a master
  label and recipes. The walkthrough uses `L100003`, routed WS100 → WS500. Recipe minimum cycle times are 0, so
  short simulated cycles are accepted.
- **Customers:** seven invented names. `Apex Lighting` has no product yet, because a customer can have only one;
  `define-routing.js` adds one for it.

The database holds no history. Parts, cycles and reports come from running parts through the line (step 3).

## 2. Run the line

Each in its own terminal (all three must keep running):

```bash
docs/site-media/run/hub.sh
docs/site-media/run/monitor.sh
docs/site-media/run/gateway.sh
```

The gateway runs in simulation mode. Its console reads simulated PLC events only from a terminal, so
`gateway.sh` runs it under `script` and feeds it from `$MEDIA_WORK/gateway.in`. Wait until
`$MEDIA_WORK/gateway.tty` shows `All 9 workers initialized successfully`.

`run/send.sh` sends one event by hand and prints the gateway's result:

```bash
docs/site-media/run/send.sh "m 100 pn L100003 bc NEW cmd 4 ps 0 cs 0"
```

Format: `m <machine> pn <part number> bc <label|NEW> cmd <code> ps <0|1> cs <0|1>`.

| Code | Event |
|---|---|
| 4 | create barcode |
| 8 | read barcode |
| 16 | create cycle |
| 32 | cycle OK |
| 64 | cycle not OK |
| 128 | end of process |
| 256 | reject |

## 3. Record

```bash
cd docs/site-media/record
npm install
source ../env.sh
node login.js            # signs in as DEMO_USER; saves the session for the next scripts
node journey.js          # video 1: part A runs WS100 -> WS500; part B skips WS400 and is refused at WS500
node trace.js            # video 3: opens part A's history (label from journey.js)
node define-routing.js   # video 2: adds product TL-2040 and its five-station route
node screenshots.js      # dashboard, routing, products, machines and reports screenshots
./encode.sh              # writes site/assets/video/* and site/assets/screens/*
```

- **Order:** run `journey.js` before `trace.js`, which reads part A's label from `$MEDIA_WORK/labels.txt`, and
  before `screenshots.js`, so the dashboard and reports have parts to show.
- **Recording again:** stop the three run scripts, rerun the seeder with `--reset`, and start them again. That
  removes TL-2040 (`define-routing.js` needs it not to exist) and gives the journey video a clean first frame.
- **`encode.sh`:** trims each recording's first seconds and takes the poster frame at a fixed second.
  Check the posters, and adjust the `encode` lines if a recording's timing changed.
- **Screenshots:** check them before publishing. The demo data is invented, but anything typed into the Monitor
  while recording shows up too.

Then review `site/assets/` and update the video descriptions and durations in `site/index.html` and
`site/es/index.html` (including the `VideoObject` JSON-LD) if they changed.

## Clean up

Stop the three run scripts, then `docker stop "$DEMO_SQL_CONTAINER"`. Keep the container to record again later.
