# Canlı Sunucu Kurulumu (api.cansayno.com)

Tek sunucu: Hetzner Cloud (Almanya) üzerinde Docker — Postgres + API + Caddy (HTTPS).
E-posta: Brevo SMTP. Dosyalar bu klasörde: `docker-compose.yml`, `Caddyfile`, `.env.example`, `backup.sh`.

Yerelde denendi (2026-11-10): imaj derleniyor, Production ortamında migration + seed (65 TR + 45 EN),
`/health`, admin girişi, Swagger kapalı, güvenlik başlıkları, `pg_dump`.

---

## 1. Hesaplar

| Servis | Ne için | Not |
|---|---|---|
| Hetzner Cloud | Sunucu | Kart/PayPal; ilk hesapta kimlik doğrulaması isteyebilir |
| Brevo | Doğrulama / şifre kodu e-postaları | Ücretsiz plan günde 300 e-posta |
| Anthropic Console | Sunucuya ayrı API anahtarı | Aylık harcama limiti koy |

## 2. Sunucu (Hetzner Cloud Console)

1. **Create Server**: Konum **Nuremberg** veya **Falkenstein** (gizlilik politikasında "Almanya" yazıyor) ·
   Görüntü **Ubuntu 24.04** · Tip: paylaşımlı x86, **2 vCPU / 4 GB** (en küçüğü yeter) ·
   **SSH key** ekle (şifreyle giriş yok) · **Backups** işaretle (+%20, 7 günlük tam disk yedeği).
2. **Firewall** oluştur ve sunucuya bağla — gelen: TCP 22 (mümkünse yalnızca kendi IP'n), TCP 80, TCP 443, UDP 443.
3. Sunucunun IPv4 (ve IPv6) adresini not et.

SSH anahtarın yoksa (Windows PowerShell): `ssh-keygen -t ed25519` → `~/.ssh/id_ed25519.pub` içeriğini Hetzner'e yapıştır.

## 3. DNS (Cloudflare › cansayno.com › DNS)

| Tür | Ad | Değer | Proxy |
|---|---|---|---|
| A | `api` | sunucu IPv4 | **DNS only (gri bulut)** |
| AAAA | `api` | sunucu IPv6 | **DNS only** |

Gri bulut şart: Caddy sertifikayı kendisi alır, API gerçek istemci IP'sini görür (IP başına giriş/kayıt sınırları).

## 4. Sunucuyu hazırla

```bash
ssh root@<IP>
apt update && apt -y upgrade && apt -y install unattended-upgrades git
dpkg-reconfigure -plow unattended-upgrades          # güvenlik güncellemeleri otomatik
sed -i 's/^#\?PasswordAuthentication.*/PasswordAuthentication no/' /etc/ssh/sshd_config && systemctl restart ssh
curl -fsSL https://get.docker.com | sh               # Docker + compose eklentisi
```

## 5. Kodu al

Repo özel ise salt okunur **deploy key** kullan:

```bash
ssh-keygen -t ed25519 -f ~/.ssh/cansayno_deploy -N ""
cat ~/.ssh/cansayno_deploy.pub     # GitHub › repo › Settings › Deploy keys › Add (yazma izni VERME)
cat >> ~/.ssh/config <<'EOF'
Host github.com
  IdentityFile ~/.ssh/cansayno_deploy
EOF
git clone git@github.com:<kullanıcı>/<repo>.git /opt/cansayno
```

## 6. Ayarlar ve ilk açılış

```bash
cd /opt/cansayno/deploy
cp .env.example .env && chmod 600 .env
nano .env        # tüm alanları doldur (rastgele değerler: openssl rand -base64 48 | tr -d '/+=' | cut -c1-48)
docker compose up -d --build        # ilk derleme birkaç dakika
docker compose logs -f api          # "Now listening on" görünce Ctrl+C
curl https://api.cansayno.com/health   # {"status":"ok"}
```

Sonra: `.env`'de `INITIAL_SUPERADMIN_PASSWORD` / `INITIAL_ADMIN_PASSWORD` satırlarını boşalt
(şifreler parola yöneticinde kalsın) → `docker compose up -d`.

## 7. Brevo (e-posta)

1. **Senders, Domains & Dedicated IPs › Domains › Add domain** → `cansayno.com` → "Authenticate" ile
   verdiği kayıtları (Brevo kodu TXT, DKIM, DMARC) Cloudflare DNS'e **DNS only** ekle; doğrulanmasını bekle.
2. **Senders** → `noreply@cansayno.com` ekle.
3. **SMTP & API › SMTP** → "Generate a new SMTP key". `.env`: `SMTP_USER` = sayfadaki *Login*, `SMTP_PASSWORD` = anahtar.
4. `docker compose up -d` → uygulamadan kayıt ol, kod/link e-postasının geldiğini ve spam'e düşmediğini kontrol et.

## 8. Yedekler

```bash
chmod +x /opt/cansayno/deploy/backup.sh
/opt/cansayno/deploy/backup.sh                  # elle bir kez dene
crontab -e    # ekle: 30 3 * * * /opt/cansayno/deploy/backup.sh >> /var/log/cansayno-backup.log 2>&1
```

Günlük `pg_dump` (14 gün, `/opt/cansayno-backups`) + Hetzner Backups (sunucunun tamamı, 7 gün).
Sunucu tamamen kaybolursa diye ara sıra bir yedeği kendi bilgisayarına indir:
`scp root@<IP>:/opt/cansayno-backups/<dosya> .`

**Yedekten geri yükleme**

```bash
cd /opt/cansayno/deploy
docker compose stop api
docker compose exec -T db psql -U cansayno -d postgres -c 'DROP DATABASE cansayno' -c 'CREATE DATABASE cansayno'
gunzip -c /opt/cansayno-backups/<dosya>.sql.gz | docker compose exec -T db psql -U cansayno -d cansayno
docker compose start api
```

## 9. Günlük işler

| İş | Komut (`/opt/cansayno/deploy` içinde) |
|---|---|
| Yeni sürüm | `git pull && docker compose up -d --build` (migration açılışta kendiliğinden) |
| Loglar | `docker compose logs --tail 200 api` |
| Durum | `docker compose ps` · `curl https://api.cansayno.com/health` |
| Veritabanı | `docker compose exec db psql -U cansayno -d cansayno` |
| Disk temizliği | `docker image prune -f` |

Kesinti bildirimi için UptimeRobot (ücretsiz) → `https://api.cansayno.com/health`, 5 dk.

## 10. Bağlantılar

- Mobil üretim yapısı zaten `https://api.cansayno.com`'a bakıyor (`Mobile/eas.json`).
- RevenueCat webhook URL: `https://api.cansayno.com/api/Webhooks/revenuecat`, Authorization = `.env`'deki `REVENUECAT_WEBHOOK_AUTH`.
- Admin paneli bu kurulumda YOK (yalnızca API). İleride `admin.cansayno.com` olarak eklenebilir.
