# MXHRM

[![MXHRM CI](https://github.com/oconneil/MXHRM/actions/workflows/ci.yml/badge.svg)](https://github.com/oconneil/MXHRM/actions/workflows/ci.yml)
[![CodeQL](https://github.com/oconneil/MXHRM/actions/workflows/codeql.yml/badge.svg)](https://github.com/oconneil/MXHRM/actions/workflows/codeql.yml)

ระบบตัวอย่าง HRM แบบ Full-stack: **ASP.NET Core Web API (.NET 10)** + **Angular 21**

## เอกสารหลัก

- [Project.md](Project.md) - learning roadmap, architecture, feature progress และ project summary
- [Docker.md](Docker.md) - Docker Compose, Nginx, healthcheck และ developer container workflow

## โครงสร้างโปรเจกต์

- `src/MXHRM.Api` - Backend API
- `client/mxhrm-web` - Frontend (Angular)
- `.github/workflows` - GitHub Actions CI และ CodeQL workflow

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

## หมายเหตุ

- หากเรียก API ผ่าน HTTPS แล้วติด certificate error ให้รัน:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```
