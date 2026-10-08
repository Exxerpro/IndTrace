#!/usr/bin/env bash
# Runs the SignalR hub on $HUB_URL. Leave it running in its own terminal.
set -euo pipefail
source "$(dirname "$0")/../env.sh"
cd "$REPO_ROOT"
exec dotnet run -r linux-x64 -p:IndTraceEnterprise=false \
    --project Src/Code/Infrastructure/IndTrace.Hub/IndTrace.Hub.Server.csproj --urls "$HUB_URL"
