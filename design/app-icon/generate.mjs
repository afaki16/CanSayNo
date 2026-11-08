// CanSayNo uygulama ikonları — Mobile/components/ui/BrandMark.tsx geometrisinden birebir.
// BrandMark: kare (köşe yarıçapı 0.32·kenar) içinde ok; okun SVG kutusu 0.57·kenar genişliğinde,
// viewBox 16×12, yol M2 10 L8 3 L14 10, çizgi 2.8, uçlar ve köşe yuvarlak.
//
//   node generate.mjs  → svg/*.svg + png/*.png (PNG'ler headless Chrome ile, şeffaflık korunur)
//
// Çıktılar Mobile/assets/images/ içindeki aynı adlı dosyaların yerine geçecek adaylardır.

import { execFileSync } from 'node:child_process';
import { mkdirSync, writeFileSync, rmSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const BLUE = '#2D7FF9';
const CHROME = process.env.CHROME ?? 'C:/Program Files/Google/Chrome/Application/chrome.exe';

// Oku, verilen merkez ve "BrandMark kenarı" ölçüsünde çizer.
function chevron(cx, cy, markSize, color = '#FFFFFF') {
  const w = markSize * 0.57;          // okun SVG kutusu genişliği
  const s = w / 16;                   // viewBox birimi → piksel
  const h = 12 * s;
  const x0 = cx - w / 2, y0 = cy - h / 2;
  const p = (x, y) => `${(x0 + x * s).toFixed(2)} ${(y0 + y * s).toFixed(2)}`;
  return `<path d="M${p(2, 10)} L${p(8, 3)} L${p(14, 10)}" fill="none" stroke="${color}" stroke-width="${(2.8 * s).toFixed(2)}" stroke-linecap="round" stroke-linejoin="round"/>`;
}

const svg = (size, body) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">${body}</svg>\n`;

const N = 1024;
// Android adaptive: 108dp tuval, launcher ortadaki 72dp'yi gösterir (%66.7). Görünen alanda
// ok, iOS ikonundaki oranla aynı büyüklükte olsun → BrandMark kenarı = tuvalin %66.7'si.
const ANDROID_VISIBLE = N * 72 / 108;

const files = {
  // iOS + genel ikon: kenardan kenara mavi (köşeleri mağaza/OS keser), ortada ok.
  'icon': svg(N, `<rect width="${N}" height="${N}" fill="${BLUE}"/>${chevron(N / 2, N / 2, N)}`),
  // Android adaptive katmanları
  'android-icon-foreground': svg(N, chevron(N / 2, N / 2, ANDROID_VISIBLE)),
  'android-icon-background': svg(N, `<rect width="${N}" height="${N}" fill="${BLUE}"/>`),
  // Tek renk: Android 13 temalı ikon + bildirim ikonu (sistem renklendirir; şekil önemli)
  'android-icon-monochrome': svg(N, chevron(N / 2, N / 2, ANDROID_VISIBLE)),
  // Açılış ekranı: tam logo (yuvarlak kare + ok), şeffaf zemin
  'splash-icon': svg(N, `<rect width="${N}" height="${N}" rx="${N * 0.32}" fill="${BLUE}"/>${chevron(N / 2, N / 2, N)}`),
  // Web favicon (Expo web çıktısı)
  'favicon': svg(48, `<rect width="48" height="48" rx="${48 * 0.32}" fill="${BLUE}"/>${chevron(24, 24, 48)}`),
};

const svgDir = join(here, 'svg'), pngDir = join(here, 'png'), tmp = join(here, '.tmp');
for (const d of [svgDir, pngDir, tmp]) mkdirSync(d, { recursive: true });

for (const [name, content] of Object.entries(files)) {
  writeFileSync(join(svgDir, `${name}.svg`), content);
  const size = Number(content.match(/width="(\d+)"/)[1]);
  const html = join(tmp, `${name}.html`);
  writeFileSync(html, `<!doctype html><html><head><style>html,body{margin:0;background:transparent;overflow:hidden}svg{display:block}</style></head><body>${content}</body></html>`);
  execFileSync(CHROME, [
    '--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--default-background-color=00000000', `--window-size=${size},${size}`,
    `--screenshot=${join(pngDir, `${name}.png`)}`, pathToFileURL(html).href,
  ], { stdio: 'ignore' });
  console.log(`✔ ${name}.png (${size}×${size})`);
}
rmSync(tmp, { recursive: true, force: true });
