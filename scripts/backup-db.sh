#!/usr/bin/env bash
set -euo pipefail   # เจอ error ปุ๊บหยุดทันที ไม่ปล่อยให้พังเงียบ

# โหลด secret จาก .env (ถ้าไม่มี ใช้ไฟล์ example สำหรับเทสต์)
source .env 2>/dev/null || source .env.production.example

DB="${MXHRM_DB_NAME:-MXHRM}"
STAMP="$(date +%Y%m%d-%H%M%S)"
BAK="/var/opt/mssql/backups/${DB}-${STAMP}.bak"

echo "📦 Backing up [$DB] → $BAK"

# ใช้ mssql-tools container ยิงคำสั่ง BACKUP ผ่าน network ของ sqlserver
# --network "container:mxhrm-sqlserver" = แชร์ network กับ sqlserver
#   → เรียก -S localhost ได้เลย ไม่ต้องรู้ชื่อ docker network
docker run --rm --network "container:mxhrm-sqlserver" \
  mcr.microsoft.com/mssql-tools \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
  -Q "BACKUP DATABASE [$DB] TO DISK = N'$BAK' WITH INIT, FORMAT, NAME = N'$DB full backup';"

# ดึงไฟล์ .bak ออกมาเก็บบน host (กฎ 3-2-1: เอาออกนอก container)
mkdir -p backups
docker cp "mxhrm-sqlserver:${BAK}" "./backups/"
echo "✅ Saved to ./backups/${DB}-${STAMP}.bak — แนะนำ upload ไป cloud/off-site ต่อ"