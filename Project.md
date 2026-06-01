# 🚀 MXHRM Full-Stack Learning Project (Summary)

[![MXHRM CI](https://github.com/oconneil/MXHRM/actions/workflows/ci.yml/badge.svg)](https://github.com/oconneil/MXHRM/actions/workflows/ci.yml)
[![CodeQL](https://github.com/oconneil/MXHRM/actions/workflows/codeql.yml/badge.svg)](https://github.com/oconneil/MXHRM/actions/workflows/codeql.yml)

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
SignalR Realtime Notification
Persistent Realtime Notification Center
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
| SignalR | ส่ง realtime notification/status update จาก backend ไป frontend |
| Notification Center | เก็บ notification ต่อ user ใน DB พร้อม unread/read state และส่ง update แบบ realtime |

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
SignalR Client
Realtime Notification Center
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
| SignalR Client | รับ realtime message และ update UI โดยไม่ต้อง refresh |
| Notification Center | แสดง notification inbox, unread badge, mark as read และ realtime toast ใน Main Layout |

---

## 🟡 Dev / Infra

```text
Docker (Seq)
Docker (Redis)
Docker Compose
Dockerized ASP.NET Core API
Dockerized Angular + Nginx
Azure SQL Edge / SQL Server Container
Nginx Reverse Proxy
Hangfire Dashboard
HTTPS Dev Certificate
CORS
SQL Server
SignalR Hub
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

## Extended NSwag Feature Boundary Flow

```text
ASP.NET Core Controller
   ↓ ProducesResponseType: 200 / 201 / 204 / ErrorResponse
Swagger / OpenAPI contract
   ↓
NSwag api-client.ts
   ↓ generated typed clients
Feature Service
   ↓ map optional generated DTO → strict feature model
Angular Component / Signals UI
```

### Kendo Grid Hybrid Rule

```text
Kendo Grid State (skip / take / sort / filter)
   ↓ toDataSourceRequestString(state)
HttpClient request with dynamic query string
   ↓ typed by NSwag GridDataSourceResult response model
Feature Service mapper
   ↓
GridDataResult for Kendo UI
```

```text
Regular CRUD / Query API = NSwag generated client directly
Kendo Grid API           = manual HttpClient transport + NSwag generated response type
Auth login / refresh     = manual flow intentionally retained for token/interceptor control
```

---

## Realtime Notification Center Flow

```text
Hangfire GeneratedReportJob
   ↓ status changes: Processing / Completed / Failed
IUserNotificationService.CreateOrUpdateAsync
   ↓ upsert by UserId + Key
UserNotifications table
   ↓ persisted inbox / unread state
IRealtimeNotifier
   ↓
SignalRRealtimeNotifier → Clients.User(userId)
   ↓ JWT authenticated connection
RealtimeHub → Angular RealtimeService
   ↓
NotificationService signals
   ↓
MainLayout Bell Badge + Popup Inbox + Realtime Toast
   ↓
NotificationsController API: load / unread-count / read / read-all
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

## ✅ Project 9.4: Audit Report Screen

### สิ่งที่ทำ

```text
สร้าง Audit Report DTO / Request / Response
เพิ่ม GetAuditReportAsync ใน IReportService
Implement audit aggregate query ใน Infrastructure ReportService
เพิ่ม ReportsController endpoint GET /api/reports/audit
Regenerate NSwag client
เพิ่ม ReportService.getAuditReport ใน Angular
สร้าง Audit Report page
เพิ่ม filters สำหรับ TableName / Action / UserId / FromUtc / ToUtc
เพิ่ม summary card
เพิ่ม Kendo Grid summary: By Action / By Table / By User
เพิ่ม route /reports/audit
เพิ่ม drawer menu Audit Report
ป้องกัน route และ menu ด้วย audit.read permission
ตรวจ type / build / browser flow ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
AuditLogs table
   ↓
Infrastructure ReportService
   ↓
IReportService
   ↓
ReportsController
   ↓
Swagger / NSwag
   ↓
ReportsClient
   ↓
Angular ReportService
   ↓
AuditReport Page
```

### Skill ที่ได้

```text
Audit Reporting
Aggregate Report Query
Report Summary UX
Permission-protected Report Route
NSwag Report API Integration
Kendo Grid Report Layout
```

---

## ✅ Project 9.5: Audit Report Excel Export

### สิ่งที่ทำ

```text
เพิ่ม ExportAuditReportExcelAsync ใน IReportService
Implement Audit Report Excel export ใน Infrastructure ReportService
Reuse GetAuditReportAsync เพื่อลด duplicate query logic
สร้าง Excel workbook ด้วย ClosedXML
เพิ่ม worksheets: Summary / By Action / By Table / By User
เพิ่ม ReportsController endpoint GET /api/reports/audit/export/excel
ใช้ ProducesFile เพื่อให้ Swagger อธิบาย file response
Regenerate NSwag client ให้ได้ FileResponse
เพิ่ม ReportService.exportAuditReportExcel ใน Angular
เพิ่ม Export Excel button ใน Audit Report page
ใช้ FileResponse.data และ FileResponse.fileName สำหรับ download
ตรวจ export แบบมี filter และไม่มี filter ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
AuditReport Component
   ↓
ReportService.exportAuditReportExcel()
   ↓
ReportsClient.exportAuditReportExcel()
   ↓
ReportsController.ExportAuditReportExcel
   ↓
IReportService.ExportAuditReportExcelAsync
   ↓
GetAuditReportAsync
   ↓
ClosedXML workbook
   ↓
FileResponse
   ↓
Browser download .xlsx
```

### Skill ที่ได้

```text
Report Export Pattern
ClosedXML Workbook Design
Reusable ReportFileResponse
Swagger File Response Codegen
NSwag FileResponse Download
Excel Export UX
```

---

## ✅ Project 9.6: Employee Summary PDF Export

### สิ่งที่ทำ

```text
ติดตั้ง QuestPDF ใน Infrastructure
ตั้งค่า QuestPDF Community license
เพิ่ม ExportEmployeeSummaryPdfAsync ใน IReportService
เพิ่ม PDF content type application/pdf
Implement Employee Summary PDF export ใน Infrastructure ReportService
Reuse GetEmployeeSummaryAsync เพื่อลด duplicate query logic
สร้าง PDF layout ด้วย QuestPDF
เพิ่ม Header / Summary Boxes / Breakdown by Company table / Footer page number
เพิ่ม ReportsController endpoint GET /api/reports/employee-summary/export/pdf
ใช้ ProducesFile(PdfContentType) เพื่อให้ Swagger อธิบาย PDF binary response
Regenerate NSwag client ให้ได้ FileResponse
เพิ่ม ReportService.exportEmployeeSummaryPdf ใน Angular
เพิ่ม Export PDF button ใน Employee Summary Report page
ใช้ FileResponse.data และ FileResponse.fileName สำหรับ download PDF
ตรวจ PDF export แบบมี filter และไม่มี filter ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
EmployeeSummaryReport Component
   ↓
ReportService.exportEmployeeSummaryPdf()
   ↓
ReportsClient.exportEmployeeSummaryPdf()
   ↓
ReportsController.ExportEmployeeSummaryPdf
   ↓
IReportService.ExportEmployeeSummaryPdfAsync
   ↓
GetEmployeeSummaryAsync
   ↓
QuestPDF Document
   ↓
ReportFileResponse
   ↓
File(application/pdf)
   ↓
Browser download .pdf
```

### Skill ที่ได้

```text
Backend PDF Generation
QuestPDF Layout
PDF File Response
application/pdf Content Type
NSwag PDF FileResponse
Reusable Report Export Architecture
```

---

## ✅ Project 9.7: Async Report Generation with Hangfire

### สิ่งที่ทำ

```text
สร้าง GeneratedReport entity ใน Domain
เพิ่ม GeneratedReports table
เพิ่ม ReportTypes / ReportFormats / ReportStatuses constants
เพิ่ม DTO สำหรับ CreateGeneratedReportRequest / GeneratedReportResponse / GeneratedReportListRequest
สร้าง IAsyncReportService ใน Application
สร้าง AsyncReportService ใน Infrastructure
สร้าง GeneratedReportJob สำหรับ Hangfire
Register IAsyncReportService และ GeneratedReportJob ใน DI
เพิ่ม GeneratedReportsController
เพิ่ม permission report.manage
Seed report.manage และ map ให้ Admin
สร้าง async report job ผ่าน POST /api/generated-reports
Enqueue Hangfire job เพื่อ generate report file
บันทึก status Pending / Processing / Completed / Failed
บันทึก file bytes / content type / filename ลง GeneratedReports table
เพิ่ม download endpoint GET /api/generated-reports/{id}/download
ปรับ ProducesFile ให้รองรับหลาย content types
แก้ Swagger 202 Accepted สำหรับ create endpoint
Regenerate NSwag client
สร้าง Angular GeneratedReportService
สร้าง Generated Reports UI
เพิ่ม route /reports/generated
เพิ่ม drawer menu Generated Reports
ตรวจ create job / status tracking / download ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
GeneratedReports UI
   ↓
GeneratedReportService
   ↓
GeneratedReportsClient
   ↓
GeneratedReportsController
   ↓
IAsyncReportService
   ↓
GeneratedReports table
   ↓
Hangfire GeneratedReportJob
   ↓
IReportService export methods
   ↓
GeneratedReports.Content
   ↓
Download later
```

### Skill ที่ได้

```text
Async Report Generation
Hangfire Job Queue Pattern
Report Job Lifecycle
Status Tracking
Download Later Pattern
202 Accepted API Design
Generated File Persistence
NSwag 202 Response Handling
```

---

## ✅ Project 9.8: Generic Realtime Notification with SignalR

### สิ่งที่ทำ

```text
ติดตั้งและตั้งค่า SignalR backend
สร้าง RealtimeHub เป็น realtime endpoint กลาง
เพิ่ม JWT authentication สำหรับ SignalR ผ่าน access_token query string
เพิ่ม CORS AllowCredentials สำหรับ SignalR negotiate
สร้าง UserIdProvider เพื่อ map SignalR connection กับ user id
สร้าง RealtimeMessage contract กลาง
สร้าง IRealtimeNotifier ใน Application
สร้าง SignalRRealtimeNotifier ใน API
Register SignalR / IRealtimeNotifier / IUserIdProvider ใน DI
เชื่อม GeneratedReportJob ให้ส่ง realtime status update
ติดตั้ง @microsoft/signalr ใน Angular
สร้าง Angular RealtimeService เป็น connection กลาง
สร้าง RealtimeMessage interface ฝั่ง Angular
เชื่อม Generated Reports page กับ realtime message
เพิ่ม realtime connection indicator
เพิ่ม realtime notice alert
เพิ่ม upsert row เมื่อ report status เปลี่ยน
เพิ่ม status badge / spinner / download button UX
ตรวจ SignalR negotiate / CORS / generated report realtime update ผ่านแล้ว
```

### จุดเชื่อมต่อหลัก

```text
GeneratedReportJob
   ↓
IRealtimeNotifier
   ↓
SignalRRealtimeNotifier
   ↓
RealtimeHub
   ↓
Angular RealtimeService
   ↓
latestMessage signal
   ↓
GeneratedReports page
   ↓
Grid row status update + UX notice
```

### Skill ที่ได้

```text
SignalR Hub
JWT Authentication for SignalR
CORS with Credentials
User-specific Realtime Messaging
Clean Architecture Realtime Abstraction
Angular SignalR Client
Realtime UI State Update
Background Job Progress Notification
Generic Realtime Infrastructure
```

---

## ✅ Project 9.8.1: Realtime Notification Center

### สิ่งที่ทำ

```text
สร้าง UserNotification entity ใน Domain
เพิ่ม UserNotifications table และ migration
เพิ่ม indexes สำหรับ query ตาม user / unread / updated time
เพิ่ม unique key ต่อ user เพื่อ upsert notification จาก event เดิม
สร้าง DTO และ IUserNotificationService ใน Application
สร้าง UserNotificationService implementation ใน Infrastructure
Register IUserNotificationService ใน DI
เพิ่ม NotificationsController สำหรับ inbox และ read state
เพิ่ม GET /api/notifications
เพิ่ม GET /api/notifications/unread-count
เพิ่ม PUT /api/notifications/{id}/read
เพิ่ม PUT /api/notifications/read-all
ขยาย RealtimeMessage ให้ส่ง NotificationId / Key / Tone / Route
ปรับ GeneratedReportJob ให้ persist notification ก่อนส่ง SignalR
Regenerate NSwag client ให้มี NotificationsClient
ปรับ Angular NotificationItem ให้ใช้ notification id จาก database
ปรับ NotificationService ให้ load inbox / unread count / mark as read / mark all as read
เชื่อม SignalR message เข้า NotificationService เพื่อ upsert item แบบ realtime
เพิ่ม notification bell badge และ popup inbox ใน MainLayout
เพิ่ม realtime toast เมื่อมี notification ใหม่
เพิ่ม loading skeleton / refresh / retry / inline error UX
ปรับ Kendo Popup overlay และ AppBar stacking ให้ panel แสดงเหนือ routed content
ตรวจ persistent inbox / unread count / realtime update / read state / popup display ผ่านแล้ว
```

### API ที่เพิ่ม

```text
GET /api/notifications
GET /api/notifications/unread-count
PUT /api/notifications/{id}/read
PUT /api/notifications/read-all
```

### จุดเชื่อมต่อหลัก

```text
GeneratedReportJob
   ↓
IUserNotificationService.CreateOrUpdateAsync
   ↓
UserNotifications table
   ↓
IRealtimeNotifier.SendToUserAsync
   ↓
SignalR RealtimeHub
   ↓
Angular RealtimeService
   ↓
NotificationService signals
   ↓
MainLayout Bell Badge / Popup Inbox / Toast
```

### Persistence + Realtime Pattern

```text
Database = source of truth
SignalR = fast delivery channel

ถ้า user offline:
notification ยังคงอยู่ใน UserNotifications และโหลดได้เมื่อเข้าใช้งานใหม่

ถ้า user online:
บันทึกลง DB ก่อน แล้ว SignalR ส่ง event เพื่อ update หน้าจอทันที
```

### Skill ที่ได้

```text
Persistent Notification Design
Realtime + Database Consistency Pattern
Per-user Notification Inbox
Unread / Read State Management
Signal-based Notification Store
Kendo Popup Notification UX
Overlay / Stacking Context Debugging
NSwag Client Extension for New APIs
```

---

## ✅ Project 9.9: Extend NSwag Usage

### สิ่งที่ทำ

```text
เพิ่ม OpenAPI response contract ให้ Employee, Role, User, Audit Log และ User Activity endpoints
ประกาศ status code จริงของ API เช่น 201 Created และ 204 No Content
ประกาศ ErrorResponse สำหรับ 400 / 404 / 409 ที่ service ส่งจริง
เพิ่ม typed GridDataSourceResult contract ให้ Employee / Audit / User Activity grid endpoints
Regenerate NSwag Angular client หลัง restart API
ปรับ ErrorService ให้ normalize SwaggerException เข้าสู่ Global Error UX ได้
Refactor Employee CRUD ให้ใช้ EmployeesClient
Refactor SecurityAdminService ให้ใช้ RolesClient / PermissionsClient / UsersClient
Refactor regular Audit / User Activity queries ให้ใช้ AuditLogsClient / UserActivityLogsClient
ใช้ mapper boundary แปลง generated DTO แบบ optional เป็น feature model ที่ UI ใช้งานได้แน่นอน
ปรับ Employee / Audit / User Activity Kendo Grid ให้ใช้ generated response type แบบ hybrid
คง AuthService เป็น manual HttpClient flow โดยตั้งใจ เพื่อดูแล token storage และ refresh interceptor
```

### จุดเชื่อมต่อหลัก

```text
Controller + ProducesResponseType
   ↓
Swagger / OpenAPI contract
   ↓
dotnet nswag run nswag.json
   ↓
core/api/api-client.ts
   ↓
Feature Service: Employees / Security Admin / Reports / Notifications
   ↓ mapping boundary
Angular Components
```

### Grid Exception Pattern

```text
NSwag ระบุ response shape ของ grid ได้
แต่ generated grid() ยังไม่มี Kendo State parameters
ดังนั้น grid transport ยังคงใช้ HttpClient + query string
และรับ response ด้วย NSwag generated Grid result type
```

### Skill ที่ได้

```text
OpenAPI Contract-first API Design
ProducesResponseType for Generated Clients
201 / 204 Response Contract Debugging
Typed Angular API Client Integration
SwaggerException Global Error Handling
Feature Service Mapping Boundary
Hybrid NSwag + Kendo Server-side Grid Pattern
Intentional Manual Auth Transport Design
```

---

## ✅ Project 10.1: Containerized Deployment Baseline

### สิ่งที่ทำ

```text
เพิ่ม Dockerfile สำหรับ ASP.NET Core API
เพิ่ม Dockerfile สำหรับ Angular production build + Nginx
เพิ่ม .dockerignore เพื่อลด Docker build context
เพิ่ม appsettings.Container.json สำหรับ container environment
ปรับ Angular production apiBaseUrl ให้เหมาะกับ Nginx reverse proxy
เพิ่ม Nginx reverse proxy สำหรับ /api และ /hubs
เพิ่ม SPA fallback สำหรับ Angular route refresh
เพิ่ม gzip และ static asset cache rule ใน Nginx
เพิ่ม Docker Compose full stack environment
เพิ่ม SQL Server container ด้วย Azure SQL Edge สำหรับ macOS arm64
เพิ่ม Redis container
เพิ่ม Seq container
เพิ่ม Docker healthchecks สำหรับ SQL Server, API และ Web
เพิ่ม /health endpoint สำหรับ API container
เพิ่ม Forwarded Headers support สำหรับ request ที่ผ่าน Nginx
เพิ่ม restart policy ให้ทุก service
เพิ่ม Docker Compose profiles: infra และ app
เพิ่ม .env และ .env.example สำหรับแยก config/secrets
เพิ่ม Makefile สำหรับคำสั่ง Docker ที่ใช้บ่อย
เพิ่ม Docker.md เป็น developer guide สำหรับ Docker workflow
เพิ่ม default admin seeder สำหรับใช้งานระบบหลัง container startup
```

### Services

```text
web        = Angular production build + Nginx
api        = ASP.NET Core Web API
sqlserver  = Azure SQL Edge / SQL Server-compatible database
redis      = Redis cache
seq        = Structured logging viewer
```

### Runtime Flow

```text
Browser
   ↓ http://localhost:4200
Web Container (Nginx + Angular)
   ├── /api/...       → API Container
   ├── /hubs/...      → SignalR Hub
   └── Angular routes → index.html fallback
API Container
   ├── SQL Server     → sqlserver:1433
   ├── Redis          → redis:6379
   └── Seq            → seq:80
```

### Developer Workflow

```text
make infra-up  = run SQL Server + Redis + Seq only
make app-up    = build and run full stack containers
make ps        = check container status
make logs      = inspect all service logs
make logs-api  = inspect API logs
make logs-web  = inspect Nginx/web logs
make logs-db   = inspect SQL Server logs
```

### Configuration Flow

```text
.env
   ↓ Docker Compose variable substitution
Container environment variables
   ↓ ASP.NET Core configuration provider
ConnectionStrings / Jwt / Redis / Serilog
```

### Healthcheck Flow

```text
sqlserver healthcheck
   ↓ waits for port 1433
api depends_on sqlserver: service_healthy
   ↓ exposes /health
web depends_on api
   ↓ serves Angular through Nginx
```

### จุดสำคัญที่ได้เรียนรู้

```text
Docker multi-stage build
Docker build context
.dockerignore
Docker Compose service discovery
Container networking
Docker Compose profiles
Healthcheck vs depends_on
Restart policy
Named volumes
Environment variable override in ASP.NET Core
Nginx SPA fallback
Nginx reverse proxy
SignalR through Nginx
Forwarded Headers behind reverse proxy
Developer Docker workflow with Makefile
Docker documentation / onboarding guide
```

---

## ✅ Project 10.2: GitHub Actions CI

### สิ่งที่ทำ

```text
เพิ่ม GitHub Actions workflow สำหรับ Backend + Frontend build
เพิ่ม .NET restore/build ใน Release mode
เพิ่ม Node.js setup และ npm cache
เพิ่ม Angular npm ci และ production build
เพิ่ม frontend build artifact upload
เพิ่ม Docker Compose config validation
เพิ่ม API Docker image build validation
เพิ่ม Web Docker image build validation
เพิ่ม GitHub Secrets pattern สำหรับ CI .env generation
เพิ่ม workflow_dispatch สำหรับ manual run
เพิ่ม branch filters สำหรับ main / master / develop
เพิ่ม path filters เพื่อลด CI runs ที่ไม่จำเป็น
เพิ่ม CodeQL workflow สำหรับ C# และ TypeScript security analysis
เพิ่ม Dependabot config สำหรับ NuGet / npm / GitHub Actions
แก้ Dependabot path ให้ถูกต้องที่ .github/dependabot.yml
แก้ CodeQL action version ให้ใช้ supported version
แก้ Angular dependency peer conflict หลัง Dependabot update
เพิ่ม GitHub Actions step summary
เพิ่ม CI / CodeQL status badges ใน Project.md และ README.md
```

### CI Flow

```text
Push / Pull Request / Manual Trigger
   ↓
MXHRM CI
   ├── Build Backend
   │      ├── dotnet restore
   │      └── dotnet build Release
   ├── Build Frontend
   │      ├── npm ci
   │      ├── npm run build
   │      └── upload mxhrm-web-dist artifact
   └── Docker Validation
          ├── docker compose config
          ├── docker build API image
          └── docker build Web image
```

### Security Automation Flow

```text
Push / Pull Request / Manual Trigger
   ↓
CodeQL
   ├── Analyze C#
   └── Analyze JavaScript / TypeScript

Weekly Schedule
   ↓
Dependabot
   ├── NuGet updates
   ├── npm updates
   └── GitHub Actions updates
```

### จุดสำคัญที่ได้เรียนรู้

```text
GitHub Actions workflow structure
CI trigger: push / pull_request / workflow_dispatch
Branch filters
Path filters
Job dependency with needs
GitHub Secrets
Temporary .env generation in CI
Build artifact upload
Docker build validation in CI
GitHub Actions step summary
Status badges
CodeQL code scanning
Dependabot package ecosystem config
Peer dependency conflict handling
```

---

## ✅ Project 10.3: Docker Image Registry + Release Tagging

### สิ่งที่ทำ

```text
เพิ่ม GitHub Container Registry login ใน CI
เพิ่ม packages: write permission ให้ docker job
เพิ่ม docker/metadata-action สำหรับ API image
เพิ่ม docker/metadata-action สำหรับ Web image
เปลี่ยน docker build เป็น docker/build-push-action
Push API image ไป GHCR
Push Web image ไป GHCR
เพิ่ม branch tag
เพิ่ม sha tag สำหรับ traceability / rollback
เพิ่ม latest tag เฉพาะ default branch
ตั้ง PR ให้ build-only และไม่ push image
เพิ่ม Docker image summary ใน GitHub Actions
เพิ่ม docker-compose.prod.yml สำหรับ pull image จาก GHCR
เพิ่ม IMAGE_TAG ใน .env.example
เพิ่ม Makefile prod commands
เพิ่ม multi-platform build: linux/amd64 + linux/arm64
แก้ platform warning บน Apple Silicon / ARM64
อัปเดต Docker.md สำหรับ production-style deployment ด้วย GHCR
```

### Registry Flow

```text
Push to main / master / develop
   ↓
GitHub Actions
   ↓
Build API + Web Docker images
   ↓
Tag images
   ├── branch tag
   ├── sha tag
   └── latest on default branch
   ↓
Push to GHCR
   ├── ghcr.io/oconneil/mxhrm-api
   └── ghcr.io/oconneil/mxhrm-web
```

### Deployment Pull Flow

```text
Deploy machine
   ↓
docker-compose.prod.yml
   ↓
IMAGE_TAG
   ↓
Pull images from GHCR
   ├── ghcr.io/oconneil/mxhrm-api:${IMAGE_TAG}
   └── ghcr.io/oconneil/mxhrm-web:${IMAGE_TAG}
   ↓
Run production-style stack
```

### Tag Strategy

```text
latest       = latest successful default branch build
master       = branch tag
sha-abc1234  = exact commit image for rollback / traceability
```

### จุดสำคัญที่ได้เรียนรู้

```text
GitHub Container Registry
GITHUB_TOKEN package publishing
Docker image metadata
Docker image tagging strategy
Branch tag / SHA tag / latest tag
Build-only PR policy
docker/build-push-action
Multi-platform Docker build
QEMU and Buildx
Production compose without local build
Deployment image version selection with IMAGE_TAG
Rollback by SHA image tag
```

---

## 🚧 Project 10.4: Environment-specific Compose + Production Hardening

Status: In Progress

### ทำถึงไหนแล้ว

```text
Step 1 completed - แยก production port exposure
Step 2 completed - เพิ่ม docker-compose.prod.local.yml สำหรับ local production debugging
Step 3 completed - เพิ่ม .env.production.example และ Docker.md production env note
Step 4 completed - เพิ่ม production runtime defaults และ web container hardening
```

### สิ่งที่ทำแล้ว

```text
ปรับ docker-compose.prod.yml ให้ production เปิด public port เฉพาะ web/nginx
ปิด public port ของ api / sqlserver / redis / seq ใน production compose
สร้าง docker-compose.prod.local.yml สำหรับ local debug/test
เพิ่ม Makefile prod-local commands
เพิ่ม .env.production.example สำหรับ production-style secrets template
อัปเดต Docker.md ให้แนะนำ production env setup
เพิ่ม TZ runtime variable
เพิ่ม DOTNET_ENVIRONMENT สำหรับ API
เพิ่ม web read_only hardening
เพิ่ม web tmpfs สำหรับ /var/cache/nginx /var/run /tmp
```

### Production Port Model

```text
Public entry point
   ↓
web / nginx
   ↓ internal Docker network
api
   ├── sqlserver
   ├── redis
   └── seq
```

### Environment Compose Model

```text
docker-compose.yml
   = local development build from source

docker-compose.prod.yml
   = hardened production-style compose pulling images from GHCR

docker-compose.prod.local.yml
   = local override for debugging production-style stack
```

### Step ที่เหลือใน Project 10.4

```text
Step 5 - เพิ่ม resource limits / reservations ให้ production services
Step 6 - ปรับ production logging policy และ Seq exposure strategy
Step 7 - เพิ่ม HTTPS-ready reverse proxy notes / TLS termination pattern
Step 8 - เพิ่ม backup / restore notes สำหรับ SQL Server volume
Step 9 - เพิ่ม deployment checklist ใน Docker.md / Project.md
Step 10 - ทดสอบ final production-style compose flow และสรุป Project 10.4
```

### จุดสำคัญที่ได้เรียนรู้แล้ว

```text
Production port exposure hardening
Internal-only infrastructure services
Docker Compose override files
Environment-specific compose pattern
Production secrets template
Runtime environment defaults
Container read-only filesystem baseline
tmpfs for writable runtime paths
```

---

# 📊 Current Status

```text
Progress: Core application complete through Project 10.4 Step 4; production hardening in progress
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
✅ Audit Report API
✅ Audit Report UI
✅ Audit Report Excel export
✅ Employee Summary PDF export with QuestPDF
✅ Report export pattern reusable across Employee and Audit
✅ GeneratedReports table
✅ Async report generation with Hangfire
✅ Generated Reports UI
✅ Report status tracking
✅ Download generated reports later
✅ report.manage permission
✅ SignalR backend hub
✅ JWT authentication for SignalR
✅ Generic realtime notification abstraction
✅ Angular SignalR client
✅ Generated report realtime status update
✅ Realtime UX indicator and status update
✅ UserNotifications persistent inbox table
✅ Notification read / unread API
✅ NSwag NotificationsClient integration
✅ Bell badge + Kendo Popup Notification Center
✅ Realtime toast notification
✅ Notification loading / refresh / retry UX
✅ Notification popup overlay above routed content
✅ OpenAPI response contracts for Employee / Role / User / Audit / Activity APIs
✅ NSwag EmployeesClient integration
✅ NSwag RolesClient / PermissionsClient / UsersClient integration
✅ NSwag AuditLogsClient / UserActivityLogsClient integration
✅ SwaggerException support in Angular Global Error UX
✅ Feature service generated DTO mapping boundary
✅ Hybrid NSwag response types + manual Kendo Grid state transport
✅ Auth manual transport retained intentionally for refresh flow
✅ API Dockerfile
✅ Angular + Nginx Dockerfile
✅ Docker Compose full stack environment
✅ SQL Server container
✅ Redis container
✅ Seq container
✅ Nginx reverse proxy for /api and /hubs
✅ Angular SPA fallback in Nginx
✅ API /health endpoint
✅ Docker healthchecks for SQL Server / API / Web
✅ Docker Compose profiles: infra / app
✅ .env / .env.example configuration split
✅ Makefile Docker workflow
✅ Docker.md developer guide
✅ Default admin user seeder
✅ GitHub Actions Backend build
✅ GitHub Actions Frontend build
✅ GitHub Actions Docker validation
✅ GitHub Actions workflow_dispatch manual run
✅ GitHub Actions branch filters
✅ GitHub Actions path filters
✅ GitHub Actions step summary
✅ Frontend build artifact upload
✅ GitHub Secrets pattern for CI
✅ CodeQL security analysis workflow
✅ Dependabot dependency update automation
✅ CI / CodeQL status badges
✅ GHCR image publishing
✅ Docker image metadata and release tags
✅ Branch / SHA / latest image tags
✅ PR build-only Docker policy
✅ Multi-platform Docker images
✅ docker-compose.prod.yml
✅ IMAGE_TAG-based production compose
✅ Makefile production commands
✅ GHCR deployment notes in Docker.md
✅ Production compose port hardening
✅ Internal-only API / SQL Server / Redis / Seq in production compose
✅ docker-compose.prod.local.yml
✅ Production local debug override compose
✅ .env.production.example
✅ Production env setup notes in Docker.md
✅ TZ runtime variable
✅ DOTNET_ENVIRONMENT production runtime config
✅ Web container read-only hardening
✅ Web tmpfs writable runtime paths
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
❌ Audit Report PDF export
❌ Project 10.4 remaining production hardening steps
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

## ✅ Project 9: Reporting + Export

Status: Completed

Completed parts:

```text
Project 9.1 Employee Summary Report API + UI
Project 9.2 Employee Summary Excel Export
Project 9.3 NSwag Client Generation
Project 9.4 Audit Report Screen
Project 9.5 Audit Report Excel Export
Project 9.6 Employee Summary PDF Export
Project 9.7 Async Report Generation with Hangfire
Project 9.8 Generic Realtime Notification with SignalR
Project 9.8.1 Realtime Notification Center
Project 9.9 Extend NSwag Usage
```

Completed output:

```text
Employee Summary Report ใช้งานได้
Audit Report ใช้งานได้
Report filter ใช้งานได้
Excel export/download ใช้งานได้ทั้ง Employee Summary และ Audit Report
PDF export/download ใช้งานได้สำหรับ Employee Summary Report
Async report generation ใช้งานได้ผ่าน Hangfire
Generated Reports UI ใช้สร้าง job / track status / download file ได้
SignalR ส่ง generated report status update แบบ realtime ได้
Angular RealtimeService เป็น connection กลาง
Generated Reports UI update row/status โดยไม่ต้อง refresh
Angular report service ใช้ NSwag generated ReportsClient
Swagger รองรับ file response สำหรับ code generation
ลด manual HttpClient สำหรับ report flow
มี QuestPDF backend PDF generation pattern
มี reusable ReportFileResponse และ FileResponse download pattern
ต่อยอดจาก Hangfire EmployeeReportJob เป็น download-later pattern แล้ว
มี generic realtime notification infrastructure สำหรับต่อยอด feature อื่น
มี persistent notification inbox ที่เก็บ unread/read state ต่อ user
มี Notification Center บน MainLayout ที่รับทั้ง API history และ SignalR live event
มี Kendo Popup / realtime toast / loading and error UX สำหรับ notification
มี OpenAPI contract ที่ระบุ success/error status สำหรับ feature API หลัก
มี NSwag typed client integration สำหรับ Employee และ Security Admin APIs
มี feature service mapper boundary ป้องกัน generated DTO กระทบ UI โดยตรง
มี hybrid Kendo Grid pattern: manual State transport + NSwag generated response type
คง Auth manual transport ไว้เพื่อควบคุม token storage และ refresh retry อย่างชัดเจน
```

Optional enhancement:

```text
Audit Report PDF Export
```

Expected final output:

```text
ระบบรายงานครบทั้ง Employee และ Audit
รองรับ download/export หลายรูปแบบ
รองรับ generated client เป็นมาตรฐานของ Angular API layer
รองรับ report generation แบบ background job ในอนาคต
รองรับ realtime status/notification สำหรับ background process
รองรับ persistent notification inbox และ read state ของผู้ใช้
รองรับ generated client เป็นมาตรฐานสำหรับ regular feature API
รองรับ server-side Kendo Grid โดยไม่เสีย paging/sort/filter state
```

---

## 🚧 Project 10: DevOps + Deployment

Status: In Progress

Completed:

```text
Project 10.1 Containerized Deployment Baseline
Project 10.2 GitHub Actions CI
Project 10.3 Docker Image Registry + Release Tagging
```

In progress:

```text
Project 10.4 Environment-specific Compose + Production Hardening
Current point: Step 4 completed
```

Completed output:

```text
Dockerized ASP.NET Core API
Dockerized Angular production app with Nginx
Docker Compose full stack environment
SQL Server / Redis / Seq containers
Nginx reverse proxy for API and SignalR
Container healthchecks and restart policy
Docker Compose profiles for infra/app workflows
Makefile developer commands
Docker.md onboarding guide
GitHub Actions backend/frontend build pipeline
Docker validation pipeline
CodeQL security scanning workflow
Dependabot update automation
CI artifact and summary reporting
Status badges in documentation
Docker image publish pipeline to GHCR
Branch / SHA / latest image tagging strategy
Multi-platform image builds for amd64/arm64
Production-style compose that pulls images from registry
IMAGE_TAG-based deployment flow
Production compose port hardening
Production local override compose
Production env template
Web read-only hardening baseline
```

Remaining scope:

```text
Project 10.4 Step 5 - Resource limits / reservations
Project 10.4 Step 6 - Production logging policy / Seq exposure strategy
Project 10.4 Step 7 - HTTPS termination pattern
Project 10.4 Step 8 - SQL Server backup / restore notes
Project 10.4 Step 9 - Deployment checklist
Project 10.4 Step 10 - Final production-style compose verification + lecture summary
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
PDF export
Async report generation
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
✔ Audit Report
✔ Excel export with ClosedXML
✔ PDF export with QuestPDF
✔ Reusable report file export pattern
✔ Async report generation with Hangfire
✔ Generated report status tracking
✔ Download-later report pattern
✔ SignalR realtime notification foundation
✔ Realtime generated report status updates
✔ Persistent User Notification Center
✔ Read / unread notification state
✔ Bell badge / popup inbox / realtime toast UX
✔ Swagger / OpenAPI client generation
✔ NSwag generated Angular API client
✔ Extended NSwag usage across Employee and Security Admin feature services
✔ OpenAPI success/error response contracts
✔ Feature Service DTO mapping boundary
✔ Hybrid NSwag + Kendo Grid integration
✔ Dockerized API and Angular frontend
✔ Nginx reverse proxy for SPA / API / SignalR
✔ Containerized SQL Server / Redis / Seq baseline
✔ Docker Compose profiles and healthchecks
✔ Developer Docker workflow with Makefile and Docker.md
✔ GitHub Actions CI foundation
✔ Docker validation in CI
✔ CodeQL security analysis workflow
✔ Dependabot dependency automation
✔ CI/CodeQL status badges
✔ Docker image publishing to GHCR
✔ Release tagging with branch / SHA / latest
✔ Multi-platform Docker images
✔ Production-style compose with IMAGE_TAG
✔ Environment-specific compose pattern
✔ Production port exposure hardening
✔ Production env template
✔ Web read-only hardening baseline
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
JWT + Refresh Token + Role + Permission DB Mapping + Security Admin APIs/UI + Better Error UX + Auto Retry + Layer Separation + Redis Cache + Advanced Filtering + Hangfire Jobs + Audit Trail + Kendo Grid + Custom Grid Adapter + Route UX + Reporting + Excel Export + PDF Export + Async Report Generation + SignalR + Persistent Notification Center + NSwag Client Generation
OpenAPI Contract-first Feature Services + Hybrid NSwag/Kendo Grid Integration
Dockerized Full-stack Runtime + Nginx Reverse Proxy + Containerized Infra Baseline
GitHub Actions CI Foundation + CodeQL + Dependabot Automation
GHCR Image Publishing + Release Tagging + Production Compose Pull Flow
Environment-specific Compose + Production Hardening Step 4
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
Project 9.4 completed - Audit Report Screen
Project 9.5 completed - Audit Report Excel Export
Project 9.6 completed - Employee Summary PDF Export
Project 9.7 completed - Async Report Generation with Hangfire
Project 9.8 completed - Generic Realtime Notification with SignalR
Project 9.8.1 completed - Realtime Notification Center
Project 9.9 completed - Extend NSwag Usage + Typed Feature Service Boundary
Project 10.1 completed - Containerized Deployment Baseline
Project 10.2 completed - GitHub Actions CI Foundation
Project 10.3 completed - Docker Image Registry + Release Tagging
Project 10.4 in progress - Environment-specific Compose + Production Hardening
Project 10.4 Step 4 completed - Runtime defaults + web read-only hardening
Next: Project 10.4 Step 5 Resource Limits / Reservations
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

# NSwag Generate
```bash
dotnet nswag run nswag.json
```
