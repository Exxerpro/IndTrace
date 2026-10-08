# Site media: demo database, walkthrough videos and screenshots

These scripts produce the project website's walkthrough videos (`site/assets/video/`) and screenshots
(`site/assets/screens/`). They run the community edition against a local SQL Server container, with a demo
database built from the repository's fictitious test fixtures and a simulated production line.

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

dotnet run --project docs/site-media/demo-db            # recreates both databases and the demo user
demo_sql < docs/site-media/demo-db/prepare-demo.sql     # makes the fixtures runnable as a line (idempotent)
```

`DemoSeeder` deletes and recreates `DEMO_DB` and `DEMO_IDENTITY_DB`, loads every fixture, and creates the demo
user. `-- --only <Fixture>` reloads a single fixture and keeps the rest.

`prepare-demo.sql` closes the gaps between unit-test fixtures and a running line:
- **PLCs:** 100–900, one per station, enabled and bound to their machine.
- **Tags:** the simulated controller needs, per PLC, exactly 4 event tags (group 1), at least one register tag
  (group 128) and at least one reference tag (group 256). It adds the missing event and reference tags.
- **Registers:** removes the boundary-test rows near `int.MaxValue`, and reseeds the identity.
- **Routes:** removes `WorkFlows` rows with a 0 endpoint, or route authoring refuses to save.
- **Demo product:** part number `L100003`, routed WS100 → WS500, with a label rule and a recipe minimum cycle
  time of 0. Cycle times are whole seconds and the minimum is exclusive.
- **Customers:** renames them to fictitious names and strips the former names from product text. At the end it
  lists any former name still present. That list must be empty before anything is recorded.

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

- **Order:** run `journey.js` before `trace.js`, which reads part A's label from `$MEDIA_WORK/labels.txt`.
- **Recording again:** `define-routing.js` needs TL-2040 not to exist yet.
  `demo_sql < docs/site-media/demo-db/reset-media.sql` removes it.
- **The journey video** shows whatever the Monitor already holds. For a clean first frame, record it on a
  freshly seeded database.
- **`encode.sh`:** trims each recording's first seconds and takes the poster frame at a fixed second.
  Check the posters, and adjust the `encode` lines if a recording's timing changed.
- **Screenshots:** check them for real brand names before publishing.

Then review `site/assets/` and update the video descriptions and durations in `site/index.html` and
`site/es/index.html` (including the `VideoObject` JSON-LD) if they changed.

## Clean up

Stop the three run scripts, then `docker stop "$DEMO_SQL_CONTAINER"`. Keep the container to record again later.
