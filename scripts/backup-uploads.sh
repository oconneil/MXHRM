#!/usr/bin/env bash
set -euo pipefail   # เจอ error ปุ๊บหยุดทันที ไม่ปล่อยให้พังเงียบ

# โหลด .env เผื่อมี UPLOADS_VOLUME กำหนดไว้ (uploads ไม่ต้องใช้ secret เหมือน DB)
source .env 2>/dev/null || true

STAMP="$(date +%Y%m%d-%H%M%S)"

# หา volume ที่เก็บไฟล์อัปโหลด — ชื่อจริงมี prefix ตาม compose project (เช่น mxhrm_mxhrm-uploads)
# override ได้ด้วยตัวแปร UPLOADS_VOLUME ใน .env ถ้า auto-detect ไม่ตรง
VOLUME="${UPLOADS_VOLUME:-$(docker volume ls --format '{{.Name}}' | grep 'mxhrm-uploads' | head -1)}"

if [ -z "$VOLUME" ]; then
  echo "❌ ไม่พบ volume ที่ชื่อมี 'mxhrm-uploads' — รัน 'docker volume ls' เพื่อหาชื่อ แล้วตั้ง UPLOADS_VOLUME ใน .env"
  exit 1
fi

OUT="uploads-${STAMP}.tar.gz"
echo "📦 Backing up uploads volume [$VOLUME] → ./backups/$OUT"

mkdir -p backups

# mount volume แบบ read-only เข้า helper container (alpine มี tar ติดมา) แล้ว tar ออกมาที่ host
# ไม่ต้องพึ่ง container api รันอยู่ → backup ได้แม้แอปดับ (กฎ 3-2-1: เอาออกนอก container)
docker run --rm \
  -v "${VOLUME}:/data:ro" \
  -v "$(pwd)/backups:/backup" \
  alpine tar czf "/backup/${OUT}" -C /data .

echo "✅ Saved to ./backups/${OUT} — แนะนำ upload ไป cloud/off-site ต่อ"
echo "ℹ️  restore: docker run --rm -v ${VOLUME}:/data -v \"\$(pwd)/backups:/backup\" alpine tar xzf /backup/${OUT} -C /data"
