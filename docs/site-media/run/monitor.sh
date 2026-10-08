#!/usr/bin/env bash
# Runs the Blazor Monitor on $MONITOR_URL. Leave it running in its own terminal.
set -euo pipefail
source "$(dirname "$0")/../env.sh"
cd "$REPO_ROOT"
exec dotnet run -r linux-x64 -p:IndTraceEnterprise=false \
    --project Src/Code/Presentation/IndTrace.Monitor/IndTrace.Monitor.csproj --urls "$MONITOR_URL"
