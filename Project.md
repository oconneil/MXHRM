# 🚀 MXHRM Full-Stack Learning Project (Summary)

## 🎯 Project Overview

โปรเจกต์นี้เป็นการสร้าง **Modern Enterprise Full-Stack Web Application**
โดยใช้แนวคิด **Clean Architecture + Secure API + Modern Angular**

ระบบตัวอย่างคือ:

```text
HRM System (Employee Management)
```

---

# 🧱 Tech Stack ทั้งหมด

## 🔵 Backend (.NET 10)

```text
ASP.NET Core Web API
Entity Framework Core
SQL Server
ASP.NET Core Identity
JWT + Refresh Token Authentication
Policy-based Authorization
FluentValidation
Serilog + Seq
Redis Cache
Hangfire Background Jobs
```

### ใช้ทำอะไร

| Tech             | Purpose                    |
| ---------------- | -------------------------- |
| ASP.NET Core     | สร้าง REST API             |
| EF Core          | ORM จัดการ database        |
| SQL Server       | เก็บข้อมูล                 |
| Identity         | จัดการ user/password       |
| JWT              | Authentication (stateless) |
| Refresh Token    | ต่ออายุ access token อย่างปลอดภัย |
| Policy Auth      | ตรวจสิทธิ์ระดับ permission       |
| FluentValidation | validate request           |
| Serilog          | logging                    |
| Seq              | ดู log แบบ UI              |
| Redis            | distributed cache / performance |
| Hangfire         | background jobs / recurring jobs |

---

## 🟣 Frontend (Angular 21)

```text
Angular Standalone
Signals
HttpClient
Reactive Forms
Bootstrap 5
```

### ใช้ทำอะไร

| Tech           | Purpose           |
| -------------- | ----------------- |
| Angular        | SPA frontend      |
| Signals        | state management  |
| HttpClient     | เรียก API         |
| Reactive Forms | form + validation |
| Bootstrap      | UI layout         |

---

## 🟡 Dev / Infra

```text
Docker (Seq)
Docker (Redis)
Hangfire Dashboard
HTTPS Dev Certificate
CORS
```

---

# 🧠 Architecture

## Backend

```text
Controller
   ↓
Service
   ↓
DbContext
   ↓
Database
```

---

## Full-stack Flow

```text
Angular UI
   ↓
HttpClient (Interceptor)
   ↓
ASP.NET API
   ↓
EF Core
   ↓
SQL Server
```

---

## Read Flow With Redis Cache

```text
Angular UI
   ↓
ASP.NET API
   ↓
EmployeeService
   ↓
Redis Cache
   ↓ ถ้า cache miss
SQL Server
```

---

# 📦 Project Breakdown

---

## ✅ Project 1: Backend Foundation

### สิ่งที่ทำ

```text
CRUD API
DTO
Service Layer
Validation
Pagination
Global Error Handling
Logging
```

### Skill ที่ได้

```text
REST API
EF Core
Clean Code
Middleware
Logging
```

---

## ✅ Project 2: Angular + API Integration

### สิ่งที่ทำ

```text
Employee List
Search + Pagination
Create / Edit / Delete
Reactive Forms
Bootstrap UI
```

### Skill ที่ได้

```text
Angular
HttpClient
Form Handling
Frontend + Backend Integration
```

---

## ✅ Project 3: Authentication + Security

### สิ่งที่ทำ

```text
Identity
Register / Login
JWT Token
Refresh Token
Role-based Authorization
Permission-based Authorization (Policy)
DB Role-Permission Mapping
Role CRUD API
Protect API
Auth Interceptor
Route Guard
Navbar State
Logout
```

### Skill ที่ได้

```text
Security
JWT
Refresh Token Flow
Role / Permission
Policy-based Authorization
Auth Flow
Frontend Auth State
```

---

## ✅ Project 3.5: Permission DB Mapping + Role Management

### สิ่งที่ทำ

```text
Permissions table
RolePermissions join table
Seed permissions
Seed role-permission mapping
JWT permission claims from DB
Policy constants
Permission authorization handler
Role CRUD API protected by role.manage
```

### จุดเชื่อมต่อหลัก

```text
AspNetRoles
   ↓
RolePermissions
   ↓
Permissions
   ↓
AuthService adds permission claims to JWT
   ↓
[Authorize(Policy = ...)]
   ↓
PermissionAuthorizationHandler checks permission claim
```

### Skill ที่ได้

```text
Enterprise Authorization
Permission Modeling
Identity Role Integration
Policy-based Security
DB-driven Access Control
```

---

## ✅ Project 3.6: Security Admin APIs

### สิ่งที่ทำ

```text
Permissions list API
Role-Permission Management API
User list / detail API
User-Role Management API
Activate / Deactivate User API
Safety guards for admin access
```

### API ที่เพิ่ม

```text
GET /api/permissions
GET /api/roles/{roleId}/permissions
PUT /api/roles/{roleId}/permissions
GET /api/users
GET /api/users/{id}
GET /api/users/{userId}/roles
PUT /api/users/{userId}/roles
PUT /api/users/{id}/activate
PUT /api/users/{id}/deactivate
```

### Safety Rules

```text
ห้ามถอด role.manage ออกจาก Admin role
ห้าม current admin ถอด role ที่มี role.manage ออกจากตัวเอง
ห้าม current admin deactivate ตัวเอง
```

### Skill ที่ได้

```text
Admin Security API Design
User-Role Administration
Role-Permission Administration
Safety Guard Design
Enterprise Access Management
```

---

## ✅ Project 3.7: Better Error UX

### สิ่งที่ทำ

```text
Backend ErrorResponse contract
Central ErrorCodes
Global Exception Middleware mapping
Validation error response format
JWT 401 / 403 JSON responses
BaseApiController error helpers
Angular ApiError model
Angular ErrorService
Angular global error interceptor
Toast-style global error UI
Employee Create/Edit backend validation display
Auto refresh token retry
Permission-based Angular UI helpers
```

### จุดเชื่อมต่อหลัก

```text
Backend ErrorResponse
   ↓
Angular ApiError
   ↓
Error Interceptor
   ↓
ErrorService signal state
   ↓
Toast UI + Form field validation messages
```

### Skill ที่ได้

```text
Consistent API Error Contract
Frontend Error Normalization
Validation UX
Toast UX
TraceId Debugging
Refresh Token Retry UX
```

---

## ✅ Project 4: Clean Architecture Refactor

### สิ่งที่ทำ

```text
แยก solution เป็น 4 layers
ย้าย entity หลักไป Domain
ย้าย DTO / Validator / Interface ไป Application
ย้าย EF Core / Identity / Service implementation ไป Infrastructure
ย้าย DbContext และ Migrations ไป Infrastructure
ย้าย Permission constants และ claim type contract ไป Application
ทำ Infrastructure DI extension
ทำ Controller ให้บางลงโดยเรียก Application service interfaces
ลด dependency ของ API ไม่ให้เรียก DbContext / Identity models โดยตรง
```

### จุดเชื่อมต่อหลัก

```text
API Controller
   ↓
Application Interface
   ↓
Infrastructure Service
   ↓
AppDbContext / Identity / SQL Server
```

### Skill ที่ได้

```text
Clean Architecture
Dependency Direction
Use-case Contracts
Infrastructure DI
EF Core Migrations Assembly Separation
Thin Controller Design
```

---

## ✅ Project 5.1: Redis Cache

### สิ่งที่ทำ

```text
เพิ่ม Redis service ใน Docker Compose
เพิ่ม Redis connection config
สร้าง ICacheService abstraction ใน Application
สร้าง RedisCacheService implementation ใน Infrastructure
Register Redis + Cache service ใน DI
Cache Employee list
Cache Employee detail
Clear cache ตอน Create / Update / Delete Employee
Implement prefix invalidation สำหรับ employee list cache
ตรวจ Redis key และ cache hit ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
EmployeeService
   ↓
ICacheService
   ↓
RedisCacheService
   ↓
Redis
```

### Skill ที่ได้

```text
Distributed Cache
Redis Integration
Cache Key Design
Cache Invalidation
Clean Architecture Abstraction
Performance Optimization
```

---

## ✅ Project 5.2: Performance Tuning + Query Optimization

### สิ่งที่ทำ

```text
ปรับ Employee read query เป็น Projection-first
เพิ่ม database indexes สำหรับ Employee query
ทำ pagination order ให้ stable ด้วย CompanyID + EmployeeID
เพิ่ม helper สำหรับ skip calculation
เพิ่ม EF Core command logging ใน development
เพิ่ม SlowQueryThresholdMs config
เพิ่ม stopwatch logging ใน Employee list query
ตรวจ build / query / migration ผ่านแล้ว
```

### Skill ที่ได้

```text
EF Core Query Optimization
Projection-first Query
Database Index Design
Stable Pagination
Slow Query Logging
Seq Performance Visibility
```

---

## ✅ Project 5.3: Advanced Query / Filtering

### สิ่งที่ทำ

```text
เพิ่ม CompanyID filter
เพิ่ม IsActive filter
เพิ่ม SortBy
เพิ่ม SortDirection
เพิ่ม SortBy / SortDirection validator whitelist
แยก ApplyFilters ใน EmployeeService
แยก ApplySorting ใน EmployeeService
ปรับ cache key ให้รวม filter + sort ทั้งหมด
ปรับ Angular Employee list ให้ส่ง query params ใหม่
เพิ่ม UI filter / sort / reset
ตรวจ advanced filtering + sorting ผ่านแล้ว
```

### API ที่รองรับ

```text
GET /api/employees?companyID=C001
GET /api/employees?isActive=true
GET /api/employees?search=john&isActive=true
GET /api/employees?sortBy=salary&sortDirection=desc
GET /api/employees?companyID=C001&isActive=true&sortBy=hireDate&sortDirection=desc&page=1&pageSize=20
```

### จุดเชื่อมต่อหลัก

```text
Angular Employee List
   ↓
Query params
   ↓
GetEmployeesRequest
   ↓
Validator
   ↓
ApplyFilters + ApplySorting
   ↓
Cache-safe key
   ↓
Redis / SQL Server
```

### Skill ที่ได้

```text
Enterprise Filtering
Safe Sorting Whitelist
Cache-safe Query Design
Angular Query Param Integration
Signal-based UI State
Full-stack Query Flow
```

---

## ✅ Project 6.1: Hangfire Foundation

### สิ่งที่ทำ

```text
เพิ่ม Hangfire packages
เพิ่ม Hangfire SQL Server storage
เพิ่ม Hangfire config
Register AddHangfire + AddHangfireServer ใน Infrastructure DI
เปิด Hangfire Dashboard
เพิ่ม Dashboard authorization filter สำหรับ production
สร้าง SystemHealthJob
Register recurring job system-health-job
ตรวจ Dashboard + recurring job ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
ASP.NET API
   ↓
Hangfire Server
   ↓
SQL Server Hangfire storage
   ↓
SystemHealthJob
   ↓
Serilog / Seq
```

### Skill ที่ได้

```text
Background Job Foundation
Hangfire SQL Server Storage
Recurring Jobs
Dashboard Monitoring
Dashboard Security Pattern
```

---

## ✅ Project 6.2: Cleanup Expired Refresh Tokens Job

### สิ่งที่ทำ

```text
เพิ่ม RefreshTokenCleanup config
สร้าง CleanupExpiredRefreshTokensJob
ลบ refresh tokens ที่หมดอายุหรือ revoked เกิน retention
Register job ใน DI
Register recurring job cleanup-expired-refresh-tokens
เพิ่ม manual trigger API สำหรับ Admin
ตรวจ recurring job + manual trigger ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
Hangfire Recurring Job / Manual Trigger
   ↓
CleanupExpiredRefreshTokensJob
   ↓
RefreshTokens table
   ↓
Delete expired / revoked old tokens
   ↓
Serilog / Seq
```

### Skill ที่ได้

```text
Security Hygiene Job
Retention Policy
Configurable Cron Schedule
Manual Background Job Trigger
202 Accepted API Pattern
```

---

## ✅ Project 6.3: Employee Report Job

### สิ่งที่ทำ

```text
สร้าง EmployeeSummaryReport DTO
สร้าง EmployeeReportJob
Query employee aggregate summary
Log report result เข้า Serilog / Seq
Register job ใน DI
Register recurring job employee-summary-report
เพิ่ม manual trigger API สำหรับ Admin
ตรวจ recurring job + manual trigger + log ผ่านแล้ว
```

### Report Summary

```text
TotalEmployees
ActiveEmployees
InactiveEmployees
AverageSalary
TotalSalary
GeneratedAtUtc
```

### จุดเชื่อมต่อหลัก

```text
Hangfire Recurring Job / Manual Trigger
   ↓
EmployeeReportJob
   ↓
Employees table
   ↓
EmployeeSummaryReport
   ↓
Serilog / Seq
```

### Skill ที่ได้

```text
Scheduled Reporting
Aggregate Read Query
Report DTO Design
Admin Job Trigger API
Report Logging Foundation
```

---

# 📊 Current Status

```text
Progress: ~96% (Project 6 Background Jobs Complete)
```

ตอนนี้คุณมี:

```text
✅ Full-stack CRUD
✅ Secure API (JWT + Refresh Token)
✅ Login system
✅ Protected routes
✅ Role-based UI rendering
✅ Permission-based API protection
✅ Permission DB mapping
✅ Role CRUD API
✅ Permission Management API
✅ User Management API
✅ User-Role Management API
✅ Activate / Deactivate User
✅ Standard backend error contract
✅ Angular global error interceptor
✅ Toast-style global error UI
✅ Employee form backend validation UX
✅ Auto refresh token retry
✅ Permission-based Angular UI rendering
✅ Clean Architecture project split
✅ Domain / Application / Infrastructure / API separation
✅ Thin API controllers
✅ Infrastructure DI registration
✅ Redis distributed cache
✅ Employee list/detail cache
✅ Cache invalidation
✅ Projection-first queries
✅ Employee performance indexes
✅ Stable pagination
✅ Slow query logging
✅ Advanced Employee filtering
✅ Advanced Employee sorting
✅ Angular filter/sort UI
✅ Hangfire background job foundation
✅ Hangfire Dashboard
✅ Recurring jobs
✅ Refresh token cleanup job
✅ Employee summary report job
✅ Manual admin job trigger APIs
✅ Production-ready base
```

👉 คุณคือระดับ:

```text
Junior → Mid-level Full-stack Developer
```

---

# ❗ สิ่งที่ยังขาด (Important)

```text
❌ State Management (NgRx / SignalStore)
❌ Permission Management UI
❌ User-Role Management UI
❌ Resource-based Permission
❌ Kendo UI Grid
❌ NSwag Client
❌ Audit Logging
❌ CI/CD (GitHub Actions)
```

---

# 🗺️ Roadmap ตั้งแต่เริ่มต้นจนจบ Project

## ✅ Project 1: Backend Foundation

Status: Completed

Scope:

```text
ASP.NET Core Web API
Employee CRUD
EF Core + SQL Server
DTO separation
Service layer
FluentValidation
Pagination
Global exception middleware
Serilog + Seq logging
```

Output:

```text
Backend API พื้นฐานที่พร้อมต่อยอด
Controller ไม่ใส่ business logic หนัก
มี validation / error handling / logging
```

---

## ✅ Project 2: Angular + API Integration

Status: Completed

Scope:

```text
Angular standalone components
Employee list
Search + pagination
Create / Edit / Delete employee
Reactive Forms
Bootstrap UI
HttpClient service
```

Output:

```text
Frontend ใช้งาน CRUD ได้จริง
Angular เชื่อม API สำเร็จ
ผู้ใช้จัดการ Employee ผ่าน UI ได้
```

---

## ✅ Project 3: Authentication + Security

Status: Completed

Completed parts:

```text
Project 3.1 Identity + Register / Login
Project 3.2 JWT Authentication
Project 3.3 Refresh Token Flow
Project 3.4 Angular Auth Interceptor + Guard
Project 3.5 Permission DB Mapping + Dynamic Policy
Project 3.6 Security Admin APIs
Project 3.7 Better Error UX
```

Output:

```text
JWT + Refresh Token ใช้งานได้
Role-based authorization ใช้งานได้
Permission-based authorization ใช้งานได้
Role / User / Permission admin APIs พร้อม
Angular auth state + route guard + auto refresh token พร้อม
Backend และ Angular error UX เป็นมาตรฐานเดียวกัน
```

Remaining security enhancements:

```text
Permission Management UI
User-Role Management UI
Resource-based Permission
Token invalidation / permission versioning
```

---

## ✅ Project 4: Clean Architecture Refactor

Status: Completed

Scope:

```text
MXHRM.Domain
MXHRM.Application
MXHRM.Infrastructure
MXHRM.Api
```

Output:

```text
Entity อยู่ Domain
DTO / Validator / Interface อยู่ Application
EF Core / Identity / Services / Jobs อยู่ Infrastructure
Controller / Middleware / API composition อยู่ API
Dependency direction ถูกต้องขึ้น
```

---

## ✅ Project 5: Advanced Backend

Status: Completed

Completed parts:

```text
Project 5.1 Redis Cache
Project 5.2 Performance Tuning + Query Optimization
Project 5.3 Advanced Query / Filtering
```

Output:

```text
Redis cache สำหรับ Employee list/detail
Cache invalidation ตอนข้อมูลเปลี่ยน
Projection-first query
Database indexes
Stable pagination
Slow query logging
Advanced filter/sort
Angular filter/sort UI
```

---

## ✅ Project 6: Background Jobs

Status: Completed

Completed parts:

```text
Project 6.1 Hangfire Foundation
Project 6.2 Cleanup Expired Refresh Tokens Job
Project 6.3 Employee Report Job
```

Output:

```text
Hangfire SQL Server storage
Hangfire Dashboard
Recurring jobs
System health job
Refresh token cleanup job
Employee summary report job
Manual admin trigger APIs
```

---

## ▶️ Project 7: Audit + Logging

Status: Current / Next

Planned scope:

```text
Audit.NET
AuditLog table
Track Create / Update / Delete
Store old values and new values
Store user id / username / trace id
AuditLogs API for Admin
Audit search / filter
User activity log foundation
```

Expected output:

```text
รู้ว่าใครแก้อะไร เมื่อไหร่
ตรวจสอบ employee changes ย้อนหลังได้
รองรับ security investigation และ compliance เบื้องต้น
```

---

## ⏭️ Project 8: Enterprise UI

Status: Planned

Planned scope:

```text
Kendo UI for Angular
Employee Grid
Permission Management UI
User-Role Management UI
Audit Log UI
Dashboard widgets
Better table filtering / sorting / export UX
```

Expected output:

```text
Frontend ดูเป็น enterprise application มากขึ้น
Admin ทำงานผ่าน UI ได้ครบ ไม่ต้องพึ่ง Swagger/Postman
```

---

## ⏭️ Project 9: Reporting

Status: Planned

Planned scope:

```text
Report APIs
Report background jobs
Export Excel
Export PDF
Telerik Document Processing
Employee summary report file
Audit report file
```

Expected output:

```text
ระบบสร้างรายงานจริงได้
รองรับ download/export
ต่อยอดจาก Hangfire report job ได้
```

---

## ⏭️ Project 10: DevOps + Deployment

Status: Planned

Planned scope:

```text
Dockerize API
Dockerize Angular
SQL Server / Redis / Seq / Hangfire setup
Nginx reverse proxy
GitHub Actions CI/CD
Environment variables
Production logging config
Deployment checklist
```

Expected output:

```text
พร้อม deploy จริง
มี CI/CD
มี production-ready infrastructure baseline
```

---

## 🎯 Final Target State

```text
Enterprise-ready HRM system
Clean Architecture backend
Secure JWT + Refresh Token auth
Permission-based authorization
Modern Angular frontend
Redis performance layer
Hangfire background jobs
Audit trail
Enterprise UI
Reporting
CI/CD + deployment baseline
```

---

# 🔥 จุดแข็งของ Project นี้

```text
✔ Real-world architecture
✔ Production mindset
✔ Security built-in
✔ DB-driven permission model
✔ Policy-based API protection
✔ Security admin APIs
✔ Better error UX
✔ Clean Architecture separation
✔ Thin controllers with service interfaces
✔ Infrastructure owns EF Core and Identity implementation
✔ Redis cache layer
✔ Employee query optimization
✔ Advanced filtering and sorting
✔ Slow query visibility
✔ Hangfire background jobs
✔ Refresh token cleanup automation
✔ Employee report recurring job
✔ Admin manual job triggers
✔ Completed auth/security foundation
✔ Scalable structure
✔ Full-stack integration
```

---

# 🎯 TL;DR

```text
Project นี้ = สร้าง Enterprise Web App ตั้งแต่ 0 → Production
```

คุณได้:

```text
Backend + Frontend + Security + Clean Architecture + Performance + Background Jobs
JWT + Refresh Token + Role + Permission DB Mapping + Security Admin APIs + Better Error UX + Auto Retry + Layer Separation + Redis Cache + Advanced Filtering + Hangfire Jobs
```

---

# 🚀 Next Step

ตอนนี้เรากำลังอยู่ที่:

```text
Project 3 completed - Auth + Security + Permission + Error UX
Project 4 completed - Clean Architecture Refactor
Project 5 completed - Redis Cache + Performance Tuning + Advanced Filtering
Project 6 completed - Hangfire Background Jobs
Next: Project 7 Audit + Logging
```

---

# Database migration (Developer)
//Add migration
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add ...... \
  --project src/MXHRM.Infrastructure \
  --startup-project src/MXHRM.Api

//Update
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/MXHRM.Infrastructure \
  --startup-project src/MXHRM.Api
  
