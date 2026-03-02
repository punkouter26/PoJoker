# Product Specification: Po.Joker - The Digital Jester

## Overview

**Po.Joker** is a passive AI comedy application that autonomously fetches jokes, predicts punchlines using AI, and provides entertaining analysis—all without requiring user interaction. It's a fully-automated comedy show running on Azure with a medieval dark-themed UI.

## Key Features

### Core Features
- **Autonomous Comedy Loop**: The application continuously fetches jokes and displays them in a never-ending show
- **AI Punchline Prediction**: Uses Azure OpenAI (GPT-4.1-nano) to predict the punchline before revealing the actual one
- **Joke Rating System**: AI analyzes jokes across 5 dimensions:
  - Cleverness (how witty)
  - Rudeness (NSFW factor)
  - Complexity (conceptual depth)
  - Difficulty (how "hard" the punchline is)
  - Commentary (human-friendly explanation)
- **Medieval Dark Theme**: Immersive Gothic UI with custom fonts and dark backgrounds
- **Text-to-Speech**: British male voice narration of jokes
- **Audio Effects**: 
  - Drum roll before punchline reveal
  - Trombone sound effect for prediction failures
- **Leaderboard System**: Browse historical jokes sorted by cleverness, rudeness, complexity, or difficulty
- **Diagnostics Dashboard**: Health checks for all external dependencies

## Business Logic

### The Comedy Loop
1. **Fetch**: Request a random two-part joke from JokeAPI
2. **Display Setup**: Show the joke setup (first part)
3. **Drama Pause**: Slight delay for comedic effect
4. **AI Prediction**: Azure OpenAI predicts the punchline
5. **Display Prediction**: Show AI's prediction with confidence score
6. **Reveal**: Display the true punchline
7. **Audio Narration**: Text-to-Speech narrates the joke
8. **Rate**: AI analyzes and rates the joke across dimensions
9. **Store**: Persist in Azure Table Storage for leaderboard
10. **Repeat**: Loop infinitely

### Safe Mode
- Toggle between safe (filtered) and adult jokes
- JokeAPI respects the safe mode parameter
- Stored jokes maintain their safety classification

## Success Metrics

### Performance Metrics
- **Uptime**: 99.5% availability target
- **Response Time**: <500ms for each API call to external services
- **Cold Start**: <3 seconds from page load to first joke
- **AI Latency**: <2 seconds for punchline prediction

### User Experience Metrics
- **Page Load Time**: <2 seconds
- **Frame Rate**: 60 FPS for smooth animations
- **Audio Sync**: <100ms latency between display and narration

### Business Metrics
- **Autonomous Runtime**: Continuously run without user input
- **Data Persistence**: 100% of jokes stored for leaderboard
- **AI Accuracy**: Track prediction vs. actual punchline similarity
- **Available Jokes**: Maintain diverse joke pool (multiple categories)

## Technical Constraints

### Infrastructure
- Runs on Azure with Managed Identity authentication
- Uses Azurite for local development (Docker)
- Deployable as standalone Blazor Web App
- Health checks against all dependencies

### Resilience
- Polly retry policies for transient failures
- Exponential backoff on external API calls
- Fallback UI when AI service is unavailable
- Connection pooling for Table Storage

### Data Retention
- Unlimited joke history in leaderboard (currently)
- Automatic de-duplication via JokeAPI exclusion list
- No PII collected from users

## Scope Boundaries

### In Scope
- Comedy loop automation
- AI-powered analysis and rating
- Leaderboard persistence
- Health diagnostics
- Text-to-speech narration
- Medieval Dark Theme UI

### Out of Scope (Future)
- User accounts/authentication
- Custom joke submissions
- Real-time multiplayer competition
- Mobile-specific UI
- Advanced ML models (fine-tuned for humor)
- Streaming video feeds

## Success Criteria

1. ✅ Application runs autonomously without user interaction
2. ✅ All jokes retrieved successfully from JokeAPI
3. ✅ AI predictions render within 2 seconds
4. ✅ Leaderboard correctly sorts jokes by rating dimension
5. ✅ Diagnostics page reflects accurate health status
6. ✅ Audio narration plays without delays
7. ✅ Medieval UI renders consistently across browsers
8. ✅ Zero crashes during extended runs (24+ hours)
9. ✅ Graceful degradation when AI service is down
10. ✅ All secrets managed via Azure Key Vault
