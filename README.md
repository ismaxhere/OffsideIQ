# ⚽ Offside IQ

> Football analytics and match insight platform. Track matches, analyze team performance, and generate rule-based insights.

---

## Architecture

```
OffsideIQ/
├── src/
│   ├── OffsideIQ.Core/           # Entities, Interfaces, DTOs, Enums
│   ├── OffsideIQ.Infrastructure/ # EF Core DbContext + Repositories
│   ├── OffsideIQ.Application/    # Business logic services + Insight engine
│   └── OffsideIQ.API/            # ASP.NET Core Controllers, Middleware
├── docs/
│   └── schema.sql                # PostgreSQL schema reference
├── frontend/                     # React frontend (Vite)
├── docker-compose.yml
├── Dockerfile
└── OffsideIQ.sln
```

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| PostgreSQL | 14+ |
| Node.js | 18+ |
| Docker (optional) | 24+ |

---

## Quick Start (Local)

### 1. Clone and configure

```bash
git clone https://github.com/yourorg/offsideiq
cd offsideiq
```

Edit `src/OffsideIQ.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=offsideiq;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Secret": "YOUR_SUPER_SECRET_KEY_MINIMUM_32_CHARACTERS_LONG",
    "Issuer": "OffsideIQ",
    "Audience": "OffsideIQ"
  }
}
```

### 2. Run the API

```bash
cd src/OffsideIQ.API
dotnet restore
dotnet ef database update          # Runs migrations automatically
dotnet run
```

API runs at: `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

### 3. Run the Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend runs at: `http://localhost:5173`

---

## Quick Start (Docker)

```bash
docker-compose up --build
```

- API: `http://localhost:5000`
- PostgreSQL: `localhost:5432`

---

## EF Core Migrations

```bash
# From solution root
dotnet ef migrations add <MigrationName> --project src/OffsideIQ.Infrastructure --startup-project src/OffsideIQ.API
dotnet ef database update --project src/OffsideIQ.Infrastructure --startup-project src/OffsideIQ.API
```

---

## API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, receive JWT |

### Teams
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/teams` | List your teams |
| GET | `/api/teams/{id}` | Get team by ID |
| GET | `/api/teams/{id}/form` | Last 5 match form |
| POST | `/api/teams` | Create team |
| PUT | `/api/teams/{id}` | Update team |
| DELETE | `/api/teams/{id}` | Delete team |

### Matches
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/matches?page=1&pageSize=20` | Paginated match list |
| GET | `/api/matches/recent` | Latest 10 matches |
| GET | `/api/matches/{id}` | Match detail |
| GET | `/api/matches/{id}/insights` | Rule-based match insights |
| POST | `/api/matches` | Create match (with optional stats) |
| PUT | `/api/matches/{id}` | Update match |
| DELETE | `/api/matches/{id}` | Delete match |
| PUT | `/api/matches/{id}/stats` | Upsert match stats |
| GET | `/api/matches/{id}/notes` | Get match notes |
| POST | `/api/matches/{id}/notes` | Add note |
| DELETE | `/api/matches/{matchId}/notes/{noteId}` | Delete note |

### Head-to-Head
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/h2h/{teamAId}/{teamBId}` | H2H record & stats |

### Insights
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/insights` | Global insights |
| GET | `/api/insights/teams/{teamId}` | Team-specific insights |
| GET | `/api/insights/predict?homeTeamId=&awayTeamId=` | Match prediction |

### Dashboard
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard` | Full dashboard payload |

### Players
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/players/team/{teamId}` | Players by team |
| GET | `/api/players/{id}` | Player detail |
| POST | `/api/players` | Create player |
| POST | `/api/players/matches/{matchId}/ratings` | Rate player in match |

---

## Insight Engine

The rule-based insight engine (`InsightService`) analyzes match data and generates contextual insights:

| Type | Trigger | Example |
|------|---------|---------|
| `possession` | ≥65% possession | "Arsenal dominated possession at 68%" |
| `scoring` | 0 or 5+ goals | "Goal fest — 6 goals in total" |
| `streak` | 3+ consecutive wins | "On Fire 🔥 — 4-match winning streak" |
| `form` | 4+ wins/losses in 5 | "Excellent form — 4W in last 5" |
| `defense` | <0.5 or ≥2.5 avg conceded | "Solid backline — 0.4 goals/game" |
| `discipline` | Red cards, 6+ yellows | "Feisty encounter — 7 yellow cards" |
| `xg` | High xG, low goals | "Underperformed xG — 2.8 xG, 1 goal" |
| `prediction` | Rule-based probability | "Home Win 58% — Away Win 28%" |

---

## Environment Variables (Production)

```env
ConnectionStrings__DefaultConnection=Host=...;Database=offsideiq;...
Jwt__Secret=<min 32 char random string>
Jwt__Issuer=OffsideIQ
Jwt__Audience=OffsideIQ
ASPNETCORE_ENVIRONMENT=Production
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| API | ASP.NET Core 8, C# 12 |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 |
| Auth | JWT Bearer Tokens |
| Passwords | BCrypt.Net |
| Docs | Swashbuckle / Swagger UI |
| Frontend | React 18 + Vite |
| Container | Docker + Docker Compose |
