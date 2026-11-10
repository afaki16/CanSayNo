#!/usr/bin/env bash
# Günlük veritabanı yedeği: sıkıştırılmış pg_dump, son 14 gün tutulur.
# cron (root): 30 3 * * * /opt/cansayno/deploy/backup.sh >> /var/log/cansayno-backup.log 2>&1
# Geri yükleme: deploy/KURULUM.md › "Yedekten geri yükleme"
set -euo pipefail

cd "$(dirname "$0")"
BACKUP_DIR=/opt/cansayno-backups
KEEP_DAYS=14
mkdir -p "$BACKUP_DIR"
chmod 700 "$BACKUP_DIR"

# shellcheck disable=SC1091
source .env
file="$BACKUP_DIR/cansayno-$(date +%Y%m%d-%H%M%S).sql.gz"

docker compose exec -T db pg_dump -U "$POSTGRES_USER" -d cansayno --no-owner | gzip > "$file.tmp"
mv "$file.tmp" "$file"
find "$BACKUP_DIR" -name 'cansayno-*.sql.gz' -mtime +"$KEEP_DAYS" -delete

echo "$(date -Is) yedek: $file ($(du -h "$file" | cut -f1))"
