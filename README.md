# MXHRM

[![MXHRM CI](https://github.com/oconneil/MXHRM/actions/workflows/ci.yml/badge.svg)](https://github.com/oconneil/MXHRM/actions/workflows/ci.yml)
[![CodeQL](https://github.com/oconneil/MXHRM/actions/workflows/codeql.yml/badge.svg)](https://github.com/oconneil/MXHRM/actions/workflows/codeql.yml)

ระบบตัวอย่าง HRM แบบ Full-stack: **ASP.NET Core Web API (.NET 10)** + **Angular 21**

## เอกสารหลัก

- [Project.md](Project.md) - learning roadmap, architecture, feature progress และ project summary
- [Docker.md](Docker.md) - Docker Compose, Nginx, healthcheck และ developer container workflow
- [ARCHITECTURE.md](ARCHITECTURE.md) - Clean Architecture อธิบายฉบับเต็ม พร้อมตัวอย่างและ analogy
- [DEPLOYMENT.md](DEPLOYMENT.md) - production deployment checklist (pre-deploy / deploy / verify / rollback)

## โครงสร้างโปรเจกต์

- `src/MXHRM.Api` - Backend API
- `client/mxhrm-web` - Frontend (Angular)
- `.github/workflows` - GitHub Actions CI และ CodeQL workflow

## สถาปัตยกรรม — Clean Architecture

Backend แบ่งเป็น 4 layer ยึดหลัก **Dependency Rule: ลูกศรพึ่งพาชี้เข้าด้านในเสมอ — ชั้นในไม่รู้จักชั้นนอก**

```text
🔴 API  →  🟢 Application  →  🟡 Domain
🔵 Infrastructure  →  (Application + Domain)
```

| Layer | หน้าที่ | ตัวอย่างในโปรเจกต์ |
|---|---|---|
| 🟡 `MXHRM.Domain` | แก่นธุรกิจ ไม่พึ่งเทคโนโลยีใดเลย | `Employee`, `BaseEntity` |
| 🟢 `MXHRM.Application` | use case + สัญญา (interface) + DTO + validator | `IEmployeeService`, `ICacheService`, DTOs |
| 🔵 `MXHRM.Infrastructure` | implement สัญญาด้วยเทคโนโลยีจริง | `EmployeeService`, `RedisCacheService`, `AppDbContext` |
| 🔴 `MXHRM.Api` | HTTP / routing + ประกอบร่างผ่าน DI | `EmployeesController`, `Program.cs` |

**Dependency Inversion** — _"คนที่ต้องการ เป็นเจ้าของ interface / คนที่ทำได้ เป็นคน implement"_
เช่น `ICacheService` อยู่ Application (คนต้องการ) แต่ `RedisCacheService` อยู่ Infrastructure (คนทำ) → เปลี่ยนเทคโนโลยีข้างในได้โดยไม่กระทบชั้นใน และ mock ตอนเขียน test ได้ง่าย

**จะวางโค้ดใหม่ไว้ที่ layer ไหน?**

- กฎธุรกิจแท้ๆ (entity, business rule) → 🟡 Domain
- interface / DTO / validator → 🟢 Application
- ผูกกับเทคโนโลยีเฉพาะ (EF Core, Redis, JWT, PDF) → 🔵 Infrastructure
- HTTP / controller / middleware / composition → 🔴 API

> Litmus test: ลบ project Infrastructure ทิ้งแล้ว Application/Domain ต้องยัง compile ได้ (ชั้นในไม่พึ่งชั้นนอก)

📖 อ่านฉบับเต็ม (analogy ร้านอาหาร, เดินตาม request ทีละชั้น, Dependency Inversion, ข้อผิดพลาดที่เจอบ่อย) ที่ **[ARCHITECTURE.md](ARCHITECTURE.md)**

## Tech Stack

- Backend: ASP.NET Core Web API, EF Core, SQL Server, Identity, JWT, FluentValidation, Serilog, Redis, Hangfire, SignalR
- Frontend: Angular Standalone, Signals, HttpClient, Reactive Forms, Bootstrap 5, Kendo UI for Angular
- DevOps: Docker Compose, Nginx, GitHub Actions, CodeQL, Dependabot
- Observability: Seq

## เริ่มใช้งาน (Development)

### 1) Run API

```bash
dotnet run --project src/MXHRM.Api
```

โดยปกติ API จะรันที่:

- `https://localhost:7276`
- `http://localhost:5090`

Swagger: `https://localhost:7276/swagger`

### 2) Run Frontend

```bash
cd client/mxhrm-web
npm install
npm start
```

Frontend: `http://localhost:4200`

## Build & Deploy

### Build (production)

```bash
# Frontend — Angular production build (output: dist/mxhrm-web/browser)
cd client/mxhrm-web && npm ci && npm run build

# Backend — release build (ทั้ง solution)
dotnet build MXHRM.slnx --configuration Release

# หรือ build เป็น Docker images (API + Angular/Nginx) ทีเดียว
make app-build
```

### Run full stack ด้วย Docker (Development)

```bash
make infra-up   # เฉพาะ infra: SQL Server + Redis + Seq
make app-up     # build + run ครบทั้ง web / api / infra
make ps         # เช็คสถานะ container
```

เปิดที่ `http://localhost:8088` — รายละเอียดทั้งหมดดู [Docker.md](Docker.md)

### Deploy (production-style)

ใช้ image ที่ CI build แล้ว push ขึ้น GitHub Container Registry (GHCR):

```bash
# 1) เตรียม .env (ตั้ง IMAGE_TAG + secrets จริง — ดู .env.production.example)
# 2) pull image ตาม IMAGE_TAG แล้วรัน hardened stack (เปิด public เฉพาะ web/nginx)
make prod-pull
make prod-up
```

- **HTTPS/TLS** ที่ edge ด้วย Caddy: เพิ่มไฟล์ override
  `docker compose -f docker-compose.prod.yml -f docker-compose.prod.tls.yml up -d`
- **Database migration** (แอปไม่ auto-migrate) — สร้าง idempotent script แล้วรันเข้า SQL:

  ```bash
  dotnet ef migrations script --idempotent -o migrate.sql \
    -p src/MXHRM.Infrastructure -s src/MXHRM.Api
  docker run --rm --network container:mxhrm-sqlserver -v "$(pwd)/migrate.sql:/m.sql" \
    mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d "$MXHRM_DB_NAME" -i /m.sql
  ```

- **ไฟล์อัปโหลด** (รูป/เอกสารพนักงาน) เก็บบน volume `mxhrm-uploads` (mount ที่ `api → /app/App_Data/uploads`)
  → อยู่รอด redeploy; อย่าลืมรวมเข้าแผน backup
- **Checklist ก่อน deploy จริง** → [DEPLOYMENT.md](DEPLOYMENT.md)
- **Image tagging / registry / multi-platform** → [Docker.md](Docker.md)

> CI/CD: push เข้า `main`/`master`/`develop` → GitHub Actions จะ build + test (`dotnet test MXHRM.slnx`) แล้ว push image ขึ้น GHCR อัตโนมัติ

### Deploy บน IIS (Windows Server) — ทางเลือกแทน Docker

> ใช้เมื่อองค์กรบังคับ Windows Server / IIS. Docker คือเส้นทางหลักที่ hardened ไว้แล้ว — IIS ต้องตั้ง Redis / TLS / background jobs เองเพิ่ม

**เตรียม server:** ติดตั้ง **ASP.NET Core 10 Hosting Bundle** (ได้ .NET runtime + ASP.NET Core Module/ANCM) + เปิด IIS feature **WebSocket Protocol** (SignalR) และ **URL Rewrite** (Angular SPA)

```bash
# 1) Publish API (ทำบน Mac/Linux ได้) — ได้ web.config มาด้วยอัตโนมัติ
dotnet publish src/MXHRM.Api/MXHRM.Api.csproj -c Release -o ./publish/api
#    เพิ่ม --self-contained -r win-x64 ถ้าไม่อยากพึ่ง runtime บนเครื่อง server

# 2) Build frontend → static files
cd client/mxhrm-web && npm ci && npm run build   # output: dist/mxhrm-web/browser
```

ตั้งค่าบน IIS:

- **App Pool** ของ API: `.NET CLR Version = "No Managed Code"` (โค้ดรันผ่าน ANCM ไม่ใช่ .NET Framework)
- **Site** ของ API: ชี้ physical path ไปโฟลเดอร์ `publish/api`
- ตั้ง env `ASPNETCORE_ENVIRONMENT=Production` ใน `web.config` (`<environmentVariables>`) + ใส่ connection string / `Jwt:SecretKey` / `Redis:ConnectionString` ผ่าน `appsettings.Production.json` หรือ env (อย่า commit secret)
- **Frontend**: สร้าง IIS site แยกชี้ไป `dist/mxhrm-web/browser` + ใส่ URL Rewrite ให้ทุก route ตกไป `index.html` (กัน 404 เวลา refresh) + ชี้ `apiBaseUrl` (`environment.prod.ts`) ไป API
- รัน migration: `dotnet ef migrations script --idempotent` แล้ว apply ที่ SQL Server

ข้อควรระวังเฉพาะ project นี้บน IIS:

| เรื่อง | ต้องทำ |
|---|---|
| **File upload** | ให้สิทธิ์ Write โฟลเดอร์ `App_Data/uploads` แก่ `IIS AppPool\<pool>` และตั้ง `FileStorage:RootPath` เป็น path นอกโฟลเดอร์แอป (กันไฟล์หายตอน deploy ทับ) |
| **Hangfire** (background jobs) | App Pool ตั้ง `Start Mode = AlwaysRunning`, `Idle Time-out = 0` — หรือแยก Hangfire worker เป็น Windows Service (`Hangfire:UseServer=false` ที่ web) |
| **SignalR** (`/hubs/realtime`) | เปิด WebSocket Protocol feature ใน IIS |
| **Redis** | ไม่มากับ Windows → ต้องมี Redis แยก แล้วชี้ `Redis:ConnectionString` |

## หมายเหตุ

- หากเรียก API ผ่าน HTTPS แล้วติด certificate error ให้รัน:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```
