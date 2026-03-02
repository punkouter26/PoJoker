# Po.Joker Documentation Index

Welcome to the comprehensive documentation for **Po.Joker** - The Digital Jester. This index provides a roadmap to all available resources.

## 📚 Quick Navigation

### For New Developers (Day 1)
Start here if you're setting up the project for the first time:
1. **[LocalSetup.md](LocalSetup.md)** – Installation, configuration, Docker Compose, troubleshooting
2. **[Architecture.mmd](Architecture.mmd)** – System overview diagram (5-min read)
3. **Screenshots** – Visual tour of the app ([screenshots/](screenshots/))

### For Business & Product People
Understand what Po.Joker does and why:
1. **[ProductSpec.md](ProductSpec.md)** – Features, business logic, success metrics
2. **[ApplicationFlow.mmd](ApplicationFlow.mmd)** – User journey and workflow
3. **[screenshots/](screenshots/)** – See the UI in action

### For DevOps & Infrastructure
Manage deployment, monitoring, and scaling:
1. **[DevOps.md](DevOps.md)** – CI/CD pipeline, secrets, environment config
2. **[Architecture.mmd](Architecture.mmd)** – Azure resource topology

### For Architects & Lead Developers
Deep dive into system design:
1. **[Architecture.mmd](Architecture.mmd)** – C4 Context Diagram
2. **[ComponentMap.mmd](ComponentMap.mmd)** – Frontend + Backend component tree
3. **[DataModel.mmd](DataModel.mmd)** – Entity Relationship Diagram
4. **[DataPipeline.mmd](DataPipeline.mmd)** – CRUD operations and data flow

---

## 📖 Documentation Files

### Markdown Files (Prose + Context)
| File | Purpose | Audience | Read Time |
|------|---------|----------|-----------|
| [**ProductSpec.md**](ProductSpec.md) | Business requirements, features, success metrics | Product, Engineering | 15 min |
| [**DevOps.md**](DevOps.md) | Deployment pipeline, CI/CD, secrets management | DevOps, SRE, Ops | 20 min |
| [**LocalSetup.md**](LocalSetup.md) | Day 1 onboarding, Docker Compose, troubleshooting | New developers | 10 min |

### Mermaid Diagram Files (Raw Diagrams - View in Mermaid Editor)
| File | Diagram Type | Purpose | Audience |
|------|---|---------|----------|
| **Architecture.mmd** | C4Context | System topology with Azure services | Architects, DevOps |
| **ApplicationFlow.mmd** | Flowchart | User journey and comedy loop workflow | Everyone |
| **DataModel.mmd** | Entity Relationship | Database schema and relationships | Backend engineers |
| **ComponentMap.mmd** | Flowchart | Frontend/Backend component hierarchy | Frontend & Full-stack |
| **DataPipeline.mmd** | Flowchart | CRUD operations and data transformations | Data engineers |

### Screenshots & Visual Assets
Captured user interface pages:
- **[screenshots/](screenshots/)** – UI screenshots with descriptions
  - `01-home.png` – Jester Stage with comedy loop
  - `02-leaderboard.png` – Historical joke rankings
  - `03-diagnostics.png` – Health check dashboard
  - `README.md` – Visual reference guide

---

## 📊 Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│ Po.Joker: Autonomous AI Comedy Application          │
└─────────────────────────────────────────────────────┘
          │
    ┌─────┴──────────────────────┐
    │   Blazor Web App (.NET 10) │
    │  • Interactive UI          │
    │  • Real-time updates       │
    │  • Performance metrics     │
    └─────┬──────────────────────┘
          │
    ┌─────┼────────────────────┬──────────────┐
    │     │                    │              │
    ▼     ▼                    ▼              ▼
 JokeAPI  Azure OpenAI    Table Storage   Key Vault
 Fetch    (GPT-4 Analysis) (Leaderboard) (Secrets)
 Jokes    Responses       Persistence    Config
```

See [Architecture.mmd](Architecture.mmd) for detailed C4 diagram.

---

## 🔄 Key Workflows

### The Comedy Loop (Autonomous)
1. **Fetch** – JokeAPI provides multi-part joke
2. **Display Setup** – Show joke setup to user
3. **Predict** – Azure OpenAI generates punchline prediction
4. **Reveal** – Display true punchline
5. **Rate** – AI analyzes joke across 5 dimensions
6. **Store** – Save to Table Storage for leaderboard
7. **Repeat** – Loop infinitely

See [ApplicationFlow.mmd](ApplicationFlow.mmd) and [DataPipeline.mmd](DataPipeline.mmd).

### Data Persistence
- All jokes stored in Azure Table Storage
- Accessible via Leaderboard page
- Sortable by rating dimension (Cleverness, Rudeness, Complexity, Difficulty)
- Queryable in real-time

See [DataModel.mmd](DataModel.mmd).

### Deployment Pipeline
```
Code Push → Build & Test → Container Build → Deploy Infrastructure → Deploy App → Smoke Tests → ✅ Production
```

See [DevOps.md](DevOps.md#cicd-pipeline).

---

## ✅ Documentation Quality Assurance

### Completeness
- ✅ All major features documented
- ✅ Architecture diagrams created
- ✅ Deployment procedures documented
- ✅ Local dev setup guide complete
- ✅ Visual assets (screenshots) captured
- ✅ API endpoints mapped
- ✅ Environment variables listed

### Mermaid Compliance
- ✅ All diagrams are valid Mermaid syntax
- ✅ Color contrast meets WCAG AAA (≥7:1)
- ✅ Subgraph labels properly quoted
- ✅ Node IDs unique and hyphenated
- ✅ No deprecated diagram types
- ✅ Style directives include fill, stroke, color

### Accuracy
- ✅ Based on actual codebase patterns
- ✅ Reflects current Azure service usage
- ✅ Deployment strategy matches Bicep templates
- ✅ API endpoints match Program.cs registration
- ✅ Configuration reflects appsettings.json

---

## 🔗 External References

### JokeAPI
- **Endpoint:** https://v2.jokeapi.dev/joke/
- **Docs:** https://jokeapi.dev/
- **Rate Limit:** 120 requests/minute per IP

### Azure Services
- **Azure OpenAI:** https://azure.microsoft.com/products/ai-services/openai-service/
- **Table Storage:** https://azure.microsoft.com/products/storage/tables/
- **Key Vault:** https://azure.microsoft.com/products/key-vault/
- **App Service:** https://azure.microsoft.com/products/app-service/

### Development Tools
- **Mermaid Diagrams:** https://mermaid.live/
- **.NET 10 SDK:** https://dotnet.microsoft.com/download/dotnet/10.0
- **Docker Desktop:** https://www.docker.com/products/docker-desktop
- **Azure CLI:** https://docs.microsoft.com/cli/azure/

---

## 📝 How to Use This Documentation

### Reading Flow by Role

**New Developer:**
```
LocalSetup.md 
    → Architecture.mmd 
    → Screenshots 
    → Run the app locally
```

**Feature Request:**
```
ProductSpec.md 
    → ApplicationFlow.mmd 
    → DataModel.mmd 
    → Component code
```

**Production Issue:**
```
DevOps.md 
    → Architecture.mmd 
    → Check health endpoint 
    → View diagnostics page
```

**Architectural Review:**
```
Architecture.mmd 
    → ComponentMap.mmd 
    → DataModel.mmd 
    → DataPipeline.mmd
```

---

## 💡 Tips for Maintaining Documentation

1. **Update Diagrams** when architecture changes (especially `Architecture.mmd`)
2. **Screenshot UI** after UI refactors using `capture-screenshots.mjs`
3. **Review ProductSpec.md** quarterly against actual product behavior
4. **Validate DevOps.md** before major deployments
5. **Keep LocalSetup.md** in sync with actual setup steps

---

## 📞 Support & Questions

- **Architecture questions?** → Check [Architecture.mmd](Architecture.mmd) + [ComponentMap.mmd](ComponentMap.mmd)
- **Setup issues?** → See [LocalSetup.md](LocalSetup.md#troubleshooting)
- **Deployment help?** → Refer to [DevOps.md](DevOps.md)
- **Business questions?** → Read [ProductSpec.md](ProductSpec.md)

---

**Last Updated:** March 1, 2026  
**Maintainers:** Engineering & DevOps Team  
**Review Cycle:** Quarterly  
**Status:** ✅ Complete & Verified
