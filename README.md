# ClosetAI
AI Wardrobe helper

- `mobile/` — Expo / React Native app
- `backend/` — ASP.NET Core API (`Closet.slnx`); `src/` is one project with `Application`, `Domain` and `Infrastructure` folders
- `docs/api-contract.md` — the API contract the backend implements

## Running the backend

### Locally with SQLite (no Docker)

```
cd backend/src
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://localhost:5080
```

In Development the database (`closet.db`) is created and seeded from `Infrastructure/Seed/demo-closet.json` on first run. Swagger UI is at http://localhost:5080/swagger and the OpenAPI document at `/openapi/v1.json`. `POST /api/demo/reset` reloads the demo data.

### With Docker (SQL Server)

```
cp .env.example .env
docker compose up --build
```

The API is on http://localhost:5080 (`API_PORT` in `.env`). SQL Server runs as `linux/amd64`, so it is emulated on Apple Silicon.

### Claude

Without an API key the backend uses a deterministic rules stylist and a fake photo tagger. To use Claude (`claude-sonnet-5-5`), provide a key through user-secrets or the environment, never `appsettings.json`:

```
cd backend/src
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-..."
```

or set `Anthropic__ApiKey` (Docker: `ANTHROPIC_API_KEY` in `.env`).

### Tests

```
cd backend
dotnet test tests/Closet.Tests --settings tests/Closet.Tests/coverage.runsettings --collect:"XPlat Code Coverage"
```
