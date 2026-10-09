#!/usr/bin/env bash
# Shared settings for the site-media scripts. Source it: `source docs/site-media/env.sh`.
#
# Required: DEMO_PASSWORD, used both as the SQL Server `sa` password of the demo container and as the
# password of the demo sign-in user. Pick any value that meets SQL Server's complexity rules.
: "${DEMO_PASSWORD:?set DEMO_PASSWORD first (SQL sa password and demo user password)}"

export DEMO_SQL_CONTAINER="${DEMO_SQL_CONTAINER:-indtrace-demo-sql}"
export DEMO_SQL_PORT="${DEMO_SQL_PORT:-14333}"
export DEMO_DB="${DEMO_DB:-IndTraceData}"
export DEMO_IDENTITY_DB="${DEMO_IDENTITY_DB:-IndTraceIdentity}"
export DEMO_USER="${DEMO_USER:-demo@example.com}"
export MONITOR_URL="${MONITOR_URL:-http://localhost:5100}"
export HUB_URL="${HUB_URL:-http://localhost:5200}"

# Repository root and a work directory for the gateway console, recordings and screenshots.
export REPO_ROOT="${REPO_ROOT:-$(git -C "$(dirname "${BASH_SOURCE[0]}")" rev-parse --show-toplevel)}"
# zsh has no BASH_SOURCE, so sourcing from zsh outside the repository finds the wrong root; refuse it.
[[ -f "$REPO_ROOT/docs/site-media/env.sh" ]] || { echo "REPO_ROOT=$REPO_ROOT is not this repository; source env.sh from bash, or from the repository root" >&2; return 1; }
export MEDIA_WORK="${MEDIA_WORK:-$REPO_ROOT/artifacts/site-media}"
mkdir -p "$MEDIA_WORK"

auth="User Id=sa;Password=$DEMO_PASSWORD;Encrypt=False;TrustServerCertificate=True"
for key in IndTraceDbContext IndTraceDbContext62 IndTraceDbContext45; do
    export "ConnectionStrings__$key=Server=localhost,$DEMO_SQL_PORT;Database=$DEMO_DB;$auth"
done
export ConnectionStrings__IndTraceDbIdentity="Server=localhost,$DEMO_SQL_PORT;Database=$DEMO_IDENTITY_DB;$auth"
export HubMonitorOptions__Url="$HUB_URL/eventmonitor"
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"

# sqlcmd inside the demo container: demo_sql <sqlcmd args...>
demo_sql() {
    docker exec -i "$DEMO_SQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$DEMO_PASSWORD" -d "$DEMO_DB" -b "$@"
}
