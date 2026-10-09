// Video 3, "Trace a part": search a barcode and open its history. Needs a signed-in session (login.js).
// Usage: node trace.js [label]   (default: part A from $MEDIA_WORK/labels.txt, written by journey.js)
const fs = require('fs');
const path = require('path');
const { work, monitor, viewport, authFile, launch, pause, recordingContext } = require('./common');

const label = process.argv[2] || fs.readFileSync(path.join(work, 'labels.txt'), 'utf8').split(/\s+/)[0];

(async () => {
  const browser = await launch();
  // Warm the page up off camera so the recording starts on a ready screen.
  const warm = await browser.newContext({ viewport, storageState: authFile });
  await (await warm.newPage()).goto(`${monitor}/Barcodes`, { waitUntil: 'networkidle' });
  await warm.close();

  const context = await recordingContext(browser, 'trace-part', { signedIn: true });
  const page = await context.newPage();
  await page.goto(`${monitor}/Barcodes`, { waitUntil: 'networkidle' });
  await pause(4500);
  const search = page.locator('input[placeholder="Search"]').first();
  await search.click();
  await search.pressSequentially(label.slice(-6), { delay: 140 });
  await pause(1800);
  // A click that lands while the table re-renders is lost, so retry until the detail dialog opens.
  for (let i = 0; i < 4 && (await page.locator('.mud-dialog').count()) === 0; i++) {
    await page.locator('button', { hasText: label }).first().click();
    await pause(2500);
  }
  await page.waitForSelector('.mud-dialog', { timeout: 10000 });
  await pause(5000);
  for (let i = 0; i < 6; i++) { await page.mouse.wheel(0, 180); await pause(500); }
  await pause(2500);
  await context.close();
  await browser.close();
})();
