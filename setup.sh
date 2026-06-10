#!/bin/bash
# ============================================
#  Kickstart Template - Setup Script
#  macOS / Linux (Bash 4+)
# ============================================

set -e

ORIGINAL_NAME="Kickstart"
ORIGINAL_NAME_LOWER="kickstart"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# --- Renk kodlari ---
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
MAGENTA='\033[0;35m'
WHITE='\033[1;37m'
NC='\033[0m'

step()  { echo -e "  ${CYAN}[*]${NC} $1"; }
ok()    { echo -e "  ${GREEN}[+]${NC} $1"; }
err()   { echo -e "  ${RED}[!]${NC} $1"; }
warn()  { echo -e "  ${YELLOW}[~]${NC} $1"; }

# --- Banner ---
echo ""
echo -e "  ${MAGENTA}============================================${NC}"
echo -e "  ${MAGENTA} Kickstart Template - Otomatik Kurulum${NC}"
echo -e "  ${MAGENTA}============================================${NC}"
echo ""

# --- Proje adi al ---
PROJECT_NAME="${1:-}"

if [ -z "$PROJECT_NAME" ]; then
    read -p "  Proje adini girin (ornek: MyAwesomeProject): " PROJECT_NAME
fi

if [ -z "$PROJECT_NAME" ]; then
    err "Proje adi bos olamaz!"
    exit 1
fi

# Validasyon
if ! [[ "$PROJECT_NAME" =~ ^[a-zA-Z][a-zA-Z0-9]*$ ]]; then
    err "Proje adi harf ile baslamali ve sadece harf/rakam icermelidir."
    err "Ornek: MyProject, ECommerceApp, CRMSystem"
    exit 1
fi

PROJECT_NAME_LOWER=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]')

echo ""
step "Proje: $PROJECT_NAME"
echo ""

# --- 1. Dosya icerikleri degistir ---
step "Dosya icerikleri guncelleniyor..."

file_count=0
error_count=0

find "$SCRIPT_DIR" -type f \( \
    -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o \
    -name "*.http" -o -name "*.vue" -o -name "*.ts" -o -name "*.js" -o \
    -name "*.md" -o -name "*.css" -o -name "*.scss" \
\) -not -path "*/.git/*" -not -path "*/node_modules/*" -not -path "*/.template.config/*" | while read -r file; do
    if grep -q "$ORIGINAL_NAME\|$ORIGINAL_NAME_LOWER" "$file" 2>/dev/null; then
        if [[ "$OSTYPE" == "darwin"* ]]; then
            # macOS (BSD sed)
            sed -i '' "s/${ORIGINAL_NAME}/${PROJECT_NAME}/g" "$file" 2>/dev/null || true
            sed -i '' "s/${ORIGINAL_NAME_LOWER}/${PROJECT_NAME_LOWER}/g" "$file" 2>/dev/null || true
        else
            # Linux (GNU sed)
            sed -i "s/${ORIGINAL_NAME}/${PROJECT_NAME}/g" "$file" 2>/dev/null || true
            sed -i "s/${ORIGINAL_NAME_LOWER}/${PROJECT_NAME_LOWER}/g" "$file" 2>/dev/null || true
        fi
        file_count=$((file_count + 1))
    fi
done

ok "Dosya icerikleri guncellendi"

# --- 2. Dosya adlarini degistir ---
step "Dosya adlari guncelleniyor..."

renamed_files=0
find "$SCRIPT_DIR" -depth -type f -name "*${ORIGINAL_NAME}*" \
    -not -path "*/.git/*" -not -path "*/.template.config/*" | while read -r filepath; do
    dir=$(dirname "$filepath")
    base=$(basename "$filepath")
    newbase="${base//${ORIGINAL_NAME}/${PROJECT_NAME}}"
    if [ "$base" != "$newbase" ]; then
        mv "$filepath" "$dir/$newbase" 2>/dev/null || {
            warn "Dosya yeniden adlandirilamadi: $base"
        }
        renamed_files=$((renamed_files + 1))
    fi
done

ok "Dosya adlari guncellendi"

# --- 3. Klasor adlarini degistir ---
step "Klasor adlari guncelleniyor..."

renamed_dirs=0
# Birden fazla kez calistir (ic ice klasorler icin)
for i in 1 2 3; do
    find "$SCRIPT_DIR" -depth -type d -name "*${ORIGINAL_NAME}*" \
        -not -path "*/.git/*" -not -path "*/.template.config/*" 2>/dev/null | while read -r dirpath; do
        parent=$(dirname "$dirpath")
        base=$(basename "$dirpath")
        newbase="${base//${ORIGINAL_NAME}/${PROJECT_NAME}}"
        if [ "$base" != "$newbase" ]; then
            mv "$dirpath" "$parent/$newbase" 2>/dev/null || {
                warn "Klasor yeniden adlandirilamadi: $base"
            }
            renamed_dirs=$((renamed_dirs + 1))
        fi
    done
done

ok "Klasor adlari guncellendi"

# --- 4. Temizlik ---
step "Temizlik yapiliyor..."

# .template.config'i sil
if [ -d "$SCRIPT_DIR/.template.config" ]; then
    rm -rf "$SCRIPT_DIR/.template.config"
    ok ".template.config silindi"
fi

# EF Core migrations (yeni projede tek InitialCreate ile baslamak icin)
MIGRATIONS_DIR="$SCRIPT_DIR/Backend/$PROJECT_NAME.Infrastructure/Migrations"
if [ -d "$MIGRATIONS_DIR" ]; then
    rm -rf "$MIGRATIONS_DIR"
    ok "Migrations klasoru silindi (dotnet ef migrations add InitialCreate ...)"
fi

# Setup script'lerini sil
rm -f "$SCRIPT_DIR/setup.ps1" 2>/dev/null || true

# Kendini sil (en son)
SELF="$SCRIPT_DIR/setup.sh"

echo ""
echo -e "  ${GREEN}============================================${NC}"
echo -e "  ${GREEN} KURULUM TAMAMLANDI!${NC}"
echo -e "  ${GREEN}============================================${NC}"
echo ""
echo -e "  ${WHITE}Proje: $PROJECT_NAME${NC}"
echo ""
echo -e "  ${YELLOW}Sonraki adimlar:${NC}"
echo -e "  ${WHITE}  1. cd Backend && dotnet restore${NC}"
echo -e "  ${WHITE}  2. appsettings.json'dan PostgreSQL baglantisini duzenle${NC}"
echo -e "  ${WHITE}  3. dotnet ef migrations add InitialCreate --project $PROJECT_NAME.Infrastructure --startup-project $PROJECT_NAME.API${NC}"
echo -e "  ${WHITE}  4. dotnet ef database update --project $PROJECT_NAME.Infrastructure --startup-project $PROJECT_NAME.API${NC}"
echo -e "  ${WHITE}  5. dotnet run --project $PROJECT_NAME.API${NC}"
echo ""
echo -e "  ${WHITE}  6. cd Frontend && npm install && npm run dev${NC}"
echo ""
echo -e "  ${CYAN}Admin giris: admin@${PROJECT_NAME_LOWER}.com / Admin123!${NC}"
echo ""

# Kendini sil
rm -f "$SELF" 2>/dev/null || true
