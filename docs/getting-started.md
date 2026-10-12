# Getting started

This guide takes you from a clone of this repository to a running IndTrace line: SQL Server in a container, a
demo database, the three hosts (hub, gateway, Monitor) and a signed-in Monitor. Then
[simulate-a-part.md](simulate-a-part.md) drives parts through that line.

The steps were checked on Linux against an empty SQL Server 2022 container. They use the bash scripts in
`docs/site-media/`, which also produce the project website's videos.

## What you need

- The .NET 10 SDK.
- Docker, for SQL Server. Any SQL Server 2019 or later works if you set the connection strings yourself.
- Linux or WSL, with bash. The scripts pass `-r linux-x64` because the projects default to the `win-x64`
  runtime, and the gateway script uses the util-linux `script` command.

## 1. Build

```bash
git clone https://github.com/Exxerpro/IndTrace.git
cd IndTrace
dotnet build Src/IndTrace.sln
```

Warnings are errors, so a clean build ends with `0 Warning(s)` and `0 Error(s)`.

## 2. Settings

`docs/site-media/env.sh` holds every setting, and every script sources it. Only `DEMO_PASSWORD` is required.
It is both the SQL Server `sa` password and the demo user's password, so it must meet SQL Server's complexity
rules (at least 8 characters, with three of: upper case, lower case, digits, symbols).

```bash
export DEMO_PASSWORD='<choose one>'
source docs/site-media/env.sh
```

Source `env.sh` from bash, or from the repository root; it finds the repository from its own path.

| Variable | Default | Meaning |
|---|---|---|
| `DEMO_PASSWORD` | none (required) | `sa` and demo-user password |
| `DEMO_SQL_CONTAINER` | `indtrace-demo-sql` | Docker container name |
| `DEMO_SQL_PORT` | `14333` | Host port for SQL Server |
| `DEMO_DB` / `DEMO_IDENTITY_DB` | `IndTraceData` / `IndTraceIdentity` | Database names |
| `DEMO_USER` | `demo@example.com` | Demo sign-in user (Administrator role) |
| `MONITOR_URL` / `HUB_URL` | `http://localhost:5100` / `http://localhost:5200` | App addresses |
| `MEDIA_WORK` | `artifacts/site-media` | Work directory for the gateway console |

`env.sh` exports the connection strings the hosts read (`ConnectionStrings__IndTraceDbContext`,
`ConnectionStrings__IndTraceDbIdentity`), pointing at the container.

## 3. Start SQL Server

```bash
docker run -d --name "$DEMO_SQL_CONTAINER" -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD="$DEMO_PASSWORD" \
    -p "$DEMO_SQL_PORT:1433" mcr.microsoft.com/mssql/server:2022-latest
```

Wait until `docker logs "$DEMO_SQL_CONTAINER"` shows `Recovery is complete`; that takes about half a minute.

## 4. Create the demo database

```bash
dotnet run -r linux-x64 --project Src/Code/Infrastructure/IndTrace.DemoSeed -- --reset
```

`IndTrace.DemoSeed` creates both databases by running their EF Core migrations, then writes a small,
fictitious line. It ends with `Demo database ready: 9 stations, 6 products, demo user demo@example.com.`

- **`--reset`** deletes and recreates both databases. Without it, the tool refuses a database that already has
  stations.
- **Stations:** WS100 to WS900, each with an enabled simulated PLC (PLC id = machine id) and the tags the
  simulated controller needs.
- **Products:** six, each with a linear route, a label rule at WS100 and recipes. The walkthrough uses
  `L100003`, routed WS100 → WS200 → WS300 → WS400 → WS500.
- **User:** `DEMO_USER` in the Administrator role, with `DEMO_PASSWORD` as its password.

The database holds no parts yet. They come from the simulated line.

### Without the demo data

To create empty databases instead, use the EF Core tools (`dotnet tool install --global dotnet-ef`):

```bash
dotnet ef database update --project Src/Code/Infrastructure/IndTrace.Persistence/IndTrace.Persistence.csproj \
    --connection "$ConnectionStrings__IndTraceDbContext"
dotnet ef database update --project Src/Code/Infrastructure/IndTrace.Identity/IndTrace.Identity.csproj \
    --connection "$ConnectionStrings__IndTraceDbIdentity"
```

That gives you the schema and nothing else: no user, stations, PLCs, tags or products. The rest of this guide
assumes the demo database.

## 5. Run the hosts

Start each in its own terminal, in this order, and leave them running:

```bash
docs/site-media/run/hub.sh        # SignalR hub on HUB_URL
docs/site-media/run/monitor.sh    # Blazor Monitor on MONITOR_URL
docs/site-media/run/gateway.sh    # gateway, in simulation mode
```

The hub and Monitor are ready when they print `Now listening on:`.

The gateway runs with `GatewaySimulationOptions__EnableSimulation=true`, so every PLC uses the in-memory
simulated controller. It reads simulated PLC events from its console, and only when that console is a terminal,
so `gateway.sh` runs it under `script` and feeds it from `$MEDIA_WORK/gateway.in`. Its output goes to
`$MEDIA_WORK/gateway.tty`. It is ready when that file shows
`All 9 workers initialized successfully: [100, 200, ..., 900]`:

```bash
grep -a "workers initialized" "$MEDIA_WORK/gateway.tty"
```

The scripts call `dotnet run` with `-p:IndTraceEnterprise=false`. To run a host by hand, use the same command
as its script.

## 6. Sign in

Open <http://localhost:5100>, sign in as `demo@example.com` with your `DEMO_PASSWORD`, and open:

- **Monitor** (`/monitor`): the live line;
- **Barcodes** (`/Barcodes`): every part and its history;
- **Products** (`/Products`): products and their routes.

Next, run parts through the line: [simulate-a-part.md](simulate-a-part.md).

## Troubleshooting

- **The SQL Server container exits at once.** `docker logs "$DEMO_SQL_CONTAINER"` usually says the password
  is too simple. Remove the container (`docker rm`), choose a stronger `DEMO_PASSWORD` and start it again.
- **`REPO_ROOT=... is not this repository`.** `env.sh` was sourced from zsh outside the repository root.
  Source it from bash, or `cd` to the repository root first.
- **`address already in use`.** Another program uses port 5100, 5200 or 14333. Set `MONITOR_URL`, `HUB_URL` or
  `DEMO_SQL_PORT` before sourcing `env.sh`.
- **The gateway refuses a PLC.** The community edition has no hardware drivers; it accepts only PLCs with
  simulation enabled, and only when `GatewaySimulationOptions__EnableSimulation=true`.

## Stop and clean up

Stop the three scripts with Ctrl+C, then `docker stop "$DEMO_SQL_CONTAINER"`. Keep the container to continue
later with `docker start "$DEMO_SQL_CONTAINER"`, or remove it with `docker rm`.
