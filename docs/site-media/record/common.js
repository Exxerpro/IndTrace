// Shared settings for the recording scripts. Values come from docs/site-media/env.sh.
const { chromium } = require('playwright');
const { execFileSync } = require('child_process');
const path = require('path');
const fs = require('fs');

const work = process.env.MEDIA_WORK;
if (!work) throw new Error('MEDIA_WORK is not set; source docs/site-media/env.sh first.');
const monitor = process.env.MONITOR_URL || 'http://localhost:5100';
const authFile = path.join(work, 'auth.json');
const viewport = { width: 1600, height: 1000 };

// CHROMIUM_PATH selects a system browser; without it Playwright's own Chromium is used (npx playwright install chromium).
const launch = () => chromium.launch(process.env.CHROMIUM_PATH ? { executablePath: process.env.CHROMIUM_PATH } : {});
const pause = ms => new Promise(resolve => setTimeout(resolve, ms));

// A browser context that records a WebM into $MEDIA_WORK/video/<name>/, optionally signed in.
const recordingContext = (browser, name, { signedIn = false } = {}) => {
  const dir = path.join(work, 'video', name);
  fs.rmSync(dir, { recursive: true, force: true });
  return browser.newContext({
    viewport,
    recordVideo: { dir, size: viewport },
    ...(signedIn ? { storageState: authFile } : {}),
  });
};

// Feeds one simulated PLC event to the running gateway (run/send.sh) and waits `seconds`.
const send = (command, seconds) =>
  execFileSync(path.join(__dirname, '..', 'run', 'send.sh'), [command, String(seconds)], { stdio: 'inherit' });

// Runs one query against the demo database and returns its single text result.
const sqlScalar = query => execFileSync('docker', ['exec', process.env.DEMO_SQL_CONTAINER || 'indtrace-demo-sql',
  '/opt/mssql-tools18/bin/sqlcmd', '-C', '-S', 'localhost', '-U', 'sa', '-P', process.env.DEMO_PASSWORD,
  '-d', process.env.DEMO_DB || 'IndTraceData', '-h', '-1', '-W', '-Q', `SET NOCOUNT ON; ${query}`]).toString().trim();

const newestBarCodeId = () => Number(sqlScalar('SELECT ISNULL(MAX(BarCodeId), 0) FROM BarCodes'));

// Sends a create-barcode event and returns the new label. The gateway can take a few seconds on its first
// command, so wait for a barcode newer than the ones that existed before the event, instead of guessing.
const createBarCode = async (machine, partNumber) => {
  const before = newestBarCodeId();
  send(`m ${machine} pn ${partNumber} bc NEW cmd 4 ps 0 cs 0`, 2);
  for (let i = 0; i < 40; i++) {
    const label = sqlScalar(`SELECT TOP (1) Label FROM BarCodes WHERE BarCodeId > ${before} ORDER BY BarCodeId`);
    if (label) return label;
    await pause(500);
  }
  throw new Error(`No barcode was created for ${partNumber} at machine ${machine}; check the gateway console.`);
};

module.exports = { work, monitor, authFile, viewport, launch, pause, recordingContext, send, createBarCode };
