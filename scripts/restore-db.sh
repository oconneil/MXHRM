#!/usr/bin/env bash
set -euo pipefail

source .env 2>/dev/null || source .env.production.example
DB="${MXHRM_DB_NAME:-MXHRM}"

# รับ path ไฟล์ .bak เป็น argument; ถ้าไม่ใส่ ฟ้อง error
BAK_FILE="${1:?Usage: ./scripts/restore-db.sh <backup-file.bak>}"

echo "♻️  Restoring [$DB] from $BAK_FILE"

# ส่งไฟล์ .bak จาก host เข้าไปใน container ก่อน
docker cp "$BAK_FILE" "mxhrm-sqlserver:/var/opt/mssql/backups/restore.bak"

docker run --rm --network "container:mxhrm-sqlserver" \
  mcr.microsoft.com/mssql-tools \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
  -Q "RESTORE DATABASE [$DB] FROM DISK = N'/var/opt/mssql/backups/restore.bak' WITH REPLACE, RECOVERY;"

echo "✅ Restore complete"