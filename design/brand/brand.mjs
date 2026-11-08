// CanSayNo marka geometrisi ve renkleri — TEK KAYNAK.
// Logo: mor→mavi geçişli zemin üzerinde beyaz konuşma balonu ve içinde mercan "dur" eli.
// (Seçim süreci: design/app-icon/balon-el/ — fikir, renk çalışması.)
//
// Mobilde aynı geometri Mobile/components/ui/BrandMark.tsx'te (react-native-svg) tekrar edilir;
// burada bir şey değişirse orayı da güncelle.

export const COLORS = {
  indigo: '#5B4BE0',      // geçişin başı (sol üst)
  blue: '#2D7FF9',        // geçişin sonu (sağ alt) = uygulamanın primary rengi
  coral: '#FF7043',       // "dur" eli
  white: '#FFFFFF',
  ink: '#16181D',
  cream: '#F8F7F4',
};

// 1024'lük tuvalde, sol üst → sağ alt
export function gradientDef(id = 'brand') {
  return `<linearGradient id="${id}" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${COLORS.indigo}"/><stop offset="1" stop-color="${COLORS.blue}"/></linearGradient>`;
}

// El: yerel koordinatlar, avuç merkezi (0,0). Dik "dur" eli; parmak araları küçük boyutta da ayrık.
export function hand(color) {
  const fw = 62;
  const fingers = [[-117, -262], [-39, -310], [39, -294], [117, -236]];
  const parts = fingers.map(([x, top]) =>
    `<line x1="${x}" y1="${top + fw / 2}" x2="${x}" y2="0" stroke="${color}" stroke-width="${fw}" stroke-linecap="round"/>`);
  parts.push(`<path d="M-148 -60 H148 V40 A130 130 0 0 1 18 170 H-18 A130 130 0 0 1 -148 40 Z" fill="${color}"/>`);
  parts.push(`<line x1="-104" y1="78" x2="-200" y2="-16" stroke="${color}" stroke-width="82" stroke-linecap="round"/>`);
  return parts.join('');
}

// Balon gövdesi + sol alt kuyruk (1024'lük tuval koordinatları)
export function bubble(color) {
  return `<rect x="182" y="176" width="660" height="590" rx="270" fill="${color}"/>` +
    `<path d="M352 640 L262 858 L500 740 Z" fill="${color}" stroke="${color}" stroke-width="44" stroke-linejoin="round"/>`;
}

export const HAND_TRANSFORM = 'translate(512 471) scale(0.8) translate(30 70)';

// Balon + el; scale tuval merkezine göre
export function mark({ bubbleColor = COLORS.white, handColor = COLORS.coral, scale = 1 } = {}) {
  return `<g transform="translate(512 512) scale(${scale}) translate(-512 -512)">${bubble(bubbleColor)}<g transform="${HAND_TRANSFORM}">${hand(handColor)}</g></g>`;
}

// Tek renk siluet: balon dolu, el boşluk (maske). Android temalı ikon + bildirim ikonu.
export function silhouette({ color = COLORS.white, scale = 1 } = {}) {
  return `<defs><mask id="cut" maskUnits="userSpaceOnUse" x="0" y="0" width="1024" height="1024">` +
    `${mark({ bubbleColor: '#fff', handColor: '#000', scale })}</mask></defs>` +
    `<rect width="1024" height="1024" fill="${color}" mask="url(#cut)"/>`;
}

// İkon içindeki balonun ölçeği (iOS ikonunda kenardan kenara zemin)
export const ICON_MARK_SCALE = 0.86;
// iOS köşe maskesine yakın yuvarlatma (uygulama içi logo, açılış ekranı, favicon)
export const ROUNDED_RADIUS = 0.2237;
