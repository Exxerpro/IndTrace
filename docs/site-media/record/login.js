// Signs in as the demo user and saves the session to $MEDIA_WORK/auth.json for the other scripts.
const { monitor, authFile, viewport, launch } = require('./common');

(async () => {
  const browser = await launch();
  const context = await browser.newContext({ viewport });
  const page = await context.newPage();
  await page.goto(`${monitor}/Account/Login`, { waitUntil: 'networkidle' });
  await page.fill('input[name="Input.Email"]', process.env.DEMO_USER || 'demo@example.com');
  await page.fill('input[name="Input.Password"]', process.env.DEMO_PASSWORD);
  await Promise.all([
    page.waitForNavigation({ waitUntil: 'networkidle' }).catch(() => {}),
    page.click('button:has-text("LOG IN")'),
  ]);
  // The redirect after a successful sign-in happens client-side, so check for the session cookie, not the URL.
  await page.waitForTimeout(2000);
  const cookies = await context.cookies();
  if (!cookies.some(cookie => cookie.name === '.AspNetCore.Identity.Application')) {
    throw new Error('Sign-in failed; check DEMO_USER and DEMO_PASSWORD.');
  }
  await context.storageState({ path: authFile });
  console.log(`signed in; session saved to ${authFile}`);
  await browser.close();
})();
