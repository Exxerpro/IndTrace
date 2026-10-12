#!/usr/bin/env bash
# send.sh "<command>" [wait-seconds]: feeds one simulated PLC event to the running gateway (gateway.sh) and
# prints the gateway's command and result lines for it.
#   Format: m <machine> pn <part number> bc <label|NEW> cmd <code> ps <0|1> cs <0|1>
#   Codes:  4 create barcode, 8 read barcode, 16 create cycle, 32 cycle OK, 64 cycle not OK,
#           128 end of process, 256 reject
set -euo pipefail
work="${MEDIA_WORK:-$(git -C "$(dirname "$0")" rev-parse --show-toplevel)/artifacts/site-media}"
start=$(wc -l < "$work/gateway.tty")
printf '%s\n\n' "$1" >> "$work/gateway.in"
sleep "${2:-5}"
# The console prints each message on its own line, after a timestamp line; older builds put a "[hh:mm:ss INF] "
# prefix on the same line. Strip colors and that prefix, then keep the command and result lines, in order.
tail -n +"$start" "$work/gateway.tty" | tr -d '\r' | sed -E 's/\x1b\[[0-9;]*m//g; s/^\[[0-9:]+ (INF|WRN|ERR)\] //' \
    | grep -E '^(Command [0-9]+=|Result: [A-Z])' | grep -v '^Result: Valid$' \
    | cut -c1-230 | awk '!seen[$0]++' | head -8 || true
