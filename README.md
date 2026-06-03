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
- **Checklist ก่อน deploy จริง** → [DEPLOYMENT.md](DEPLOYMENT.md)
- **Image tagging / registry / multi-platform** → [Docker.md](Docker.md)

> CI/CD: push เข้า `main`/`master`/`develop` → GitHub Actions จะ build + test (`dotnet test MXHRM.slnx`) แล้ว push image ขึ้น GHCR อัตโนมัติ

## หมายเหตุ

- หากเรียก API ผ่าน HTTPS แล้วติด certificate error ให้รัน:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```
