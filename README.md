# TaskBoard — Full-Stack Hiring Showcase

**Language:** [English](README.md) | [Türkçe](README_tr.md)

A Kanban-style task board: .NET 8 Web API + React 18 + EF Core 8.

## Stack

- **API:** .NET 8 (`net8.0`), EF Core 8 (SQL Server + InMemory fallback), JWT Bearer, Swagger (dev), open CORS
- **Tests:** xUnit + EF InMemory (8 tests)
- **Web:** Vite + React 18 + react-router-dom + axios
- **Deploy:** Dockerfiles + docker-compose (api + web + SQL Server)

## Quickstart (local, no Docker)

```bash
# API (http://localhost:5000, swagger in Development)
cd taskboard-api
dotnet run --urls http://localhost:5000

# Web (http://localhost:5173, /api proxied to :5000)
cd taskboard-web
npm install
npm run dev
```

With Docker:

```bash
docker compose up --build
# web :8080, api :5000, mssql :1433
```

## API overview

| Method | Route | Auth | Notes |
|---|---|---|---|
| POST | `/api/auth/register` | no | `{username,password,role}` → JWT with role claim |
| POST | `/api/auth/login` | no | verifies SHA256(password+salt) |
| GET/POST | `/api/projects` | JWT | |
| GET | `/api/projects/{id}` | JWT | includes tasks |
| GET | `/api/projects/{id}/tasks?status=` | JWT | filter Todo/InProgress/Done |
| POST | `/api/projects/{id}/tasks` | JWT | creates as Todo, records creator |
| POST | `/api/tasks/{id}/move` | JWT | `{status}`; Done is terminal → 400 |
| GET/POST | `/api/tasks/{id}/comments` | JWT | |
| DELETE | `/api/tasks/{id}` | JWT | Manager or creator only, else 403 |

## Security note

Passwords are stored as `SHA256(password + per-user salt)` (demo-grade; use PBKDF2/Argon2/bcrypt in production). The JWT key in `appsettings.json` / compose files is a **dev placeholder** — no real secrets are committed.

## Verify

```bash
dotnet test taskboard-api.Tests/taskboard-api.Tests.csproj
npm --prefix taskboard-web run build
```
