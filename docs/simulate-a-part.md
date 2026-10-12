# Simulate a part through the line

This walkthrough runs parts through the demo line from [getting-started.md](getting-started.md). You play the
PLCs and the operator: you report what happened at each station. IndTrace does not move the part or tell the
line where to send it. It records each event and checks whether it was legal, for example whether the station
a part arrives at is allowed by its route.

You need the demo database and the three hosts from the getting-started guide running, with `env.sh`
sourced in your terminal.

## The line

Product `L100003` runs through five stations:

| Station | Role |
|---|---|
| WS100 | Label printing; the part gets its barcode here |
| WS200 | Screw driving |
| WS300 | Leak test |
| WS400 | Vision inspection |
| WS500 | End-of-line test (final station) |

## Sending events

`docs/site-media/run/send.sh` sends one PLC event to the running gateway, waits, and prints the gateway's
answer:

```bash
docs/site-media/run/send.sh "m <machine> pn <part number> bc <label|NEW> cmd <code> ps <0|1> cs <0|1>"
```

`m` is the station (machine id), `pn` the part number and `bc` the barcode label, or `NEW` when asking for
one. `ps` and `cs` are the part and cycle status flags the PLC reports: `1` for OK, `0` otherwise.

| Code | Event |
|---|---|
| 4 | create barcode |
| 8 | read barcode |
| 16 | create cycle (the part arrives at the station) |
| 32 | cycle OK |
| 64 | cycle not OK |
| 128 | end of process |
| 256 | reject |

An optional second argument is how many seconds to wait for the answer (default 5).

Open the Monitor's live page (<http://localhost:5100/monitor>) before you start, to watch the stations update.

## Part 1: a good part

Ask WS100 for a barcode:

```bash
docs/site-media/run/send.sh "m 100 pn L100003 bc NEW cmd 4 ps 0 cs 0"
```

```
Command 4=CreateBarCodeAsync from Machine 100 and PLC 100 Status Success: BarCode: WS100L100003262850001
```

The label is the station, the part number, the year and day of the year, and a counter, so yours differs. Keep
it in a variable:

```bash
LABEL=WS100L100003262850001   # the label your gateway printed
```

Creating the barcode also starts the part's cycle at WS100, so finish it there, then run it through the other
stations; each station is an arrival (`16`) and a result (`32`):

```bash
docs/site-media/run/send.sh "m 100 pn L100003 bc $LABEL cmd 32 ps 1 cs 1"
for m in 200 300 400 500; do
    docs/site-media/run/send.sh "m $m pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
    docs/site-media/run/send.sh "m $m pn L100003 bc $LABEL cmd 32 ps 1 cs 1"
done
```

Every event answers `Status Success`, for example:

```
Command 16=CreateCycleAsync from Machine 200 and PLC 200 Status Success: BarCode: WS100L100003262850001
Command 32=UpdateCycleOkAsync from Machine 200 and PLC 200 Status Success: BarCode: WS100L100003262850001
```

Open **Barcodes** (<http://localhost:5100/Barcodes>), search for the last digits of the label and click it. Its
history lists one finished cycle per station, WS100 to WS500, and the values each station recorded.

## Part 2: a part that skips a station

Create a second part and run it through WS100 to WS300 only:

```bash
docs/site-media/run/send.sh "m 100 pn L100003 bc NEW cmd 4 ps 0 cs 0"
LABEL=<the new label>
docs/site-media/run/send.sh "m 100 pn L100003 bc $LABEL cmd 32 ps 1 cs 1"
for m in 200 300; do
    docs/site-media/run/send.sh "m $m pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
    docs/site-media/run/send.sh "m $m pn L100003 bc $LABEL cmd 32 ps 1 cs 1"
done
```

Now report it at WS500, skipping WS400:

```bash
docs/site-media/run/send.sh "m 500 pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
```

```
Result: DestinationNotValid
Result: Request Failed; Details: ["Validation failed -128"]; Errors: ["Validation failed -128"]
Command 16=CreateCycleAsync from Machine 500 and PLC 500 Status Validation failed -128
```

WS500 is not a legal next station after WS300, so the arrival is refused. The PLC receives the refusal code;
what the line then does with the part is up to the line. No cycle is recorded at WS500, and the part's
history still ends at WS300.

## Part 3: a part that fails and is reworked

Create a third part, finish it at WS100, and fail it at WS200:

```bash
docs/site-media/run/send.sh "m 100 pn L100003 bc NEW cmd 4 ps 0 cs 0"
LABEL=<the new label>
docs/site-media/run/send.sh "m 100 pn L100003 bc $LABEL cmd 32 ps 1 cs 1"
docs/site-media/run/send.sh "m 200 pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
docs/site-media/run/send.sh "m 200 pn L100003 bc $LABEL cmd 64 ps 0 cs 0"
```

The failed result is recorded (`Status Success` means the event was stored, not that the part passed). If the
part now shows up at WS300, it is refused with `DestinationNotValid`, like part 2:

```bash
docs/site-media/run/send.sh "m 300 pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
```

Run it at WS200 again, and this time it passes; after that, WS300 accepts it:

```bash
docs/site-media/run/send.sh "m 200 pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
docs/site-media/run/send.sh "m 200 pn L100003 bc $LABEL cmd 32 ps 1 cs 1"
docs/site-media/run/send.sh "m 300 pn L100003 bc $LABEL cmd 16 ps 0 cs 0"
```

Its history shows both WS200 cycles, the failed one and the passed one.

## Where the events go

- **Gateway console:** `$MEDIA_WORK/gateway.tty` has the full log of each event, including the SQL it ran.
- **Database:** barcodes are in `BarCodes`, cycles in `Cycles`, and every request and answer in
  `TaskGatewayRequests` and `TaskGatewayResponses`. `env.sh` defines `demo_sql`, which runs `sqlcmd` in the
  container, for example:

  ```bash
  demo_sql -Q "SELECT TOP (5) Label, PartStatus FROM BarCodes ORDER BY BarCodeId DESC"
  ```

- **Monitor:** the live page, **Barcodes** for each part's history, and **Reports**.

To start over with an empty line, stop the three hosts, rerun `IndTrace.DemoSeed` with `--reset`, and start
them again.
