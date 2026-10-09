// Video 2, "Define a routing": create a product, connect its five stations, save, and show the new route on
// the Configuration page. Needs a signed-in session (login.js). The product TL-2040 must not exist yet;
// demo-db/reset-media.sql removes it after a previous run.
const { monitor, launch, pause, recordingContext } = require('./common');

(async () => {
  const browser = await launch();
  const context = await recordingContext(browser, 'define-routing', { signedIn: true });
  const page = await context.newPage();
  page.on('pageerror', error => console.log('pageerror', error.message.slice(0, 200)));
  const type = async (label, text) => {
    const input = page.getByLabel(label, { exact: true });
    await input.click();
    await input.fill('');
    await input.pressSequentially(text, { delay: 45 });
  };

  await page.goto(`${monitor}/Products`, { waitUntil: 'networkidle' });
  await pause(4000);
  await page.click('button:has-text("ADD PRODUCT")');
  await pause(1500);
  await type('Product Name', 'Tail lamp housing');
  await type('Product PartNumber', 'TL-2040');
  await type('Alias Part Number', 'TL2040');
  await type('Customer Part Number', 'TLH-2040-A');
  await type('Description', 'Rear tail lamp housing, five-station line');
  await page.getByLabel('Customer', { exact: true }).click();
  await pause(800);
  await page.locator('.mud-popover-open .mud-list-item').filter({ hasText: 'Apex Lighting' }).first().click();
  await pause(800);
  await page.click('button:has-text("SUBMIT")');
  await pause(2500);
  await page.click('button:has-text("CONFIGURE WORKFLOW")');
  await pause(2500);

  for (const machine of [100, 200, 300, 400, 500]) {
    await page.click(`[data-testid="add-machine-${machine}"]`);
    await pause(900);
  }
  // Connect each station to the next one, then mark the first and last stations.
  for (let i = 0; i < 4; i++) {
    await page.click(`[data-testid="add-branch-${i}"]`);
    await pause(500);
    await page.selectOption(`[data-testid="edge-target-${i}-0"]`, String((i + 2) * 100));
    await pause(700);
  }
  await page.selectOption('[data-testid="node-role-0"]', { label: 'Initial' });
  await pause(600);
  await page.selectOption('[data-testid="node-role-4"]', { label: 'Final' });
  await pause(1500);
  await page.click('[data-testid="submit"]');
  await pause(2500);
  await page.locator('button:has-text("SAVE")').click();
  await pause(4000);
  console.log('messages', await page.locator('.mud-snackbar, .mud-alert').allInnerTexts());

  await page.goto(`${monitor}/configuration`, { waitUntil: 'networkidle' });
  await pause(2000);
  await page.mouse.wheel(0, 2000);
  await pause(4000);
  await context.close();
  await browser.close();
})();
