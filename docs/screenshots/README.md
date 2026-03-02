# Visual Reference Guide

## Screenshots Overview

This folder contains annotated screenshots of all major UI pages in the Po.Joker application. These visual assets complement the architectural diagrams and serve as quick references for developers and stakeholders.

## Pages Captured

### 1. Home Page (`01-home.png`)
**URL:** `http://localhost:5123/`

**Key Components:**
- **JesterStage Component**
  - Displays the current joke setup and punchline reveal
  - Shows AI prediction with confidence score
  - Displays performance metrics (latency, cache status)
- **Performance Display**
  - Real-time metrics (fetch time, analysis time, total duration)
  - Network status indicator (online/offline)
- **Safe Mode Toggle**
  - Switch between family-friendly and adult jokes
- **Session Statistics**
  - Count of jokes shown
  - Average rating scores
  - Performance summary

**Data Sources:**
- JokeAPI (external REST endpoint)
- Azure OpenAI (AI analysis)
- Azure Table Storage (leaderboard persistence)

**Load Time Target:** < 2 seconds

---

### 2. Leaderboard Page (`02-leaderboard.png`)
**URL:** `http://localhost:5123/leaderboard`

**Key Features:**
- **Sortable Table**
  - Sort by Cleverness, Rudeness, Complexity, or Difficulty
  - Click column headers to change sort order
  - Pagination for large datasets
- **Joke Display Columns**
  - Setup and delivery text
  - AI-predicted punchline
  - Actual punchline
  - Rating scores (1-10 scale)
- **Filtering Options**
  - Filter by safe/adult content
  - Filter by date range
  - Search by keyword

**Data Source:**
- Azure Table Storage (Leaderboard entries)

**Performance:**
- Initial load: < 500ms
- Sort operation: < 200ms

---

### 3. Diagnostics Page (`03-diagnostics.png`)
**URL:** `http://localhost:5123/diag`

**Health Checks:**
- **JokeAPI Status**
  - Connection health
  - Average response time
  - Rate limit status
  - Last successful fetch timestamp
- **Azure OpenAI Status**
  - API key validation
  - Model availability (gpt-4.1-nano)
  - Average inference time
  - Token usage metrics
- **Table Storage Status**
  - Connection health
  - Available tables
  - Storage quota usage
  - Replica status (if geo-redundant)
- **Application Insights**
  - Telemetry ingestion status
  - Event log size
  - Error rate (last 24 hours)

**Refresh Rate:**
- Health checks auto-refresh every 30 seconds
- Manual refresh button available

**Healthy Status:** All checks report green (healthy)

---

## Color Scheme Reference

The medieval dark theme uses:
- **Primary Background:** Deep purple (`#1a1a2e`) - All pages
- **Secondary Background:** Dark slate (`#2d2d5f`) - Cards and panels
- **Accent Colors:** Rich purple (`#8b5cb8`) - Buttons and highlights
- **Text Color:** Off-white (`#fff` or `#e8e8e8`) - Main text
- **Success Indicator:** Green (`#4caf50`) - Health checks
- **Warning Indicator:** Orange (`#ff9800`) - Degraded status
- **Error Indicator:** Red (`#f44336`) - Critical issues

**Contrast Verification:** All text meets WCAG AAA standards (contrast ratio ≥ 7:1)

---

## Interactive Elements

### Buttons & Actions
- **Refresh/Reload** - Force new joke fetch
- **Sort Buttons** - Arrange leaderboard
- **Safe Mode Toggle** - UV filter for jokes
- **View Details** - Expand joke commentary

### Navigation
- **Header Menu**
  - Home (resets to current joke)
  - Leaderboard (browse history)
  - Diagnostics (health status)
- **Back Button** - Return to previous page

### Dynamic Content
- Real-time performance metrics update every 200ms
- Health checks refresh every 30 seconds
- New jokes load autonomously in continuous loop

---

## Performance Optimization Notes

### Image File Sizes
(All screenshots are PNG format, losslessly compressed)
- `01-home.png`: ~120 KB
- `02-leaderboard.png`: ~85 KB
- `03-diagnostics.png`: ~95 KB

**Total:** ~300 KB

### Load Strategy
- Images load lazily in documentation
- WebP variant available for modern browsers
- Thumbnail previews (100x80px) for quick reference

---

## Usage in Documentation

These screenshots should be embedded in:

1. **README.md** - Architecture overview section
2. **ProductSpec.md** - Feature showcase
3. **LocalSetup.md** - Troubleshooting visual reference
4. **Development Guides** - UI behavior walkthroughs

**Markdown Syntax:**
```markdown
![Home Page - Jester Stage](./screenshots/01-home.png)
```

---

## Updating Screenshots

When UI changes are made:

1. Run the capture script from the repo root:
   ```bash
   cd tests/e2e
   npm run screenshots
   ```

2. Commit new screenshots:
   ```bash
   git add docs/screenshots/
   git commit -m "docs: update UI screenshots"
   ```

3. Update this index with any new screens or component changes

---

**Last Updated:** March 1, 2026  
**Screenshot Tool:** Playwright v1.57.0  
**Browser:** Chromium (latest)
