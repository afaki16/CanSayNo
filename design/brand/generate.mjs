// Marka dosyalarını üretir ve projelere kopyalar.
//
//   node design/brand/generate.mjs
//
// Çıktı: design/brand/out/ (svg + png). Ayrıca:
//   Mobile/assets/images/   icon, android-icon-*, splash-icon, favicon
//   Web/src/                favicon.svg, assets/apple-touch-icon.png, assets/og-image.png, assets/logo.svg
//   Frontend/public/        favicon.svg, favicon.png, images/logo.svg
//   docs/store/gorseller/   Google öne çıkan görsel (1024×500)
// PNG'ler headless Chrome ile; şeffaflık gereken dosyalar şeffaf, iOS ikonu alfasız (Apple şartı).

import { execFileSync } from 'node:child_process';
import { copyFileSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { COLORS, ICON_MARK_SCALE, ROUNDED_RADIUS, gradientDef, mark, silhouette } from './brand.mjs';

const here = fileURLToPath(new URL('.', import.meta.url));
const root = join(here, '..', '..');
const CHROME = process.env.CHROME ?? 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const out = join(here, 'out'), tmp = join(here, '.tmp');
for (const d of [out, tmp]) mkdirSync(d, { recursive: true });

const N = 1024;
const ANDROID_VISIBLE = 72 / 108;  // adaptive ikon: 108dp tuvalin ortadaki 72dp'si görünür

const svg = (w, h, body, viewBox = `0 0 ${w} ${h}`) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${viewBox}">${body}</svg>\n`;
const gradRect = (r = 0) =>
  `<defs>${gradientDef()}</defs><rect width="1024" height="1024" rx="${r}" fill="url(#brand)"/>`;

// Yuvarlak köşeli logo (uygulama içi, açılış ekranı, favicon, web)
const roundedMark = gradRect(N * ROUNDED_RADIUS) + mark({ scale: ICON_MARK_SCALE });

const assets = [
  // [ad, genişlik, yükseklik, svg gövdesi (1024 koordinatları), şeffaf mı, viewBox]
  ['icon', N, N, gradRect() + mark({ scale: ICON_MARK_SCALE }), false],
  ['android-icon-background', N, N, gradRect(), false],
  ['android-icon-foreground', N, N, mark({ scale: ICON_MARK_SCALE * ANDROID_VISIBLE }), true],
  ['android-icon-monochrome', N, N, silhouette({ scale: ICON_MARK_SCALE * ANDROID_VISIBLE }), true],
  ['splash-icon', N, N, roundedMark, true],
  ['logo', N, N, roundedMark, true],
  ['favicon-48', 48, 48, roundedMark, true, '0 0 1024 1024'],
  ['apple-touch-icon', 180, 180, gradRect() + mark({ scale: ICON_MARK_SCALE }), false, '0 0 1024 1024'],
];

function render(name, w, h, content, transparent) {
  const html = join(tmp, `${name}.html`);
  writeFileSync(html, `<!doctype html><html><head><style>@font-face{font-family:Satoshi;src:url('${pathToFileURL(join(root, 'Mobile/assets/fonts/Satoshi-Black.ttf')).href}');font-weight:900}@font-face{font-family:Satoshi;src:url('${pathToFileURL(join(root, 'Mobile/assets/fonts/Satoshi-Medium.ttf')).href}');font-weight:500}html,body{margin:0;overflow:hidden;background:${transparent ? 'transparent' : '#fff'}}svg{display:block}</style></head><body>${content}</body></html>`);
  const args = ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--allow-file-access-from-files', '--virtual-time-budget=2000',
    `--window-size=${w},${h}`, `--screenshot=${join(out, `${name}.png`)}`, pathToFileURL(html).href];
  if (transparent) args.splice(4, 0, '--default-background-color=00000000');
  execFileSync(CHROME, args, { stdio: 'ignore' });
}

for (const [name, w, h, body, transparent, viewBox] of assets) {
  const content = svg(w, h, body, viewBox ?? `0 0 ${w} ${h}`);
  writeFileSync(join(out, `${name}.svg`), content);
  render(name, w, h, content, transparent);
  console.log(`✔ ${name}.png ${w}×${h}${transparent ? ' (şeffaf)' : ''}`);
}

// Paylaşım görseli (web, 1200×630) ve Google Play öne çıkan görsel (1024×500): geçiş zemin,
// solda logo, sağda marka adı + slogan.
function banner(name, w, h, logoSize, title, subtitle) {
  const body = `<div style="width:${w}px;height:${h}px;display:flex;align-items:center;gap:${logoSize * 0.28}px;padding:0 ${w * 0.08}px;box-sizing:border-box;
      background:linear-gradient(135deg, ${COLORS.indigo}, ${COLORS.blue});font-family:Satoshi,sans-serif;color:#fff">
    <div style="width:${logoSize}px;height:${logoSize}px;flex:none;border-radius:${logoSize * ROUNDED_RADIUS}px;background:rgba(255,255,255,.14);box-shadow:0 20px 50px rgba(20,16,80,.25)">
      ${svg(logoSize, logoSize, mark({ scale: ICON_MARK_SCALE }), '0 0 1024 1024')}
    </div>
    <div><div style="font-weight:900;font-size:${h * 0.15}px;letter-spacing:-0.03em;line-height:1">${title}</div>
    <div style="font-weight:500;font-size:${h * 0.065}px;opacity:.92;margin-top:${h * 0.04}px;line-height:1.25">${subtitle}</div></div>
  </div>`;
  render(name, w, h, body, false);
  console.log(`✔ ${name}.png ${w}×${h}`);
}
banner('og-image', 1200, 630, 260, 'CanSayNo', 'Hayır demeyi yapay zekâ<br>karakterlerle pratik et');
banner('feature-graphic', 1024, 500, 210, 'CanSayNo', 'Hayır demeyi yapay zekâ<br>karakterlerle pratik et');
rmSync(tmp, { recursive: true, force: true });

// ---- Projelere kopyala ---------------------------------------------------------------------
const copy = (from, to) => { mkdirSync(dirname(to), { recursive: true }); copyFileSync(join(out, from), to); };
const mobile = join(root, 'Mobile', 'assets', 'images');
for (const f of ['icon', 'android-icon-background', 'android-icon-foreground', 'android-icon-monochrome', 'splash-icon'])
  copy(`${f}.png`, join(mobile, `${f}.png`));
copy('favicon-48.png', join(mobile, 'favicon.png'));

const web = join(root, 'Web', 'src');
copy('logo.svg', join(web, 'favicon.svg'));
copy('logo.svg', join(web, 'assets', 'logo.svg'));
copy('apple-touch-icon.png', join(web, 'assets', 'apple-touch-icon.png'));
copy('og-image.png', join(web, 'assets', 'og-image.png'));

const front = join(root, 'Frontend', 'public');
copy('logo.svg', join(front, 'favicon.svg'));
copy('logo.svg', join(front, 'images', 'logo.svg'));   // data.json → app.logo.src (kenar çubuğu)
copy('favicon-48.png', join(front, 'favicon.png'));

copy('feature-graphic.png', join(root, 'docs', 'store', 'gorseller', 'google-feature-graphic.png'));
copy('icon.png', join(root, 'docs', 'store', 'gorseller', 'app-icon-1024.png'));
console.log('\n✔ Mobile/assets/images, Web/src, Frontend/public, docs/store/gorseller güncellendi');
