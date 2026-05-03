# MXHRM

ระบบตัวอย่าง HRM แบบ Full-stack: **ASP.NET Core Web API (.NET 10)** + **Angular 21**

## โครงสร้างโปรเจกต์

- `src/MXHRM.Api` - Backend API
- `client/mxhrm-web` - Frontend (Angular)
- `seq-data` - ข้อมูล/ล็อกของ Seq (local)

## Tech Stack

- Backend: ASP.NET Core Web API, EF Core, SQL Server, Identity, JWT, FluentValidation, Serilog
- Frontend: Angular Standalone, HttpClient, Reactive Forms, Bootstrap 5
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

