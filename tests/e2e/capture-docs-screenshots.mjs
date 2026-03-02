import { chromium } from 'playwright';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const baseURL = 'http://localhost:5123';
const screenshotDir = path.join(__dirname, 'screenshots');

// Create screenshots directory if it doesn't exist
if (!fs.existsSync(screenshotDir)) {
  fs.mkdirSync(screenshotDir, { recursive: true });
}

async function takeScreenshots() {
  const browser = await chromium.launch();
  
  try {
    // Home page
    console.log('Capturing Home page...');
    const homePage = await browser.newPage();
    await homePage.goto(`${baseURL}/`, { waitUntil: 'domcontentloaded' });
    await homePage.waitForTimeout(2000);
    await homePage.screenshot({ path: path.join(screenshotDir, '01-home.png'), fullPage: true });
    console.log('✓ Home page screenshot saved');
    await homePage.close();

    // Leaderboard page
    console.log('Capturing Leaderboard page...');
    const leaderboardPage = await browser.newPage();
    await leaderboardPage.goto(`${baseURL}/leaderboard`, { waitUntil: 'domcontentloaded' });
    await leaderboardPage.waitForTimeout(2000);
    await leaderboardPage.screenshot({ path: path.join(screenshotDir, '02-leaderboard.png'), fullPage: true });
    console.log('✓ Leaderboard page screenshot saved');
    await leaderboardPage.close();

    // Diagnostics page
    console.log('Capturing Diagnostics page...');
    const diagPage = await browser.newPage();
    await diagPage.goto(`${baseURL}/diag`, { waitUntil: 'domcontentloaded' });
    await diagPage.waitForTimeout(2000);
    await diagPage.screenshot({ path: path.join(screenshotDir, '03-diagnostics.png'), fullPage: true });
    console.log('✓ Diagnostics page screenshot saved');
    await diagPage.close();

    console.log('\n✓ All screenshots captured successfully!');
    console.log('Files:', fs.readdirSync(screenshotDir));
  } catch (error) {
    console.error('Error taking screenshots:', error.message);
    process.exit(1);
  } finally {
    await browser.close();
  }
}

takeScreenshots();
