# 🏛️ MXHRM — Clean Architecture

เอกสารนี้อธิบาย Clean Architecture ที่ใช้ใน MXHRM แบบเข้าใจง่าย พร้อมตัวอย่างจากโค้ดจริง

---

## 🎯 หัวใจเดียวที่ต้องจำ: Dependency Rule

> **ลูกศรการพึ่งพา (dependency) ชี้เข้าด้านในเสมอ — ชั้นในไม่รู้จักชั้นนอก**

```text
        🔴 API  (นอกสุด)
           ↓ พึ่งพา
        🟢 Application
           ↓ พึ่งพา
        🟡 Domain  (ในสุด — ไม่พึ่งใครเลย)

   🔵 Infrastructure ──→ พึ่ง Application + Domain
   (API ต่อ Infrastructure เข้าระบบผ่าน DI)
```

Domain ไม่รู้ว่ามี SQL Server, ไม่รู้ว่ามี HTTP, ไม่รู้ว่ามี Angular — มันรู้แค่ "พนักงานคืออะไร"

---

## 🍜 เปรียบเทียบ: ร้านอาหาร

| Layer | ในร้านอาหาร | หน้าที่ |
|---|---|---|
| 🟡 **Domain** | สูตรอาหาร + กฎของจาน | "ผัดกะเพราต้องมีอะไรบ้าง" — ไม่สนว่าใช้เตาแก๊สหรือเตาไฟฟ้า |
| 🟢 **Application** | ขั้นตอนการทำงานของเชฟ | "พอออเดอร์เข้า → ทำตามนี้" + บอกว่า "ฉันต้องการคนส่งวัตถุดิบ" (แต่ไม่สนว่าใคร) |
| 🔵 **Infrastructure** | ซัพพลายเออร์จริง + เตา + ตู้เย็น | Makro, ตู้เย็นยี่ห้อนี้ (= SQL Server, Redis) — "ของจริง" ที่จับต้องได้ |
| 🔴 **API** | พนักงานเสิร์ฟ + หน้าร้าน | รับออเดอร์จากลูกค้า (HTTP) → ส่งเข้าครัว → เอาผลลัพธ์มาเสิร์ฟ |

**กุญแจสำคัญ:** สูตรอาหาร (Domain) ไม่เปลี่ยนเลย ต่อให้เปลี่ยนเตา เปลี่ยนซัพพลายเออร์ (เปลี่ยน SQL Server → PostgreSQL) เพราะมันอยู่ "ชั้นใน" ที่ไม่รู้จักของพวกนั้น

---

## 📂 4 ชั้น + ตัวอย่างจริงในโปรเจกต์

### 🟡 1. Domain — "แก่นธุรกิจ"

**กฎ:** ไม่พึ่งใครเลย (ไม่มี `using` ของ EF Core / ASP.NET / Redis)

```text
Employees/Employee.cs          ← entity พนักงาน
Common/BaseEntity.cs           ← CompanyID, RowVersion, CreatedDate...
Notifications/UserNotification.cs
Reports/GeneratedReport.cs
```

`Employee.cs` มีแต่ property (FirstName, Salary, IsActive) ไม่มีโค้ดต่อ DB เลย = "สูตรอาหาร" ล้วนๆ

### 🟢 2. Application — "use case + สัญญา (contract)"

**กฎ:** พึ่งแค่ Domain / ห้ามรู้จัก EF Core, Redis, HTTP

```text
Employees/IEmployeeService.cs       ← interface (สัญญาว่าทำอะไรได้บ้าง)
Common/Interfaces/ICacheService.cs  ← "ฉันต้องการ cache" (แต่ไม่บอกว่า Redis)
Employees/DTOs/...                  ← รูปร่างข้อมูลเข้า-ออก
Employees/Validators/...            ← กฎ validate
Authorization/Permissions.cs        ← ค่าคงที่ permission
```

มีแต่ **interface (I...)** กับ DTO กับ validator — ไม่มี implementation จริง เพราะ Application แค่ "บอกว่าต้องการอะไร" ไม่ใช่ "ทำยังไง"

### 🔵 3. Infrastructure — "ของจริงที่จับต้องได้"

**กฎ:** พึ่ง Application + Domain / เป็นที่อยู่ของ "เทคโนโลยีจริง" ทั้งหมด

```text
Employees/EmployeeService.cs   ← implement IEmployeeService (ทำจริง ต่อ DB)
Caching/RedisCacheService.cs   ← implement ICacheService (ต่อ Redis จริง)
Data/AppDbContext.cs           ← EF Core
Auth/AuthService.cs            ← Identity, JWT
```

ทุกอย่างที่ "ผูกกับเทคโนโลยีเฉพาะ" (EF, Redis, Hangfire, QuestPDF) อยู่ที่นี่หมด เพื่อให้ชั้นในไม่เปื้อน

### 🔴 4. API — "ประตูหน้า + ผู้ประกอบร่าง"

**กฎ:** พึ่ง Application (เรียกผ่าน interface) + ต่อ Infrastructure เข้าระบบผ่าน DI

```text
Controllers/EmployeesController.cs   ← รับ HTTP → เรียก IEmployeeService
Program.cs                           ← "ประกอบร่าง" (AddInfrastructure)
Middlewares/, Authorization/Handler  ← เรื่อง HTTP/auth
```

---

## 🔑 จุดที่งงที่สุด: Dependency Inversion

**ทำไม `ICacheService` อยู่ Application แต่ `RedisCacheService` อยู่ Infrastructure?**

🍅 **เปรียบเทียบ:** เชฟ (Application) เขียนใบสั่งว่า *"ฉันต้องการคนส่งมะเขือเทศ"* (= `ICacheService` — สัญญา) เชฟ **เป็นเจ้าของใบสั่ง** เพราะเชฟเป็นคน "ต้องการ"

ส่วน **ใครจะมาส่ง** (Makro / ตลาดสด = `RedisCacheService`) เป็นเรื่องของซัพพลายเออร์ (Infrastructure) ที่มา "ทำตามสัญญา"

```text
Application:    "ฉันต้องการ ICacheService"            (เจ้าของสัญญา = คนที่ต้องการ)
                          ↑ implement
Infrastructure: RedisCacheService : ICacheService     (คนทำตามสัญญา)
```

ผลคือ **ลูกศรชี้เข้าใน** (Infrastructure → Application) ถ้าเปลี่ยนจาก Redis เป็น Memcached — แก้แค่ Infrastructure, Application/Domain ไม่ต้องแตะเลย

> 🧠 หลักคิด: **"คนที่ต้องการ เป็นเจ้าของ interface / คนที่ทำได้ เป็นคน implement"**

---

## 🧭 หลักคิด: "ของชิ้นนี้ควรอยู่ layer ไหน?"

ถามตัวเอง 4 คำถามตามลำดับ:

```text
1. เป็น "กฎธุรกิจแท้ๆ" ที่ไม่เกี่ยวเทคโนโลยีไหม?
   → ใช่: 🟡 Domain (entity, business rule)

2. เป็น "สัญญา/use case/รูปร่างข้อมูล/กฎ validate" ไหม?
   → ใช่: 🟢 Application (interface, DTO, validator)

3. ผูกกับเทคโนโลยีเฉพาะไหม (EF, Redis, JWT, PDF, email)?
   → ใช่: 🔵 Infrastructure (implement interface จาก Application)

4. เกี่ยวกับ HTTP / routing / รับ request / ประกอบร่างไหม?
   → ใช่: 🔴 API (controller, middleware, Program.cs)
```

**ตัวอย่าง:** เพิ่มฟีเจอร์ "ส่ง email ต้อนรับพนักงานใหม่"

| ของ | layer |
|---|---|
| `IEmailService` (สัญญา "ฉันต้องการส่ง email") | 🟢 Application |
| `WelcomeEmailRequest` (DTO) | 🟢 Application |
| `SmtpEmailService` (ต่อ SMTP จริง) | 🔵 Infrastructure |
| เรียก `_emailService.Send(...)` ใน `EmployeeService.CreateAsync` | 🔵 Infrastructure |

---

## 🚶 เดินตาม 1 request จริง (Employee Create)

```text
🔴 EmployeesController.Create(request)        ← รับ HTTP POST
     ↓ เรียกผ่าน interface
🟢 IEmployeeService.CreateAsync(request)      ← สัญญา (Application)
     ↓ DI ชี้ไปที่ implementation จริง
🔵 EmployeeService.CreateAsync(request)       ← ทำจริง (Infrastructure)
     ├─ สร้าง 🟡 Employee entity (Domain)
     ├─ _db.Employees.Add() → SaveChanges (EF Core)
     └─ _cache.RemoveByPrefixAsync() (ผ่าน ICacheService)
     ↓
🔵 AppDbContext → SQL Server
```

Controller (🔴) ไม่รู้เลยว่าข้างในใช้ EF Core หรือ Redis — มันรู้แค่ `IEmployeeService` ที่เป็นสัญญาจาก 🟢 Application นี่คือ "ความสะอาด" ที่ทำให้เปลี่ยนเทคโนโลยีข้างในได้โดยไม่กระทบข้างนอก

---

## 🔍 Litmus Test — เช็คว่าทำถูกไหม

| เช็ค | ถ้าผ่าน = ถูก |
|---|---|
| Domain มี `using Microsoft.EntityFrameworkCore` ไหม? | ❌ ไม่ควรมี |
| Application มี `using` ของ Redis/EF/Hangfire ไหม? | ❌ ไม่ควรมี |
| Controller เรียก `AppDbContext` ตรงๆ ไหม? | ❌ ไม่ควร (ต้องผ่าน interface) |
| ลบ project Infrastructure ทิ้ง แล้ว Application ยัง compile ได้ไหม? | ✅ ต้องได้ |

ข้อสุดท้ายคือ "การทดสอบสูงสุด" — Application/Domain ต้องไม่รู้จัก Infrastructure เลย

---

## ⚠️ ข้อผิดพลาดที่เจอบ่อย

```text
❌ ใส่ EF Core attribute/logic ใน Domain entity (Domain เปื้อนเทคโนโลยี)
❌ Controller เรียก AppDbContext หรือ DbSet ตรงๆ (ข้าม Application)
❌ เอา interface ไปไว้ Infrastructure ข้างๆ implementation (ต้องอยู่ Application)
❌ ใส่ business logic หนักๆ ใน Controller (Controller ควรบาง = เรียก service แล้วจบ)
❌ Application อ้างอิง Infrastructure (ลูกศรชี้ออกนอก = ผิดหลัก)
```

---

## 🎁 ได้อะไรจากการทำแบบนี้

```text
✅ เปลี่ยนเทคโนโลยีง่าย (SQL→Postgres, Redis→Memcached) แก้แค่ Infrastructure
✅ เทสต์ง่าย (mock interface ได้)
✅ business logic ไม่ปนกับ technical detail
✅ ทีมแบ่งงานได้ (คนทำ Domain logic ไม่ต้องรู้เรื่อง Redis)
```

> 🔗 **เชื่อมกับ Testing:** ที่ unit test ของ MXHRM `mock ICacheService` ได้ง่ายๆ ก็เพราะ Clean Architecture — `EmployeeService` พึ่ง `ICacheService` (สัญญา) ไม่ใช่ `RedisCacheService` (ของจริง) จึงสลับเป็นของปลอมตอนเทสต์ได้
