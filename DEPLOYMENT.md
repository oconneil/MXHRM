# MXHRM Production Deployment Checklist

## Phase 1 — Pre-deploy (เตรียมก่อนปล่อย)

- [ ] สร้าง `.env` จริงบน server (คัดลอกจาก `.env.production.example`)
- [ ] เปลี่ยน secret ทุกตัวที่ยังเป็น `change-me-...`:
  - [ ] `MSSQL_SA_PASSWORD` (strong, ≥ 8 ตัว มีตัวใหญ่/เลข/อักขระพิเศษ)
  - [ ] `JWT_SECRET_KEY` (random ≥ 32 ตัวอักษร)
  - [ ] `SEQ_ADMIN_PASSWORD`
- [ ] ตั้ง `IMAGE_TAG` เป็น sha/version ที่เจาะจง (อย่าใช้ `latest` บน prod — รู้ไม่ได้ว่าได้ build ไหน)
- [ ] DNS A record ของโดเมนชี้มาที่ public IP ของ server
- [ ] แก้ `Caddyfile`: ใส่โดเมนจริง + ลบบรรทัด `tls internal`
- [ ] ตรวจ RAM ของ server พอสำหรับ reservations รวม (sql 2g + api 384m + seq 256m + ...)
- [ ] Firewall: เปิดเฉพาะ 80 / 443 / 22 (ssh); ปิด 1433 / 6379 / 5341 / 8080 จากภายนอก
- [ ] ยืนยัน volume `mxhrm-uploads` ถูกประกาศใน compose และ mount ที่ `api → /app/App_Data/uploads`
      (ไฟล์อัปโหลด รูป/เอกสารพนักงาน ต้องอยู่บน volume — ไม่งั้นหายทุกครั้งที่ deploy ใหม่)

## Phase 2 — Deploy (ปล่อย)

- [ ] `docker login ghcr.io` (ใช้ GitHub token)
- [ ] `docker compose -f docker-compose.prod.yml -f docker-compose.prod.tls.yml pull`
- [ ] `docker compose -f docker-compose.prod.yml -f docker-compose.prod.tls.yml up -d`
- [ ] รอ healthcheck เขียวทุกตัว: `docker compose ps` (status = healthy)
- [ ] apply database migration (แอปไม่ auto-migrate) — สร้าง idempotent script แล้วรันเข้า SQL:

  ```bash
  # บนเครื่อง dev: สร้าง script (รันซ้ำได้ ไม่พัง)
  dotnet ef migrations script --idempotent -o migrate.sql \
    -p src/MXHRM.Infrastructure -s src/MXHRM.Api

  # รันเข้า container sqlserver (azure-sql-edge ไม่มี sqlcmd → ใช้ mssql-tools ชั่วคราว)
  docker run --rm --network container:mxhrm-sqlserver -v "$(pwd)/migrate.sql:/m.sql" \
    mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d "$MXHRM_DB_NAME" -i /m.sql
  ```

## Phase 3 — Post-deploy verification (พิสูจน์ว่าใช้ได้จริง)

- [ ] เปิด `https://<domain>` เห็นหน้า login + มี 🔒 (cert ถูกต้อง)
- [ ] Login ด้วย seeded admin สำเร็จ
- [ ] Realtime notification (SignalR `/hubs`) เชื่อมต่อติด — กระดิ่งทำงาน
- [ ] สร้าง / export report (Excel/PDF) ได้
- [ ] อัปโหลดรูป/เอกสารพนักงาน → `restart` container `api` → ไฟล์ยังอยู่ (พิสูจน์ volume persist)
- [ ] Seq (ผ่าน SSH tunnel) มี log เข้าจริง
- [ ] รัน `./scripts/backup-db.sh` สำเร็จ + ทดสอบ restore ใน staging

## Phase 4 — Rollback plan (เตรียมไว้ก่อนเสมอ)

- [ ] มี backup ล่าสุด "ก่อน" deploy รอบนี้
- [ ] วิธี rollback image: เปลี่ยน `IMAGE_TAG` เป็น sha เก่า → `up -d` ใหม่
- [ ] รู้วิธี restore DB: `./scripts/restore-db.sh <backup.bak>`

## Ongoing — Operational (หลัง deploy เดินเครื่อง)

- [ ] ตั้ง cron backup รายวัน (cd เข้า project ก่อน เพราะ `source .env`)
- [ ] เฝ้าดู disk usage (log rotation ช่วยแล้ว แต่ volume DB / backups / uploads โตได้)
- [ ] รวม volume `mxhrm-uploads` เข้าแผน backup ด้วย — `./scripts/backup-uploads.sh` (ไฟล์อัปโหลดไม่ได้อยู่ใน DB)
- [ ] เฝ้าดู Let's Encrypt cert ต่ออายุอัตโนมัติ (Caddy จัดการ แต่ verify ครั้งแรก)