// CanSayNo statik sitesi: src/ şablonları + docs/store/*.md → dist/
//
//   npm run build          → dist/ üretir, doldurulmamış yer tutucuları ([AD SOYAD] gibi) uyarır
//   npm run build:strict   → yer tutucu kalmışsa HATA verir (yayından önce bunu kullan)
//
// Yasal metinlerin TEK kaynağı docs/store/: metni orada düzelt, siteyi yeniden üret.
// Temiz URL'ler için her sayfa kendi klasörüne index.html olarak yazılır (/gizlilik/ gibi).

import { cpSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { marked } from 'marked';

const root = dirname(fileURLToPath(import.meta.url));
const docs = join(root, '..', 'docs', 'store');
const src = join(root, 'src');
const dist = join(root, 'dist');
const strict = process.argv.includes('--strict');
const siteUrl = 'https://cansayno.com';
const urls = [];

const layout = readFileSync(join(src, 'layout.html'), 'utf8');

const legalPages = [
  { path: 'gizlilik', altPath: 'en/privacy', doc: 'gizlilik-politikasi.md', lang: 'tr', title: 'Gizlilik Politikası',
    description: 'CanSayNo gizlilik politikası ve KVKK aydınlatma metni.' },
  { path: 'kullanim-sartlari', altPath: 'en/terms', doc: 'kullanim-sartlari.md', lang: 'tr', title: 'Kullanım Şartları',
    description: 'CanSayNo kullanım şartları.' },
  { path: 'hesap-silme', altPath: '/hesap-silme/#english', doc: 'hesap-silme.md', lang: 'tr', title: 'Hesap Silme',
    description: 'CanSayNo hesabını ve verilerini nasıl silersin. How to delete your CanSayNo account and data.' },
  { path: 'en/privacy', altPath: 'gizlilik', doc: 'privacy-policy.md', lang: 'en', title: 'Privacy Policy',
    description: 'CanSayNo privacy policy.' },
  { path: 'en/terms', altPath: 'kullanim-sartlari', doc: 'terms.md', lang: 'en', title: 'Terms of Use',
    description: 'CanSayNo terms of use.' },
];

// Başlıklara bağlantı verilebilsin diye id: "## English" → id="english" (/hesap-silme/#english).
const slug = (text) => text.toLocaleLowerCase('tr')
  .replace(/[çğıöşü]/g, (c) => ({ ç: 'c', ğ: 'g', ı: 'i', ö: 'o', ş: 's', ü: 'u' })[c])
  .replace(/<[^>]+>/g, '').replace(/&[a-z0-9#]+;/g, '').replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
marked.use({
  renderer: {
    heading({ tokens, depth }) {
      const html = this.parser.parseInline(tokens);
      return `<h${depth} id="${slug(html)}">${html}</h${depth}>\n`;
    },
  },
});

// [AD SOYAD], [ALAN ADI], [90] gibi doldurulmamış alanlar. Markdown linki ve onay kutusu değil.
const placeholder = /\[(?:[A-ZÇĞİÖŞÜ][A-ZÇĞİÖŞÜ0-9 /:.,'-]*|\d+)\](?!\()/g;
const missing = [];

// Şablondaki menü / alt bilgi metinleri (layout.html {{ui.*}}). altLang: geçiş linkinin dili.
const UI = {
  tr: {
    home: '/', homeLabel: 'CanSayNo ana sayfa', navLabel: 'Ana menü',
    navHow: 'Nasıl çalışır', navContexts: 'Bağlamlar', navPrice: 'Fiyat',
    howId: 'nasil-calisir', contextsId: 'baglamlar', priceId: 'fiyat',
    tagline: 'Hayır demek bir beceridir.', legal: 'Yasal', languageLabel: 'Dil',
    privacy: 'Gizlilik Politikası', privacyHref: '/gizlilik/',
    terms: 'Kullanım Şartları', termsHref: '/kullanim-sartlari/',
    deletion: 'Hesap Silme', deletionHref: '/hesap-silme/',
    disclaimer: 'CanSayNo bir terapi ya da psikolojik danışmanlık hizmeti değildir.',
    altLang: 'en', altLabel: 'English',
  },
  en: {
    home: '/en/', homeLabel: 'CanSayNo home', navLabel: 'Main menu',
    navHow: 'How it works', navContexts: 'Areas', navPrice: 'Pricing',
    howId: 'how-it-works', contextsId: 'contexts', priceId: 'pricing',
    tagline: 'Saying no is a skill.', legal: 'Legal', languageLabel: 'Language',
    privacy: 'Privacy Policy', privacyHref: '/en/privacy/',
    terms: 'Terms of Use', termsHref: '/en/terms/',
    deletion: 'Account Deletion', deletionHref: '/hesap-silme/#english',
    disclaimer: 'CanSayNo is not a therapy or psychological counselling service.',
    altLang: 'tr', altLabel: 'Türkçe',
  },
};

// altPath: aynı sayfanın diğer dildeki adresi (dil geçiş linki + hreflang)
function render({ title, description, lang, content, bodyClass = '', path = '', altPath = '' }) {
  const ui = UI[lang];
  const altHref = altPath.startsWith('/') ? altPath : `/${altPath ? `${altPath}/` : ''}`;
  let html = layout;
  for (const [key, value] of Object.entries(ui)) html = html.replaceAll(`{{ui.${key}}}`, value);
  return html
    .replaceAll('{{url}}', `${siteUrl}/${path ? `${path}/` : ''}`)
    .replaceAll('{{altUrl}}', `${siteUrl}${altHref.split('#')[0]}`)
    .replaceAll('{{altPath}}', altHref)
    .replaceAll('{{lang}}', lang)
    .replaceAll('{{title}}', escapeHtml(title))
    .replaceAll('{{description}}', escapeHtml(description))
    .replaceAll('{{bodyClass}}', bodyClass)
    .replace('{{content}}', content);
}

function escapeHtml(text) {
  return text.replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);
}

function write(path, html) {
  urls.push(`${siteUrl}/${path ? `${path}/` : ''}`);
  const file = join(dist, path, 'index.html');
  mkdirSync(dirname(file), { recursive: true });
  writeFileSync(file, html);
}

rmSync(dist, { recursive: true, force: true });
mkdirSync(dist, { recursive: true });
cpSync(join(src, 'assets'), join(dist, 'assets'), { recursive: true });
cpSync(join(root, '..', 'Mobile', 'assets', 'fonts'), join(dist, 'assets', 'fonts'), { recursive: true });
// _headers / _redirects: Cloudflare yapılandırması (güvenlik başlıkları, kısa adresler)
for (const file of ['robots.txt', 'favicon.svg', '_headers', '_redirects']) cpSync(join(src, file), join(dist, file));

// Ana sayfalar (Türkçe /, İngilizce /en/)
write('', render({
  path: '',
  altPath: 'en',
  title: 'CanSayNo: Hayır Demeyi Öğren',
  description: 'Zor insanlara "hayır" demeyi yapay zekâ karakterlerle güvenle pratik et. Patron, aile, arkadaş, satıcı: 8 bağlamda 65+ gerçekçi senaryo.',
  lang: 'tr',
  bodyClass: 'home',
  content: readFileSync(join(src, 'home.html'), 'utf8'),
}));
write('en', render({
  path: 'en',
  altPath: '',
  title: 'CanSayNo: Learn to Say No',
  description: 'Practise saying "no" to pushy people with realistic AI characters. Managers, family, friends, salespeople: 45 scenarios across 8 areas.',
  lang: 'en',
  bodyClass: 'home',
  content: readFileSync(join(src, 'home.en.html'), 'utf8'),
}));

// Yasal sayfalar
for (const page of legalPages) {
  const markdown = readFileSync(join(docs, page.doc), 'utf8');
  for (const match of markdown.matchAll(placeholder)) missing.push(`${page.doc}: ${match[0]}`);
  const body = marked.parse(markdown);
  write(page.path, render({
    ...page,
    path: page.path,
    title: `${page.title} · CanSayNo`,
    bodyClass: 'legal',
    content: `<main class="legal-body container">${body}</main>`,
  }));
}

// 404 (barındırma servisleri kökteki 404.html'i kullanır)
writeFileSync(join(dist, '404.html'), render({
  altPath: 'en',
  title: 'Sayfa bulunamadı · CanSayNo',
  description: 'Aradığın sayfa bulunamadı.',
  lang: 'tr',
  bodyClass: 'legal',
  content: '<main class="legal-body container"><h1>Sayfa bulunamadı</h1><p>Bu sayfaya "hayır" dedik. <a href="/">Ana sayfaya dön</a>.</p></main>',
}));

// Arama motorları için site haritası (robots.txt'te belirtilir).
const sitemapEntries = urls.map((u) => `  <url><loc>${u}</loc></url>`).join('\n');
writeFileSync(join(dist, 'sitemap.xml'),
  `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n${sitemapEntries}\n</urlset>\n`);

const unique = [...new Set(missing)];
if (unique.length > 0) {
  console.warn(`\n⚠ Doldurulmamış ${unique.length} yer tutucu (docs/store/ içinde düzelt):`);
  for (const m of unique) console.warn(`  - ${m}`);
  if (strict) {
    console.error('\n✖ --strict: yer tutucular doldurulmadan yayınlanamaz.');
    process.exit(1);
  }
}
console.log(`\n✔ dist/ hazır: ana sayfa + ${legalPages.length} yasal sayfa + 404`);
