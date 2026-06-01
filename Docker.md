# MXHRM Docker Guide

เอกสารนี้อธิบายวิธีรัน MXHRM ด้วย Docker Compose สำหรับ local development และ containerized deployment baseline.

เป้าหมายของชุด Docker นี้คือทำให้ระบบรันได้ครบทั้ง frontend, backend และ infrastructure สำคัญในคำสั่งเดียว.

```text
Browser
   ↓ http://localhost:4200
Web Container (Nginx + Angular)
   ├── /              → Angular static files
   ├── /api/...       → API Container
   └── /hubs/...      → SignalR Hub
API Container (ASP.NET Core)
   ├── SQL Server     → sqlserver:1433
   ├── Redis          → redis:6379
   └── Seq            → seq:80
```

---

## Services

| Service | Container | Purpose | Host URL / Port |
|---|---|---|---|
| `web` | `mxhrm-web` | Angular production build served by Nginx | `http://localhost:4200` |
| `api` | `mxhrm-api` | ASP.NET Core Web API | `http://localhost:8080` |
| `sqlserver` | `mxhrm-sqlserver` | Azure SQL Edge / SQL Server-compatible database | `localhost,1433` |
| `redis` | `mxhrm-redis` | Redis cache | `localhost:6379` |
| `seq` | `mxhrm-seq` | Structured log viewer | `http://localhost:5341` |

---

## Important Files

| File | Purpose |
|---|---|
| `docker-compose.yml` | Defines all containers, profiles, ports, environment variables, volumes, and healthchecks |
| `.env` | Local secrets and environment values used by Docker Compose |
| `.env.example` | Safe template for required environment values |
| `.env.production.example` | Production-style template for deployment secrets and image tag |
| `Makefile` | Short commands for daily Docker workflow |
| `src/MXHRM.Api/Dockerfile` | Builds and runs the ASP.NET Core API container |
| `client/mxhrm-web/Dockerfile` | Builds Angular and serves it with Nginx |
| `client/mxhrm-web/nginx.conf` | Nginx SPA fallback, API proxy, SignalR proxy, gzip, and cache rules |
| `src/MXHRM.Api/appsettings.Container.json` | Container-specific API configuration |

---

## Prerequisites

Install these before running the stack:

```text
Docker Desktop
Docker Compose v2
.NET SDK 10
Node.js 24 or compatible version used by the Angular project
```

Check Docker:

```bash
docker --version
docker compose version
```

Check project tools:

```bash
dotnet --version
node --version
npm --version
```

---

## Environment Setup

Create `.env` from `.env.example`.

```bash
cp .env.example .env
```

Example local values:

```env
MSSQL_SA_PASSWORD=P@ssw0rd
MXHRM_DB_NAME=MXHRM

JWT_ISSUER=MXHRM.Api
JWT_AUDIENCE=MXHRM.Web
JWT_SECRET_KEY=THIS_IS_DEV_SECRET_KEY_CHANGE_ME_1234567890
JWT_ACCESS_TOKEN_MINUTES=15

SEQ_ADMIN_USERNAME=admin
SEQ_ADMIN_PASSWORD=P@ssw0rd
```

Do not commit `.env`.

Commit `.env.example` only.

For production-style deployment, create `.env` from `.env.production.example` instead.

```bash
cp .env.production.example .env
```

Then replace all `change-me` values with strong secrets.

Production template values include:

```text
MSSQL_SA_PASSWORD
MXHRM_DB_NAME
JWT_ISSUER
JWT_AUDIENCE
JWT_SECRET_KEY
JWT_ACCESS_TOKEN_MINUTES
SEQ_ADMIN_USERNAME
SEQ_ADMIN_PASSWORD
IMAGE_TAG
```

---

## Docker Compose Profiles

The project uses Compose profiles to separate infrastructure from app containers.

```text
infra profile
   ├── sqlserver
   ├── redis
   └── seq

app profile
   ├── api
   └── web
```

This lets us choose between two common workflows.

---

## Workflow 1: Run Infrastructure Only

Use this when you want to run API and Angular locally, but use Docker for SQL Server, Redis, and Seq.

```bash
make infra-up
```

This starts:

```text
sqlserver
redis
seq
```

Then run API locally:

```bash
dotnet run --project src/MXHRM.Api
```

Then run Angular locally:

```bash
cd client/mxhrm-web
npm start
```

Useful local URLs:

```text
Angular dev server: http://localhost:4200
API HTTPS:          https://localhost:7276
API HTTP:           http://localhost:5090
Seq:                http://localhost:5341
```

---

## Workflow 2: Run Full Stack Containers

Use this when you want the whole system to run inside Docker.

```bash
make app-up
```

This starts:

```text
web
api
sqlserver
redis
seq
```

Open:

```text
http://localhost:4200
```

In this mode, browser requests flow through Nginx:

```text
http://localhost:4200
   ↓
Nginx in web container
   ├── /api/...  → http://api:8080/api/...
   └── /hubs/... → http://api:8080/hubs/...
```

---

## Makefile Commands

| Command | Description |
|---|---|
| `make infra-up` | Start only infrastructure containers |
| `make infra-down` | Stop infrastructure containers |
| `make app-up` | Build and start full stack containers |
| `make app-down` | Stop full stack containers |
| `make app-build` | Build API and web images |
| `make ps` | Show container status |
| `make logs` | Follow logs from all services |
| `make logs-api` | Follow API logs |
| `make logs-web` | Follow web/Nginx logs |
| `make logs-db` | Follow SQL Server logs |
| `make restart-api` | Restart only API container |
| `make restart-web` | Restart only web container |
| `make clean` | Stop full stack containers |

---

## Main URLs

| URL | Purpose |
|---|---|
| `http://localhost:4200` | Angular app served by Nginx |
| `http://localhost:4200/api/...` | API through Nginx reverse proxy |
| `http://localhost:4200/hubs/realtime` | SignalR through Nginx reverse proxy |
| `http://localhost:8080/swagger` | API Swagger directly from API container |
| `http://localhost:8080/health` | API health endpoint |
| `http://localhost:5341` | Seq dashboard |

Seq default local login:

```text
Username: admin
Password: P@ssw0rd
```

---

## Seeded Admin User

The application seeds a default admin user on API startup.

```text
Username: admin
Password: P@ssw0rd
CompanyID: JCROP
Role: Admin
```

The seeder is called from:

```text
src/MXHRM.Api/Program.cs
   ↓
IdentitySeeder.SeedRolesAsync(app.Services)
```

The seeder lives in:

```text
src/MXHRM.Infrastructure/Data/IdentitySeeder.cs
```

---

## Database Migration

If the SQL Server container database is empty, run EF migration from the host machine:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/MXHRM.Infrastructure \
  --startup-project src/MXHRM.Api
```

The container exposes SQL Server on:

```text
localhost,1433
```

Inside Docker network, the API uses:

```text
sqlserver,1433
```

This difference is important:

```text
Host machine → localhost,1433
API container → sqlserver,1433
```

---

## Container Networking Notes

Inside Docker Compose, containers do not use `localhost` to talk to each other.

Use service names:

```text
api → sqlserver:1433
api → redis:6379
api → seq:80
web → api:8080
```

Why:

```text
localhost inside api container = api container itself
localhost inside web container = web container itself
```

So the API connection string in Docker uses:

```text
Server=sqlserver,1433
```

Redis uses:

```text
redis:6379
```

Seq uses:

```text
http://seq:80
```

---

## Healthchecks

Expected status:

```bash
make ps
```

Expected result:

```text
mxhrm-sqlserver   Up (healthy)
mxhrm-api         Up (healthy)
mxhrm-web         Up (healthy)
mxhrm-redis       Up
mxhrm-seq         Up
```

Current healthcheck strategy:

| Service | Healthcheck |
|---|---|
| `sqlserver` | Checks TCP port `1433` |
| `api` | Calls `http://localhost:8080/health` |
| `web` | Calls `http://localhost/` inside Nginx container |

Redis and Seq currently do not have custom healthchecks.

---

## Nginx Behavior

Nginx handles three important responsibilities.

### 1. Serve Angular SPA

Angular routes such as these should work even after browser refresh:

```text
/employees
/security-admin/roles
/reports/generated-reports
```

Nginx falls back to:

```text
index.html
```

### 2. Reverse Proxy API

Browser request:

```text
http://localhost:4200/api/Employees
```

Nginx forwards to:

```text
http://api:8080/api/Employees
```

### 3. Reverse Proxy SignalR

Browser request:

```text
http://localhost:4200/hubs/realtime
```

Nginx forwards to:

```text
http://api:8080/hubs/realtime
```

The config includes WebSocket upgrade headers:

```nginx
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection "Upgrade";
```

---

## Forwarded Headers

Because requests pass through Nginx before reaching ASP.NET Core, the API must read forwarded headers:

```text
X-Forwarded-For
X-Forwarded-Proto
```

This helps the API understand the original request scheme and client IP.

Relevant API setup:

```text
ForwardedHeadersOptions
app.UseForwardedHeaders()
```

This is important for:

```text
request logging
client IP tracking
HTTPS termination in production
reverse proxy correctness
```

---

## Common Troubleshooting

### Compose says a variable is not set

Example:

```text
The "JWT_SECRET_KEY" variable is not set
```

Fix:

```bash
cp .env.example .env
```

Then fill missing values in `.env`.

Validate:

```bash
docker compose --profile infra --profile app config
```

---

### SQL Server is unhealthy

Check logs:

```bash
make logs-db
```

If SQL Server logs say this, the database engine is ready:

```text
SQL Server is now ready for client connections.
```

Current healthcheck uses TCP port `1433` because Azure SQL Edge image may not include `sqlcmd`.

---

### API is unhealthy

Check API logs:

```bash
make logs-api
```

Common causes:

```text
SQL Server connection string is wrong
Database has no migrations applied
Redis connection is wrong
JWT secret is missing
appsettings.Container.json is missing required values
```

Test API health directly:

```text
http://localhost:8080/health
```

---

### Web is healthy but login does not work

Check API through Nginx:

```text
http://localhost:4200/api/Employees
```

Expected result when not authenticated:

```text
401 Unauthorized
```

This means Nginx is forwarding to API correctly.

If you get Nginx `404`, check:

```text
client/mxhrm-web/nginx.conf
```

---

### Angular refresh gives 404

If refreshing a deep route fails:

```text
http://localhost:4200/security-admin/roles
```

Check that Nginx has SPA fallback:

```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

---

### SignalR does not connect

Check browser console and API logs.

Also verify Nginx has:

```nginx
location /hubs/ {
    proxy_pass http://api:8080/hubs/;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "Upgrade";
}
```

If authentication fails, check that Angular sends access token through SignalR `accessTokenFactory`.

---

### Port already in use

Current host ports:

```text
4200 → web
8080 → api
1433 → sqlserver
6379 → redis
5341 → seq
```

If a port is already used, either stop the existing service or change the port mapping in `docker-compose.yml`.

Example:

```yaml
ports:
  - "14330:1433"
```

Then host access becomes:

```text
localhost,14330
```

---

## Rebuild Rules

Use `make app-up` when changing:

```text
Dockerfile
Angular source
API source
package.json
.csproj
nginx.conf
```

Use `make restart-api` when only restarting the current API container is enough.

Use `make app-build` when you only want to build images without starting logs.

---

## Volumes

Compose uses named volumes:

```text
seq-data
redis-data
sqlserver-data
```

These keep data even after:

```bash
make app-down
```

If you intentionally want a fresh database, remove the volume manually.

Be careful: deleting `sqlserver-data` removes the database.

```bash
docker volume ls | grep mxhrm
```

Then remove only the intended volume.

---

## Production-style Deployment with GHCR

The CI pipeline publishes Docker images to GitHub Container Registry.

```text
GitHub Actions
   ↓
Build multi-platform images
   ↓
Push to GHCR
   ├── ghcr.io/oconneil/mxhrm-api
   └── ghcr.io/oconneil/mxhrm-web
```

This gives us a deployment flow where a server does not need source code or local Docker builds.

The deploy machine only needs:

```text
docker-compose.prod.yml
.env
Docker / Docker Compose
Access to GHCR packages if the images are private
```

---

## Production Compose File

Use `docker-compose.prod.yml` when you want to run images from GHCR instead of building from local source.

```text
docker-compose.yml
= local development compose that builds images from source

docker-compose.prod.yml
= production-style compose that pulls images from GHCR
```

Key difference:

```yaml
api:
  image: ghcr.io/oconneil/mxhrm-api:${IMAGE_TAG:-latest}

web:
  image: ghcr.io/oconneil/mxhrm-web:${IMAGE_TAG:-latest}
```

In other words:

```text
dev compose  = build from local source code
prod compose = pull prebuilt images from GHCR
```

---

## Production Commands

Pull images:

```bash
make prod-pull
```

Run production-style stack:

```bash
make prod-up
```

Check status:

```bash
make prod-ps
```

Follow logs:

```bash
make prod-logs
```

Stop stack:

```bash
make prod-down
```

---

## Image Tags

The production compose file uses:

```text
IMAGE_TAG
```

Default:

```text
latest
```

Examples:

```bash
IMAGE_TAG=latest make prod-up
```

```bash
IMAGE_TAG=sha-abc1234 make prod-up
```

Image names:

```text
ghcr.io/oconneil/mxhrm-api:${IMAGE_TAG:-latest}
ghcr.io/oconneil/mxhrm-web:${IMAGE_TAG:-latest}
```

Typical tag strategy:

```text
latest       = latest successful build on default branch
master       = branch tag
sha-abc1234  = exact commit tag for rollback / traceability
```

Use SHA tags when you want a repeatable deployment:

```bash
IMAGE_TAG=sha-abc1234 make prod-up
```

---

## Multi-platform Images

CI builds images for:

```text
linux/amd64
linux/arm64
```

This allows the same image tag to work on:

```text
x64 Linux servers
Apple Silicon / ARM64 machines
```

Why this matters:

```text
If the image is only linux/amd64
and the host is linux/arm64/v8
Docker may run it through emulation and show a platform warning.
```

With multi-platform images, Docker automatically pulls the matching platform image.

---

## GHCR Login

If the packages are private, login first:

```bash
docker login ghcr.io
```

Use:

```text
Username: your GitHub username
Password: GitHub token with read:packages
```

If the packages are public, login may not be required.

For CI publishing, GitHub Actions uses:

```text
GITHUB_TOKEN
packages: write
```

For deploy machines pulling private images, use a token with:

```text
read:packages
```

---

## Deployment Flow

```text
1. Push code to main / master / develop
2. GitHub Actions builds API and Web images
3. GitHub Actions pushes images to GHCR
4. Deploy machine pulls images
5. docker-compose.prod.yml runs the stack
```

Production-style deploy command:

```bash
make prod-pull
make prod-up
make prod-ps
```

Rollback idea:

```bash
IMAGE_TAG=sha-previous123 make prod-up
```

---

## Security Notes

This setup is a development/staging baseline.

Before real production:

```text
Use strong secrets
Do not use P@ssw0rd
Move secrets to environment variables or a secret manager
Use HTTPS termination
Pin image versions instead of latest
Restrict ForwardedHeaders known proxies/networks
Review CORS origins
Back up SQL Server volumes
Protect Seq with strong credentials
Review Hangfire dashboard authorization
Use production-grade SQL Server hosting if needed
```

---

## Quick Start

Fresh setup:

```bash
cp .env.example .env
make app-up
```

Apply database migrations if needed:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/MXHRM.Infrastructure \
  --startup-project src/MXHRM.Api
```

Open app:

```text
http://localhost:4200
```

Login:

```text
Username: admin
Password: P@ssw0rd
```

Check status:

```bash
make ps
```
