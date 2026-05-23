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
Dynamic Permission Policy
FluentValidation
Serilog + Seq
Redis Cache
Hangfire Background Jobs
Audit.NET / Custom Audit Trail
Custom Kendo Grid Adapter
ClosedXML Excel Export
Swagger / OpenAPI
NSwag Client Generation
```

### ใช้ทำอะไร

| Tech | Purpose |
| --- | --- |
| ASP.NET Core | สร้าง REST API |
| EF Core | ORM จัดการ database |
| SQL Server | เก็บข้อมูลหลัก, Identity, Hangfire, Audit |
| Identity | จัดการ user/password/role |
| JWT | Authentication แบบ stateless |
| Refresh Token | ต่ออายุ access token อย่างปลอดภัย |
| Policy Auth | ตรวจสิทธิ์ระดับ permission |
| Dynamic Permission Policy | สร้าง policy ตาม permission string โดยไม่ต้อง register ทีละตัว |
| FluentValidation | validate request |
| Serilog + Seq | structured logging + log viewer |
| Redis | distributed cache / performance |
| Hangfire | background jobs / recurring jobs |
| Audit Trail | เก็บประวัติ data changes และ user activity |
| Custom Grid Adapter | รับ Kendo Grid state แล้วทำ paging/sort/filter ฝั่ง server |
| ClosedXML | สร้างไฟล์ Excel report |
| Swagger / OpenAPI | สร้าง API contract สำหรับ documentation และ code generation |
| NSwag | Generate Angular API client จาก Swagger |

---

## 🟣 Frontend (Angular 21)

```text
Angular Standalone Components
Signals
HttpClient + Interceptors
Reactive Forms
Bootstrap 5
Kendo UI for Angular
Kendo Grid
Kendo Drawer / AppBar / Dialog
Permission Guard
Access Denied / Not Found UX
NSwag Generated API Client
```

### ใช้ทำอะไร

| Tech | Purpose |
| --- | --- |
| Angular | SPA frontend |
| Standalone Components | ลด NgModule boilerplate |
| Signals | component state management |
| HttpClient | เรียก API |
| Interceptors | แนบ JWT, refresh token retry, global error handling |
| Reactive Forms | form + validation |
| Bootstrap | layout และ utility เบื้องต้น |
| Kendo UI | enterprise grid/admin UI |
| Kendo Dialog | detail viewer สำหรับ audit/activity logs |
| NSwag Client | ลด manual HttpClient และ sync TypeScript client กับ Backend API contract |

---

## 🟡 Dev / Infra

```text
Docker (Seq)
Docker (Redis)
Hangfire Dashboard
HTTPS Dev Certificate
CORS
SQL Server
```

---

# 🧠 Architecture

## Backend Clean Architecture

```text
MXHRM.Domain
   ↓
MXHRM.Application
   ↓
MXHRM.Infrastructure
   ↓
MXHRM.Api
```

### Dependency Direction

```text
API → Application → Domain
Infrastructure → Application + Domain
API wires Infrastructure through DI
```

---

## Full-stack Flow

```text
Angular UI
   ↓
HttpClient + Interceptors
   ↓
ASP.NET Core API
   ↓
Application Service Interface
   ↓
Infrastructure Service / EF Core
   ↓
SQL Server / Redis / Hangfire
```

---

## Secure Auth Flow

```text
Login
   ↓
Access Token + Refresh Token
   ↓
Angular stores session
   ↓
Auth Interceptor attaches JWT
   ↓
API validates JWT
   ↓
401 triggers refresh token retry
```

---

## Permission Flow

```text
AspNetUsers
   ↓
AspNetUserRoles
   ↓
AspNetRoles
   ↓
RolePermissions
   ↓
Permissions
   ↓
JWT permission claims
   ↓
Dynamic policy provider
   ↓
[Authorize(Policy = "...")]
   ↓
Angular menu + permissionGuard
```

---

## Kendo Grid Server-side Flow

```text
Kendo Grid
   ↓ dataStateChange
gridState: State
   ↓
toDataSourceRequestString(state)
   ↓
POST /api/{resource}/grid?...query
   ↓
GridDataSourceRequestParser
   ↓
ToGridDataSourceResultAsync()
   ↓
EF Core IQueryable
   ↓
{ data, total }
   ↓
Angular GridDataResult
```

---

## Reporting + NSwag Flow

```text
ReportsController
   ↓
Swagger / OpenAPI contract
   ↓
NSwag
   ↓
api-client.ts
   ↓
ReportsClient
   ↓
ReportService
   ↓
Employee Summary Report UI
   ↓
Excel download
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
Permission-based Authorization
Dynamic Policy-based Authorization
DB Role-Permission Mapping
Role CRUD API
Security Admin APIs
Auth Interceptor
Route Guard
Permission Guard
Access Denied UX
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
Route UX
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
Dynamic permission policy provider
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

## ✅ Project 7.1: Audit + Logging Foundation

### สิ่งที่ทำ

```text
เพิ่ม Audit.NET packages
สร้าง AuditLog entity
เพิ่ม AuditLogs table
เพิ่ม CurrentUserService
สร้าง IAuditLogService
บันทึก audit ตอน Employee Create / Update / Delete
เพิ่ม AuditLogs API สำหรับ Admin
เพิ่ม validator สำหรับ audit query
เพิ่ม indexes สำหรับ audit query
ตรวจ audit create/update/delete ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
EmployeeService
   ↓
IAuditLogService
   ↓
CurrentUserService
   ↓
AuditLogs table
   ↓
AuditLogsController
```

### Skill ที่ได้

```text
Audit Trail Design
Old/New Value Tracking
TraceId Linkage
Admin Audit API
Compliance Foundation
```

---

## ✅ Project 7.2: User Activity Logging

### สิ่งที่ทำ

```text
สร้าง UserActivityLog entity
เพิ่ม UserActivityLogs table
ขยาย CurrentUserService ให้เก็บ IP และ UserAgent
สร้าง IUserActivityLogService
บันทึก LoginSuccess / LoginFailed
บันทึก RefreshToken / RefreshTokenFailed
บันทึก Logout / LogoutFailed
บันทึก ManualJobTrigger
เพิ่ม UserActivityLogs API สำหรับ Admin
เพิ่ม validator สำหรับ activity query
ตรวจ auth activity และ admin activity ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
AuthService / JobsController
   ↓
IUserActivityLogService
   ↓
CurrentUserService
   ↓
UserActivityLogs table
   ↓
UserActivityLogsController
```

### Skill ที่ได้

```text
Security Activity Logging
Login Failure Tracking
Admin Action Tracking
IP / UserAgent Capture
Operational Visibility
```

---

## ✅ Project 7.3: Audit Permissions

### สิ่งที่ทำ

```text
เพิ่ม permission audit.read
เพิ่ม permission activity.read
Seed permissions ลง DB
Map permissions ให้ Admin
เปลี่ยน AuditLogsController ให้ใช้ audit.read
เปลี่ยน UserActivityLogsController ให้ใช้ activity.read
เพิ่ม Angular permission constants
ตรวจ Admin access และ 403 unauthorized ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
Permissions constants
   ↓
IdentitySeeder
   ↓
JWT permission claims
   ↓
Dynamic policy provider
   ↓
Audit / Activity APIs
```

### Skill ที่ได้

```text
Fine-grained Permission Design
Sensitive Log Access Control
Policy-based Audit Security
Frontend Permission Readiness
```

---

## ✅ Project 8.1: Kendo Grid Foundation

### สิ่งที่ทำ

```text
ติดตั้ง Kendo UI for Angular packages
ปรับ Angular package compatibility
เพิ่ม Kendo theme
เพิ่ม Angular localize polyfill
ปรับ build budget ให้รองรับ Kendo bundle
เปลี่ยน Employee table เป็น Kendo Grid
เชื่อม GridDataResult กับ Employee data
เพิ่ม server-side endpoint /api/employees/grid
ใช้ Kendo State + toDataSourceRequestString
เพิ่ม toolbar / refresh / clear filters / no records UX
ตรวจ build และ grid flow ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
EmployeeList
   ↓
gridState: State
   ↓
EmployeeService.getEmployeesGrid(state)
   ↓
POST /api/employees/grid?...query
   ↓
Backend grid endpoint
   ↓
{ data, total }
```

### Skill ที่ได้

```text
Kendo Grid Integration
GridDataResult
Angular State-driven Grid
Server-side Grid Query Flow
Enterprise Table UX
```

---

## ✅ Project 8.2: Security Admin UI

### สิ่งที่ทำ

```text
สร้าง features/security-admin
สร้าง SecurityAdmin models
สร้าง SecurityAdminService
สร้าง Roles Management UI
สร้าง Role Permissions Management UI
สร้าง Users Management UI
สร้าง User Roles Management UI
สร้าง Audit Logs UI
สร้าง User Activity Logs UI
เพิ่ม permission-based drawer menu
เพิ่ม permissionGuard
เพิ่ม route-level permission protection
ปรับ MainLayout เป็น authenticated shell แบบไม่มี /main ใน URL
ตรวจ roles/users/permissions/logs UI ผ่านแล้ว
```

### Route ที่เพิ่ม

```text
/security-admin/roles
/security-admin/roles/:roleId/permissions
/security-admin/users
/security-admin/users/:userId/roles
/security-admin/audit-logs
/security-admin/user-activity-logs
```

### จุดเชื่อมต่อหลัก

```text
SecurityAdmin Pages
   ↓
SecurityAdminService
   ↓
Security Admin APIs
   ↓
Role / Permission / User / Audit data
```

### Skill ที่ได้

```text
Admin Console UI
Role-Permission Management UX
User-Role Management UX
Permission-based Navigation
Route-level Authorization UX
```

---

## ✅ Project 8.3: Custom Kendo Grid Adapter + Logs Server-side Grid

### สิ่งที่ทำ

```text
แทน Telerik.UI.for.AspNet.Core ด้วย custom grid adapter
สร้าง GridDataSourceRequest
สร้าง GridDataSourceResult<T>
สร้าง GridDataSourceRequestParser
สร้าง ToGridDataSourceResultAsync extension
รองรับ Kendo MVC query format จาก toDataSourceRequestString
รองรับ page/pageSize
รองรับ sort=field-dir
รองรับ filter expression แบบ nested AND/OR
รองรับ text / number / bool / date filters
ปรับ Employee Grid ให้ใช้ Kendo built-in filter/sort จาก gridState
เพิ่ม /api/audit-logs/grid
เพิ่ม /api/user-activity-logs/grid
ปรับ Audit Logs UI เป็น GridDataResult
ปรับ User Activity Logs UI เป็น GridDataResult
ตรวจ paging/sorting/filtering ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
Kendo Grid State
   ↓
toDataSourceRequestString
   ↓
Custom GridDataSourceRequestParser
   ↓
GridQueryableExtensions
   ↓
EF Core IQueryable
   ↓
{ data, total }
```

### Skill ที่ได้

```text
Server-side Grid Adapter Design
Expression Tree Filtering
Dynamic Sorting
Nested Filter Parsing
Reducing Third-party Backend Dependency
Reusable Enterprise Grid Pattern
```

---

## ✅ Project 8.4: Grid UX Polish + JSON Detail Viewer

### สิ่งที่ทำ

```text
เพิ่ม Kendo Dialog ใน Audit Logs
เพิ่ม Kendo Dialog ใน User Activity Logs
เปลี่ยน logs grid เป็น summary-first UX
เพิ่ม View Details action
เพิ่ม JSON pretty print helper
เพิ่ม badge สีตาม action/activity type
ย้าย oldValues / newValues / metadata / userAgent ไปอยู่ใน dialog
ปรับ dialog width / max-height / scroll content
เพิ่ม responsive styling สำหรับ dialog
ตรวจ dialog + grid flow ผ่านแล้ว
```

### UX Pattern

```text
Grid = Summary
Dialog = Details
```

### Skill ที่ได้

```text
Enterprise Monitoring UX
JSON Detail Viewer
Kendo Dialog
Readable Audit UI
Responsive Admin Detail Modal
```

---

## ✅ Project 8.5: Access Denied + Page Not Found + Route UX

### สิ่งที่ทำ

```text
สร้าง Access Denied page
แสดง required permission จาก query param
ปรับ permissionGuard ให้ redirect ไป /access-denied
ปรับ authGuard ให้ส่ง returnUrl
ปรับ Login ให้กลับไป returnUrl หลัง login สำเร็จ
สร้าง Page Not Found page
เพิ่ม /not-found route
เพิ่ม wildcard route **
ตรวจ 401 / 403 / 404 route UX ผ่านแล้ว
```

### Route UX

```text
401 Unauthorized → /login?returnUrl=...
403 Forbidden → /access-denied?permission=...
404 Not Found → /not-found
```

### Skill ที่ได้

```text
Route Guard UX
ReturnUrl Login Flow
Access Denied Design
Wildcard Routing
Enterprise Navigation Polish
```

---

## ✅ Project 9.1: Employee Summary Report API + UI

### สิ่งที่ทำ

```text
สร้าง Report DTO / Request / Response
สร้าง IReportService ใน Application
สร้าง ReportService implementation ใน Infrastructure
เพิ่ม Employee Summary Report API
เพิ่ม Employee Summary Report page ใน Angular
เพิ่ม report filters
เพิ่ม summary cards
เพิ่ม Kendo Grid breakdown by company
ตรวจ report API + UI ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
EmployeeSummaryReport UI
   ↓
ReportService
   ↓
ReportsController
   ↓
IReportService
   ↓
Infrastructure ReportService
   ↓
Employees table aggregate query
```

### Skill ที่ได้

```text
Report API Design
Aggregate Query
Full-stack Reporting
Angular Report Screen
Kendo Grid Report View
```

---

## ✅ Project 9.2: Employee Summary Excel Export

### สิ่งที่ทำ

```text
เพิ่ม ClosedXML
สร้าง ReportFileResponse
เพิ่ม ExportEmployeeSummaryExcelAsync
สร้าง Excel workbook จาก Employee Summary Report
เพิ่ม ReportsController export endpoint
ส่ง Content-Disposition filename จาก backend
Expose Content-Disposition ผ่าน CORS
เพิ่ม Angular download flow
อ่าน filename จาก backend
ตรวจ export Excel ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
Employee Summary Report UI
   ↓
Export Excel button
   ↓
ReportsController export endpoint
   ↓
IReportService.ExportEmployeeSummaryExcelAsync
   ↓
ClosedXML workbook
   ↓
File response + Content-Disposition
   ↓
Browser download .xlsx
```

### Skill ที่ได้

```text
Excel Export
File Response API
Content-Disposition
CORS Exposed Headers
Report Download UX
```

---

## ✅ Project 9.3: NSwag Client Generation

### สิ่งที่ทำ

```text
ติดตั้ง NSwag local tool
สร้าง nswag.json
Generate Angular api-client.ts จาก Swagger
แก้ duplicate operation names ด้วย CustomOperationIds
Register API_BASE_URL ใน Angular
เพิ่ม ProducesFileAttribute
เพิ่ม FileResponseOperationFilter
ทำให้ Swagger อธิบาย binary file response ได้
Regenerate export endpoint ให้เป็น FileResponse
Refactor ReportService ให้ใช้ ReportsClient
Refactor EmployeeSummaryReport ให้ใช้ FileResponse
ตรวจ report + export flow ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
Backend Controller
   ↓
Swagger / OpenAPI
   ↓
NSwag
   ↓
api-client.ts
   ↓
ReportsClient
   ↓
ReportService
   ↓
Angular Component
```

### Skill ที่ได้

```text
OpenAPI Contract
Client Code Generation
Typed API Client
OperationId Design
File Download Codegen
Reducing Manual HttpClient
```

---

# 📊 Current Status

```text
Progress: ~99% (Project 9.3 NSwag Client Generation Complete)
```

ตอนนี้คุณมี:

```text
✅ Full-stack CRUD
✅ Secure API (JWT + Refresh Token)
✅ Login system
✅ Protected routes
✅ Role-based UI rendering
✅ Permission-based API protection
✅ Dynamic permission policy
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
✅ Hangfire background job foundation
✅ Hangfire Dashboard
✅ Recurring jobs
✅ Refresh token cleanup job
✅ Employee summary report job
✅ Manual admin job trigger APIs
✅ AuditLogs table
✅ Employee create/update/delete audit
✅ AuditLogs Admin API
✅ UserActivityLogs table
✅ Auth activity logging
✅ Manual job trigger activity logging
✅ UserActivityLogs Admin API
✅ audit.read permission
✅ activity.read permission
✅ Kendo UI for Angular
✅ Employee Kendo Grid
✅ Security Admin UI
✅ Role-Permission Management UI
✅ User-Role Management UI
✅ Audit Logs UI
✅ User Activity Logs UI
✅ Custom Kendo Grid Adapter
✅ Server-side grid paging/sorting/filtering
✅ JSON detail dialogs
✅ Access Denied page
✅ Page Not Found page
✅ ReturnUrl login flow
✅ Employee Summary Report API
✅ Employee Summary Report UI
✅ Excel export with ClosedXML
✅ File download response with filename
✅ Swagger binary file metadata
✅ NSwag generated Angular client
✅ ReportsClient integration
✅ Production-ready base
```

👉 คุณคือระดับ:

```text
Junior → Mid-level Full-stack Developer
```

---

# ❗ สิ่งที่ยังขาด (Important)

```text
❌ Resource-based Permission
❌ Permission versioning / token invalidation after permission change
❌ Dashboard widgets
❌ Audit report screen
❌ Export PDF reports
❌ CI/CD (GitHub Actions)
❌ Deployment baseline (Dockerize API/Angular + Nginx)
❌ Automated tests coverage
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

## ✅ Project 7: Audit + Logging

Status: Completed

Completed parts:

```text
Project 7.1 Audit + Logging Foundation
Project 7.2 User Activity Logging
Project 7.3 Audit Permissions
```

Output:

```text
Audit trail สำหรับ Employee data changes
User activity logging สำหรับ auth/admin actions
AuditLogs API และ UserActivityLogs API
Permission แยก audit.read และ activity.read
รองรับ security investigation และ compliance foundation
```

---

## ✅ Project 8: Enterprise UI

Status: Completed

Completed parts:

```text
Project 8.1 Kendo Grid Foundation
Project 8.2 Security Admin UI
Project 8.3 Custom Kendo Grid Adapter + Logs Server-side Grid
Project 8.4 Grid UX Polish + JSON Detail Viewer
Project 8.5 Access Denied + Page Not Found + Route UX
```

Output:

```text
Kendo UI for Angular พร้อมใช้งาน
Employee Grid ใช้ server-side paging/sorting/filtering
Security Admin UI ใช้งาน roles/users/permissions/logs ได้
Audit/Activity logs ใช้ grid pattern กลาง
JSON detail viewer อ่านง่าย
Route UX ครบ 401 / 403 / 404
Admin ทำงานผ่าน UI ได้ ไม่ต้องพึ่ง Swagger/Postman สำหรับงานหลัก
```

---

## ▶️ Project 9: Reporting + Export

Status: In Progress

Completed parts:

```text
Project 9.1 Employee Summary Report API + UI
Project 9.2 Employee Summary Excel Export
Project 9.3 NSwag Client Generation
```

Completed output:

```text
Employee Summary Report ใช้งานได้
Report filter ใช้งานได้
Excel export/download ใช้งานได้
Angular report service ใช้ NSwag generated ReportsClient
Swagger รองรับ file response สำหรับ code generation
ลด manual HttpClient สำหรับ report flow
ต่อยอดจาก Hangfire EmployeeReportJob ได้บางส่วน
```

Remaining scope:

```text
Project 9.4 Audit Report Screen
Project 9.5 PDF Export หรือ export format เพิ่มเติม
Project 9.6 Hangfire report file generation / async report download
Project 9.7 Extend NSwag usage ไปยัง feature services อื่น
```

Expected final output:

```text
ระบบรายงานครบทั้ง Employee และ Audit
รองรับ download/export หลายรูปแบบ
รองรับ generated client เป็นมาตรฐานของ Angular API layer
รองรับ report generation แบบ background job ในอนาคต
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

# 🎯 Final Target State

```text
Enterprise-ready HRM system
Clean Architecture backend
Secure JWT + Refresh Token auth
Permission-based authorization
Modern Angular frontend
Kendo enterprise UI
Redis performance layer
Hangfire background jobs
Audit trail
Reporting
NSwag generated API client
CI/CD + deployment baseline
```

---

# 🔥 จุดแข็งของ Project นี้

```text
✔ Real-world architecture
✔ Production mindset
✔ Security built-in
✔ DB-driven permission model
✔ Dynamic policy-based API protection
✔ Security admin APIs
✔ Security admin UI
✔ Better error UX
✔ Clean Architecture separation
✔ Thin controllers with service interfaces
✔ Infrastructure owns EF Core and Identity implementation
✔ Redis cache layer
✔ Employee query optimization
✔ Advanced filtering and sorting
✔ Custom Kendo Grid Adapter
✔ Server-side grid paging/sorting/filtering
✔ Slow query visibility
✔ Hangfire background jobs
✔ Refresh token cleanup automation
✔ Employee report recurring job
✔ Admin manual job triggers
✔ Audit trail foundation
✔ User activity logging
✔ Audit-specific permissions
✔ Kendo UI for Angular
✔ Access Denied / Not Found UX
✔ Employee Summary Report
✔ Excel export with ClosedXML
✔ Swagger / OpenAPI client generation
✔ NSwag generated Angular API client
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
Backend + Frontend + Security + Clean Architecture + Performance + Background Jobs + Audit + Enterprise UI
JWT + Refresh Token + Role + Permission DB Mapping + Security Admin APIs/UI + Better Error UX + Auto Retry + Layer Separation + Redis Cache + Advanced Filtering + Hangfire Jobs + Audit Trail + Kendo Grid + Custom Grid Adapter + Route UX + Reporting + Excel Export + NSwag Client Generation
```

---

# 🚀 Next Step

ตอนนี้เราอยู่ที่:

```text
Project 1 completed - Backend Foundation
Project 2 completed - Angular CRUD
Project 3 completed - Auth + Security + Permission + Error UX
Project 4 completed - Clean Architecture Refactor
Project 5 completed - Redis Cache + Performance Tuning + Advanced Filtering
Project 6 completed - Hangfire Background Jobs
Project 7 completed - Audit + Logging
Project 8 completed - Enterprise UI + Kendo Grid + Admin UX
Project 9.1 completed - Employee Summary Report API + UI
Project 9.2 completed - Employee Summary Excel Export
Project 9.3 completed - NSwag Client Generation
Next: Project 9.4 Audit Report Screen
```

---

# Database migration (Developer)

```bash
# Add migration
ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add ...... \
  --project src/MXHRM.Infrastructure \
  --startup-project src/MXHRM.Api

# Update database
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update \
  --project src/MXHRM.Infrastructure \
  --startup-project src/MXHRM.Api
```

# Telerik NuGet (Developer)

```bash
dotnet nuget add source https://nuget.telerik.com/v3/index.json \
  --name telerik \
  --username YOUR_TELERIK_EMAIL \
  --password YOUR_TELERIK_API_KEY \
  --store-password-in-clear-text
```
