# Feature Specification: Digital Jester Core - AI Punchline Guessing Loop

**Feature Branch**: `001-digital-jester-core`  
**Created**: 2025-12-12  
**Status**: Draft  
**Input**: User description: "The Digital Jester: PoJoker - A passive AI comedy application where a Generative AI plays a Medieval Court Jester, fetching jokes and attempting to guess punchlines before revealing them"

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Watch the Passive Comedy Loop (Priority: P1)

As a user, I want to start the application and watch an autonomous, never-ending comedy show where the AI Jester fetches jokes, attempts to guess punchlines, and reveals the actual answers—all without requiring my input.

**Why this priority**: This is the core value proposition of PoJoker. Without the passive performance loop, there is no product. It delivers the complete "lean-back" entertainment experience that differentiates this from a simple joke database.

**Independent Test**: Can be fully tested by launching the app, pressing "Start," and observing at least 3 complete joke cycles (Setup → AI Guess → Reveal) automatically playing in sequence.

**Acceptance Scenarios**:

1. **Given** the application is launched and idle, **When** the user presses the "Start" button, **Then** the performance loop begins automatically and continues without further user input
2. **Given** the performance loop is running, **When** a joke cycle completes (all 5 acts), **Then** a new joke is automatically fetched and the next cycle begins within 5 seconds
3. **Given** the loop is running, **When** the user presses the "Stop" button, **Then** the performance halts immediately and the application returns to an idle state
4. **Given** the AI is generating a punchline guess, **When** the AI exceeds the timeout threshold (e.g., 15 seconds), **Then** the system treats this as "stumped," displays a thematic message, and proceeds to the reveal

---

### User Story 2 - Experience the Medieval Dark Theme with Audio (Priority: P2)

As a user, I want the application to present a rich, immersive Medieval Dark theme with text-to-speech narration so that the experience feels like a theatrical court performance rather than a sterile utility.

**Why this priority**: The theme is what transforms a simple joke app into an "experience." While the loop (P1) proves the concept works, the theme is what makes users want to keep watching. It's essential for user retention and differentiation.

**Independent Test**: Can be tested by launching the app and verifying: dark color palette (blacks, charcoals, reds, yellows), Gothic/Fantasy typography, thematic loading animations, voiced narration of jokes, and audio cues (drum roll before punchline, trombone on failure).

**Acceptance Scenarios**:

1. **Given** the application is running, **When** any screen is displayed, **Then** the UI uses a dark color palette with red and yellow accents consistent with the Medieval Dark theme
2. **Given** a joke setup is displayed, **When** the system reads it aloud, **Then** TTS narration plays using a distinguished British male voice persona
3. **Given** the AI guess has been spoken, **When** the reveal phase begins, **Then** a drum roll audio cue plays before the actual punchline is delivered
4. **Given** the AI's guess was significantly wrong, **When** the reveal completes, **Then** a melancholic trombone slide audio cue plays

---

### User Story 3 - See AI Analysis and Joke Ratings (Priority: P3)

As a user, I want to see the AI's statistical analysis of each joke (Cleverness, Rudeness, Complexity, Difficulty) presented in an engaging way, so I can understand how the AI interprets humor and compare jokes over time.

**Why this priority**: Analysis adds depth and replay value to the passive loop. It transforms entertainment into a "Comedy Observatory" where users can observe AI behavior. However, the core loop works without it, making it lower priority than P1/P2.

**Independent Test**: Can be tested by completing one joke cycle and observing the Judgment phase displays: numeric scores (1-10) for each metric, visual charts, and personality-driven commentary from the Jester.

**Acceptance Scenarios**:

1. **Given** the actual punchline has been revealed, **When** the Judgment phase begins, **Then** the AI generates and displays scores for Cleverness, Rudeness, Complexity, and Difficulty (each 1-10)
2. **Given** scores are displayed, **When** the user views the analysis, **Then** the data is visualized using charts that contrast with the medieval aesthetic ("Cyber-Medieval" fusion)
3. **Given** the joke was particularly bad or offensive, **When** the AI commentary is generated, **Then** the Jester provides personality-driven remarks matching the content (e.g., "That was dry as a bone")

---

### User Story 4 - Browse Joke Leaderboards (Priority: P4)

As a user, I want to browse historical leaderboards of jokes rated by the AI (Most Clever, Most Rude, AI Triumphs) so I can revisit the best or most memorable moments.

**Why this priority**: Leaderboards provide long-term engagement and persistence, but require the core loop and analysis to generate data first. This is an enhancement that adds value after the foundational experience is solid.

**Independent Test**: Can be tested by running at least 10 joke cycles, then navigating to the Leaderboard view and verifying jokes are sorted correctly by each category.

**Acceptance Scenarios**:

1. **Given** the application has processed multiple jokes, **When** the user navigates to the Leaderboard view, **Then** they see a list of jokes sortable by Cleverness, Rudeness, or "AI Triumphs"
2. **Given** a joke is displayed on a leaderboard, **When** the user views it, **Then** they see the original setup, AI guess, actual punchline, and all ratings
3. **Given** the AI guessed a punchline word-for-word, **When** viewing the "AI Triumphs" leaderboard, **Then** that joke appears in the list marked as a perfect match

---

### User Story 5 - Graceful Handling of Sensitive Content (Priority: P5)

As a user, I want the application to handle jokes that trigger AI safety filters gracefully within the narrative context, so the experience remains immersive even when content is "forbidden."

**Why this priority**: While important for robustness, this is an edge case handler. The majority of jokes will process normally. This prevents crashes but is not core to the entertainment value.

**Independent Test**: Can be tested by manually injecting a joke known to trigger AI safety filters and observing the "Speechless Jester" protocol activates with a thematic message before transitioning to a new joke.

**Acceptance Scenarios**:

1. **Given** a joke's content triggers the AI's safety guardrails, **When** the AI refuses to process it, **Then** the system displays a thematic "Speechless Jester" message (e.g., "The Court forbids this tongue!")
2. **Given** the "Speechless Jester" state is active, **When** the pause completes, **Then** the system automatically fetches a new joke and resumes the performance loop

---

### User Story 6 - Verify System Health via Diagnostics (Priority: P6)

As an administrator or developer, I want a Diagnostics page that shows the health of all external dependencies (database, AI service, Joke API, secrets) so I can troubleshoot issues and monitor latency.

**Why this priority**: Diagnostics are operational infrastructure, not user-facing entertainment. Essential for production reliability but provides no direct entertainment value.

**Independent Test**: Can be tested by navigating to the DIAG page and observing connectivity status indicators for each external service, plus AI latency metrics.

**Acceptance Scenarios**:

1. **Given** the user navigates to the Diagnostics page, **When** the page loads, **Then** it displays health check status for: Database, Joke API, AI Service, and Secret Management
2. **Given** the AI service is connected, **When** viewing diagnostics, **Then** the current AI response latency is displayed
3. **Given** any external service is unreachable, **When** viewing diagnostics, **Then** that service shows a failed status with an error message

---

### Edge Cases

- What happens when the external Joke API is unavailable or rate-limited?
  - *System displays a thematic "The Royal Scroll is missing" error and retries after a delay*
- What happens when the AI service times out during punchline generation?
  - *System treats the Jester as "stumped," skips the guess, and proceeds to reveal*
- What happens when a joke is fetched that has already been shown (duplicate)?
  - *System tracks shown joke IDs in the session and re-fetches if duplicate detected*
- What happens when the user's device loses network connectivity mid-loop?
  - *System pauses gracefully with a "The Jester awaits the courier" message until reconnection*
- What happens when the user closes the browser during a joke cycle?
  - *In-progress joke state is not persisted; next session starts fresh*

## Requirements *(mandatory)*

### Functional Requirements

#### Core Loop (P1)
- **FR-001**: System MUST fetch two-part jokes (setup and delivery) from an external joke database
- **FR-027**: System MUST provide a "Safe Mode" toggle that excludes Dark and Rude joke categories when enabled (default: off)
- **FR-002**: System MUST display the joke setup and read it aloud using TTS
- **FR-003**: System MUST send only the joke setup to the AI and request a punchline prediction
- **FR-004**: System MUST enforce a timeout on AI punchline generation to prevent indefinite waiting
- **FR-005**: System MUST display and narrate the AI's guess distinctively marked as an attempt
- **FR-006**: System MUST trigger a drum roll audio cue before revealing the actual punchline
- **FR-007**: System MUST display and narrate the actual punchline from the joke source
- **FR-008**: System MUST automatically loop to fetch the next joke after completing all phases
- **FR-009**: Users MUST be able to start and stop the passive performance loop at any time

#### Theme & Audio (P2)
- **FR-010**: System MUST use a Medieval Dark color palette (blacks, charcoals, red and yellow accents)
- **FR-011**: System MUST use Gothic Blackletter fonts for headers and Fantasy Serif for body text
- **FR-012**: System MUST use thematic loading animations (no standard spinners)
- **FR-013**: System MUST use browser-native TTS (Web Speech API) with a British male voice for all narration
- **FR-014**: System MUST play contextual audio cues (drum roll for reveals, trombone for failures)

#### Analysis (P3)
- **FR-015**: System MUST generate numeric scores (1-10) for Cleverness, Rudeness, Complexity, and Difficulty after each joke
- **FR-016**: System MUST display scores using visual charts
- **FR-017**: System MUST generate personality-driven commentary matching the Jester persona

#### Persistence & Leaderboards (P4)
- **FR-018**: System MUST persist all processed jokes with their ratings to Azure Table Storage (partitioned by SessionId for device-local isolation)
- **FR-019**: System MUST support viewing jokes sorted by rating categories (Most Clever, Most Rude, etc.)
- **FR-020**: System MUST track and display "AI Triumphs" where the guess matched the actual punchline
- **FR-026**: System MUST NOT require user authentication; all data is device-local and personal

#### Error Handling (P5)
- **FR-021**: System MUST handle AI safety filter refusals with a thematic "Speechless Jester" message
- **FR-022**: System MUST display thematic error messages for all backend failures (no raw error codes)
- **FR-023**: System MUST log all errors centrally for debugging

#### Diagnostics (P6)
- **FR-024**: System MUST provide a Diagnostics page with health checks for all external services
- **FR-025**: System MUST display AI service latency on the Diagnostics page

### Key Entities

- **Joke**: A comedy unit consisting of a Setup (the premise), Delivery (the actual punchline), Category (e.g., Programming, General, Dark), and Source (external API identifier)
- **JokePerformance**: A record of one complete joke cycle, linking a Joke to the AI's GuessedPunchline, the JokeRating, PerformanceTimestamp, and whether it was an AI Triumph (guess matched delivery)
- **JokeRating**: Numeric scores (1-10) for Cleverness, Rudeness, Complexity, Difficulty, plus the AI's Commentary text
- **PerformanceSession**: A container for a sequence of JokePerformances within a single user session, tracking session start/end times and total jokes performed

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can watch at least 10 consecutive joke cycles without manual intervention after pressing "Start"
- **SC-002**: Each joke cycle (Setup → Guess → Reveal → Analysis) completes in under 60 seconds under normal conditions
- **SC-003**: AI punchline generation completes or times out within 15 seconds to maintain performance flow
- **SC-004**: TTS narration is audible and synchronized with displayed text for 100% of spoken content
- **SC-005**: 95% of users can identify the application theme as "Medieval" or "Fantasy" when surveyed
- **SC-006**: Leaderboard queries return results in under 2 seconds for datasets up to 1,000 jokes
- **SC-007**: System gracefully handles AI safety filter triggers without crashing or breaking immersion
- **SC-008**: Diagnostics page accurately reflects the actual health status of all external services

## Assumptions

- The external Joke API provides jokes in a predictable two-part format (setup + delivery) with category metadata
- Browser-native Web Speech API provides adequate British male voice quality for entertainment purposes
- The AI model can accept a joke setup and generate a reasonable punchline prediction within the timeout period
- Audio cues (drum roll, trombone) will be provided as static audio files or generated programmatically
- Users have devices capable of playing audio and displaying modern web content
- The application will be deployed as a web application accessible via modern browsers (Chromium-based)

## Clarifications

### Session 2025-12-12

- Q: Is this a single-user or multi-user application? → A: Single-user, device-local - All joke history stored locally per device; no authentication; leaderboards are personal
- Q: Should TTS be browser-native or external AI voice service? → A: Browser-native (Web Speech API) - Free, offline-capable, no API costs
- Q: Can users filter or control which joke categories appear? → A: Safe mode toggle - Single on/off switch to exclude Dark/Rude categories; default is off (all jokes)
