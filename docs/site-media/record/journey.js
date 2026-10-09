// Video 1, "A part's journey": the live monitor while two parts run through the simulated line.
// Part A passes WS100 -> WS500. Part B skips WS400, so its arrival at WS500 is not a legal successor and
// the gateway refuses it. Needs the hub, Monitor and gateway running (run/*.sh).
// Prints both labels and saves them to $MEDIA_WORK/labels.txt for trace.js.
const fs = require('fs');
const path = require('path');
const { work, monitor, launch, pause, recordingContext, send, createBarCode } = require('./common');

const partNumber = 'L100003';
const runStations = (label, stations) => {
  for (const machine of stations) {
    send(`m ${machine} pn ${partNumber} bc ${label} cmd 16 ps 0 cs 0`, 2.6);
    send(`m ${machine} pn ${partNumber} bc ${label} cmd 32 ps 1 cs 1`, 1.4);
  }
};

(async () => {
  const browser = await launch();
  const context = await recordingContext(browser, 'part-journey');
  const page = await context.newPage();
  await page.goto(`${monitor}/monitor`, { waitUntil: 'networkidle' });
  await page.evaluate(() => { document.body.style.zoom = '0.72'; });
  await pause(2500);

  const partA = await createBarCode(100, partNumber);
  runStations(partA, [100, 200, 300, 400, 500]);

  const partB = await createBarCode(100, partNumber);
  runStations(partB, [100, 200, 300]);
  send(`m 500 pn ${partNumber} bc ${partB} cmd 16 ps 0 cs 0`, 1);

  await pause(4000);
  await context.close();
  await browser.close();
  fs.writeFileSync(path.join(work, 'labels.txt'), `${partA} ${partB}\n`);
  console.log(partA, partB);
})();
