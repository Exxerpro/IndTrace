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
tail -n +"$start" "$work/gateway.tty" | sed 's/\x1b\[[0-9;]*m//g' \
    | grep -E '^\[[0-9:]+ (INF|WRN|ERR)\] (Command [0-9]+=|Result:|.*(failed|FAIL|nvalid|eject|legal|successor))' \
    | grep -v DbCommand | cut -c1-230 | sort -u | head -8 || true
