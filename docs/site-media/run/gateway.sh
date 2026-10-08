#!/usr/bin/env bash
# Runs the gateway in simulation mode. Its console accepts simulated PLC events only when attached to a
# terminal, so it runs under `script` (a pseudo-terminal) and reads its input from $MEDIA_WORK/gateway.in,
# where send.sh appends commands. Its console output goes to $MEDIA_WORK/gateway.tty.
set -euo pipefail
source "$(dirname "$0")/../env.sh"
export GatewaySimulationOptions__EnableSimulation=true
cd "$REPO_ROOT"
: > "$MEDIA_WORK/gateway.in"
tail -f "$MEDIA_WORK/gateway.in" | script -qfec \
    "dotnet run -r linux-x64 -p:IndTraceEnterprise=false --project Src/Code/Infrastructure/IndTrace.Communications/IndTrace.Communications.csproj" \
    "$MEDIA_WORK/gateway.tty" > /dev/null
