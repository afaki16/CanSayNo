// "Konuşma balonu + dur eli" logosu — kullanıcının yapay zekâ görselindeki fikrin (../alternatif/)
// geometrik, temiz yeniden çizimi. Dik duran el, eşit parmaklar, hareket çizgisi yok ("merhaba"
// değil "dur" okunsun); küçük boyutta siluet okunur kalsın diye parmak araları geniş.
//
//   node generate.mjs → svg/*.svg + png/*.png + preview.html (karşılaştırma sayfası)

import { execFileSync } from 'node:child_process';
import { mkdirSync, rmSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const CHROME = process.env.CHROME ?? 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const BLUE = '#2D7FF9';
const CORAL = '#FF7043';
const WHITE = '#FFFFFF';
const N = 1024;

// ---- Şekiller (1024'lük tuvalde, merkez 512) ----------------------------------------------

// El: yerel koordinatlar, avuç merkezi (0,0). Parmaklar yuvarlak uçlu çizgi (kapsül).
function hand(color) {
  const fw = 62;                                   // parmak kalınlığı; aralık 16 → küçük boyutta da ayrık
  const fingers = [                                // [x, uç y] — orta parmak en uzun
    [-117, -262], [-39, -310], [39, -294], [117, -236],
  ];
  const parts = fingers.map(([x, top]) =>
    `<line x1="${x}" y1="${top + fw / 2}" x2="${x}" y2="0" stroke="${color}" stroke-width="${fw}" stroke-linecap="round"/>`);
  // Avuç: üst köşeler düz (parmak dipleri çentiksiz birleşsin), alt köşeler çok yuvarlak.
  // Dış parmak kenarları (±148) avuç kenarlarıyla hizalı.
  parts.push(`<path d="M-148 -60 H148 V40 A130 130 0 0 1 18 170 H-18 A130 130 0 0 1 -148 40 Z" fill="${color}"/>`);
  // Başparmak: kısa ve kalın, avucun sol yanından yukarı-dışa
  parts.push(`<line x1="-104" y1="78" x2="-200" y2="-16" stroke="${color}" stroke-width="82" stroke-linecap="round"/>`);
  return parts.join('');
}

// Balon gövdesi + sol alt kuyruk. Kuyruğun köşeleri aynı renkte kalın çizgiyle yuvarlatılır.
function bubble(color) {
  const body = `<rect x="182" y="176" width="660" height="590" rx="270" fill="${color}"/>`;
  const tail = `<path d="M352 640 L262 858 L500 740 Z" fill="${color}" stroke="${color}" stroke-width="44" stroke-linejoin="round"/>`;
  return body + tail;
}

// Logo: balon + el (balonun optik merkezine, kuyruk payıyla hafif yukarı)
function mark({ bubbleColor, handColor }, scale = 1) {
  // El kutusu x −241…148, y −310…170. Görsel ağırlık parmaklarda olduğu için yatayda
  // geometrik merkez yerine hafif sola (−30) hizalanır; balon gövdesinin merkezi (512, 471).
  const handT = `translate(512 471) scale(0.8) translate(30 70)`;
  return `<g transform="translate(512 512) scale(${scale}) translate(-512 -512)">${bubble(bubbleColor)}<g transform="${handT}">${hand(handColor)}</g></g>`;
}

const svg = (body, bg) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${N}" height="${N}" viewBox="0 0 ${N} ${N}">${bg ? `<rect width="${N}" height="${N}" fill="${bg}"/>` : ''}${body}</svg>\n`;

// ---- Varyasyonlar ------------------------------------------------------------------------
// İkon ölçeği: iOS'ta kenardan kenara; balon tuvalin ~%64'ü
const ICON_SCALE = 0.86;
const variants = {
  'a-mavi-zemin': { bg: BLUE, bubbleColor: WHITE, handColor: BLUE, label: 'A · Mavi zemin, beyaz balon, mavi el' },
  'b-beyaz-zemin': { bg: WHITE, bubbleColor: BLUE, handColor: WHITE, label: 'B · Beyaz zemin, mavi balon, beyaz el' },
  'c-mercan-el': { bg: BLUE, bubbleColor: WHITE, handColor: CORAL, label: 'C · Mavi zemin, beyaz balon, mercan el' },
};

const svgDir = join(here, 'svg'), pngDir = join(here, 'png'), tmp = join(here, '.tmp');
for (const d of [svgDir, pngDir, tmp]) mkdirSync(d, { recursive: true });

function renderPng(name, content, size = N) {
  const html = join(tmp, `${name}.html`);
  writeFileSync(html, `<!doctype html><html><head><style>html,body{margin:0;background:transparent;overflow:hidden}svg{display:block;width:${size}px;height:${size}px}</style></head><body>${content}</body></html>`);
  execFileSync(CHROME, ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--default-background-color=00000000', `--window-size=${size},${size}`,
    `--screenshot=${join(pngDir, `${name}.png`)}`, pathToFileURL(html).href], { stdio: 'ignore' });
}

for (const [name, v] of Object.entries(variants)) {
  const content = svg(mark(v, ICON_SCALE), v.bg);
  writeFileSync(join(svgDir, `${name}.svg`), content);
  renderPng(name, content);
  console.log(`✔ ${name}`);
}
// Tek başına logo (şeffaf zemin) — site, açılış ekranı
const logo = svg(mark({ bubbleColor: BLUE, handColor: WHITE }, 1));
writeFileSync(join(svgDir, 'logo.svg'), logo);
renderPng('logo', logo);
rmSync(tmp, { recursive: true, force: true });

// ---- Karşılaştırma sayfası -----------------------------------------------------------------
const sizes = [180, 120, 60, 40, 29];
const iconRow = (src, label) => `
  <h2>${label}</h2>
  <div class="row">
    ${sizes.map((s) => `<div class="cell"><img class="ios" src="${src}" width="${s}"><span>${s}</span></div>`).join('')}
    <div class="cell"><div class="android" style="width:120px;height:120px"><img src="${src}"></div><span>Android daire</span></div>
    <div class="wall light"><img class="ios" src="${src}" width="60"></div>
    <div class="wall dark"><img class="ios" src="${src}" width="60"></div>
  </div>`;

writeFileSync(join(here, 'preview.html'), `<!doctype html><html lang="tr"><head><meta charset="utf-8"><title>Balon + el ikonları</title>
<style>
  @font-face { font-family: Satoshi; src: url('../../../Mobile/assets/fonts/Satoshi-Bold.ttf'); }
  body { margin: 0; padding: 28px; font-family: Satoshi, system-ui, sans-serif; background: #F8F7F4; color: #16181D; }
  h1 { margin: 0 0 4px; } h2 { font-size: 16px; margin: 22px 0 10px; }
  .row { display: flex; gap: 22px; align-items: flex-end; flex-wrap: wrap; }
  .cell { display: flex; flex-direction: column; align-items: center; gap: 6px; font-size: 12px; color: #5A6472; }
  .ios { border-radius: 22.37%; box-shadow: 0 3px 10px rgba(0,0,0,.14); }
  .android { border-radius: 50%; overflow: hidden; position: relative; box-shadow: 0 3px 10px rgba(0,0,0,.14); }
  .android img { position: absolute; width: 150%; height: 150%; left: -25%; top: -25%; }
  .wall { padding: 18px; border-radius: 18px; }
  .wall.light { background: linear-gradient(135deg, #f3d9c6, #c9d8f2); }
  .wall.dark { background: linear-gradient(135deg, #1c2333, #3a2440); }
</style></head><body>
  <h1>Balon + "dur" eli</h1>
  <div style="color:#5A6472">Android önizlemesi kabaca: tam ikon ortadan kırpılmıştır; gerçek adaptive katmanlar seçimden sonra üretilir.</div>
  ${Object.entries(variants).map(([name, v]) => iconRow(`png/${name}.png`, v.label)).join('')}
  ${iconRow('../alternatif/balon-el-ai.jpeg', 'Karşılaştırma · Yapay zekâ görseli (ham)')}
  ${iconRow('../png/icon.png', 'Karşılaştırma · Önceki ok ikonu')}
</body></html>`);
console.log('✔ preview.html');
