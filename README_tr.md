# TaskBoard — Full-Stack İşe Alım Vitrini

**Dil:** [English](README.md) | [Türkçe](README_tr.md)

Kanban tarzı görev panosu: .NET 8 Web API + React 18 + EF Core 8.

## Teknolojiler

- **API:** .NET 8 (`net8.0`), EF Core 8 (SQL Server + InMemory yedeği), JWT Bearer, Swagger (dev), açık CORS
- **Test:** xUnit + EF InMemory (8 test)
- **Web:** Vite + React 18 + react-router-dom + axios
- **Dağıtım:** Dockerfile + docker-compose (api + web + SQL Server)

## Hızlı başlangıç (lokal, Docker'sız)

```bash
# API (http://localhost:5000, Development'ta swagger)
cd taskboard-api
dotnet run --urls http://localhost:5000

# Web (http://localhost:5173, /api -> :5000 proxy)
cd taskboard-web
npm install
npm run dev
```

Docker ile:

```bash
docker compose up --build
# web :8080, api :5000, mssql :1433
```

## API özeti

| Metot | Rota | Yetki | Not |
|---|---|---|---|
| POST | `/api/auth/register` | yok | `{username,password,role}` → rol claim'li JWT |
| POST | `/api/auth/login` | yok | SHA256(parola+salt) doğrular |
| GET/POST | `/api/projects` | JWT | |
| GET | `/api/projects/{id}` | JWT | görevlerle birlikte |
| GET | `/api/projects/{id}/tasks?status=` | JWT | Todo/InProgress/Done filtresi |
| POST | `/api/projects/{id}/tasks` | JWT | Todo olarak açılır, oluşturan kaydedilir |
| POST | `/api/tasks/{id}/move` | JWT | `{status}`; Done terminaldir → 400 |
| GET/POST | `/api/tasks/{id}/comments` | JWT | |
| DELETE | `/api/tasks/{id}` | JWT | Yalnızca Manager veya oluşturan, yoksa 403 |

## Güvenlik notu

Parolalar `SHA256(parola + kullanıcıya özel salt)` ile saklanır (demo seviyesi; üretimde PBKDF2/Argon2/bcrypt kullanın). `appsettings.json` / compose dosyalarındaki JWT anahtarı **geliştirme amaçlı yer tutucudur** — gerçek secret commitlenmedi.

## Doğrulama

```bash
dotnet test taskboard-api.Tests/taskboard-api.Tests.csproj
npm --prefix taskboard-web run build
```
