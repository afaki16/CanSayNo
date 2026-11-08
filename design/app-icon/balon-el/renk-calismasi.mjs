// Zemin rengi çalışması: C varyasyonunun (beyaz balon + mercan el) zemini değiştirilir.
//   node renk-calismasi.mjs → renk/*.png + renk.html
// Her aday için WCAG kontrast oranı da hesaplanır (beyaz balon ↔ zemin): ikon küçükken
// balonun zeminden ayrılması için en az ~3:1 iyi.

import { execFileSync } from 'node:child_process';
import { mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const CHROME = process.env.CHROME ?? 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const base = readFileSync(join(here, 'svg', 'c-mercan-el.svg'), 'utf8');
const BG = '<rect width="1024" height="1024" fill="#2D7FF9"/>';
if (!base.includes(BG)) throw new Error('Zemin dikdörtgeni bulunamadı — generate.mjs çıktısı değişmiş.');

const grad = (id, a, b) =>
  `<defs><linearGradient id="${id}" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${a}"/><stop offset="1" stop-color="${b}"/></linearGradient></defs><rect width="1024" height="1024" fill="url(#${id})"/>`;

const candidates = [
  { id: 'mavi', bg: '#2D7FF9', name: 'Mavi (şu anki)', note: 'Güven, yetkinlik. Mağazada en kalabalık renk; mesajlaşma uygulamalarına benzer.' },
  { id: 'lacivert', bg: '#1E2B6B', name: 'Gece laciverti', note: 'Sakin, olgun, kendinden emin. Mercan el en güçlü kontrastla parlar. Uygulamanın mavisiyle aynı aile.' },
  { id: 'indigo-mor', bg: '#5B4BE0', name: 'İndigo / mor', note: 'Güçlenme, özgünlük. Sağlık uygulamalarında daha az kullanılıyor. Mavinin yakın akrabası.' },
  { id: 'turkuaz', bg: '#0E9488', name: 'Derin turkuaz', note: 'Denge, huzur; Türkiye\'de olumlu kültürel çağrışım (firuze). Kontrast sınırda.' },
  { id: 'murekkep', bg: '#16181D', name: 'Mürekkep (uygulamanın koyu rengi)', note: 'Premium ve net. Koyu ikonlar ana ekranda öne çıkar ama soğuk durabilir.' },
  { id: 'gece-gradyan', bg: null, svgBg: grad('g1', '#2D7FF9', '#1E2B6B'), stops: ['#2D7FF9', '#1E2B6B'], name: 'Mavi → lacivert geçiş', note: 'Uygulamanın mavisini korur, derinlik katar. Modern mağaza ikonlarında yaygın.' },
  { id: 'mor-gradyan', bg: null, svgBg: grad('g2', '#5B4BE0', '#2D7FF9'), stops: ['#5B4BE0', '#2D7FF9'], name: 'Mor → mavi geçiş', note: 'Enerjik ama sakin. Mavi kalabalığından ayrılır.' },
];

// WCAG göreli parlaklık ve kontrast
const lum = (hex) => {
  const c = [1, 3, 5].map((i) => parseInt(hex.slice(i, i + 2), 16) / 255)
    .map((v) => (v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4));
  return 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2];
};
const contrast = (a, b) => { const [x, y] = [lum(a), lum(b)].sort((p, q) => q - p); return (x + 0.05) / (y + 0.05); };

const outDir = join(here, 'renk'), tmp = join(here, '.tmp');
mkdirSync(outDir, { recursive: true }); mkdirSync(tmp, { recursive: true });

for (const c of candidates) {
  const svg = base.replace(BG, c.svgBg ?? `<rect width="1024" height="1024" fill="${c.bg}"/>`);
  writeFileSync(join(outDir, `${c.id}.svg`), svg);
  const html = join(tmp, `${c.id}.html`);
  writeFileSync(html, `<!doctype html><html><head><style>html,body{margin:0;overflow:hidden}svg{display:block}</style></head><body>${svg}</body></html>`);
  execFileSync(CHROME, ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--window-size=1024,1024', `--screenshot=${join(outDir, `${c.id}.png`)}`, pathToFileURL(html).href], { stdio: 'ignore' });
  const colors = c.stops ?? [c.bg];
  c.contrast = Math.min(...colors.map((x) => contrast('#FFFFFF', x))).toFixed(1);
  console.log(`✔ ${c.id}  beyaz balon kontrastı ${c.contrast}:1`);
}
rmSync(tmp, { recursive: true, force: true });

writeFileSync(join(here, 'renk.html'), `<!doctype html><html lang="tr"><head><meta charset="utf-8"><title>Zemin rengi çalışması</title>
<style>
  @font-face { font-family: Satoshi; src: url('../../../Mobile/assets/fonts/Satoshi-Bold.ttf'); }
  body { margin: 0; padding: 26px; font-family: Satoshi, system-ui, sans-serif; background: #F8F7F4; color: #16181D; }
  h1 { margin: 0 0 18px; font-size: 26px; }
  .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(290px, 1fr)); gap: 18px; }
  .card { background: #fff; border-radius: 18px; padding: 16px; border: 1px solid #ECEDF0; }
  .sizes { display: flex; align-items: flex-end; gap: 12px; }
  .ios { border-radius: 22.37%; box-shadow: 0 3px 10px rgba(0,0,0,.14); }
  .walls { display: flex; gap: 8px; margin-top: 12px; }
  .wall { flex: 1; padding: 12px; border-radius: 14px; display: flex; justify-content: center; gap: 10px; }
  .light { background: linear-gradient(135deg, #f3d9c6, #c9d8f2); } .dark { background: linear-gradient(135deg, #1c2333, #3a2440); }
  .white { background: #fff; border: 1px solid #ECEDF0; }
  h2 { font-size: 16px; margin: 12px 0 2px; } p { margin: 0; font-size: 13px; color: #5A6472; line-height: 1.4; }
  .ratio { font-size: 12px; color: #98A1AE; margin-top: 4px; }
</style></head><body>
  <h1>Zemin rengi adayları · beyaz balon + mercan el</h1>
  <div class="grid">
  ${candidates.map((c) => `<div class="card">
    <div class="sizes">
      <img class="ios" src="renk/${c.id}.png" width="120"><img class="ios" src="renk/${c.id}.png" width="60"><img class="ios" src="renk/${c.id}.png" width="29">
    </div>
    <div class="walls">
      <div class="wall light"><img class="ios" src="renk/${c.id}.png" width="44"></div>
      <div class="wall dark"><img class="ios" src="renk/${c.id}.png" width="44"></div>
      <div class="wall white"><img class="ios" src="renk/${c.id}.png" width="44"></div>
    </div>
    <h2>${c.name}</h2><p>${c.note}</p>
    <div class="ratio">Beyaz balon / zemin kontrastı: ${c.contrast}:1</div>
  </div>`).join('')}
  </div>
</body></html>`);
console.log('✔ renk.html');
