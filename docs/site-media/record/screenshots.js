// The website's screenshots, as PNGs in $MEDIA_WORK/screens/ (1440x900 at 2x). encode.sh converts them.
const fs = require('fs');
const path = require('path');
const { work, monitor, launch } = require('./common');

const shots = [
  { name: 'dashboard', route: '/dashboard' },
  { name: 'routing', route: '/configuration' },
  { name: 'products', route: '/Products' },
  { name: 'machines', route: '/machines' },
  { name: 'reports', route: '/Reports', click: 'text=GET LIST BARCODES' },
];

(async () => {
  const out = path.join(work, 'screens');
  fs.mkdirSync(out, { recursive: true });
  const browser = await launch();
  for (const shot of shots) {
    const context = await browser.newContext({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 2 });
    const page = await context.newPage();
    try {
      await page.goto(monitor + shot.route, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(4000);
      if (shot.click) { await page.click(shot.click, { timeout: 5000 }); await page.waitForTimeout(4000); }
      await page.screenshot({ path: path.join(out, `${shot.name}.png`) });
      console.log(shot.name, 'ok');
    } catch (error) {
      console.log(shot.name, 'FAILED', error.message.split('\n')[0]);
    }
    await context.close();
  }
  await browser.close();
})();
