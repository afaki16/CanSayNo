// dist/'i yerelde gösterir (bağımlılıksız): node preview.mjs → http://localhost:4173
// Temiz URL'leri barındırma servisleri gibi çözer: /gizlilik → dist/gizlilik/index.html

import { createServer } from 'node:http';
import { existsSync, readFileSync, statSync } from 'node:fs';
import { extname, join, normalize } from 'node:path';
import { fileURLToPath } from 'node:url';

const dist = fileURLToPath(new URL('./dist/', import.meta.url));
const port = Number(process.env.PORT ?? 4173);
const types = {
  '.html': 'text/html; charset=utf-8', '.css': 'text/css; charset=utf-8', '.svg': 'image/svg+xml',
  '.ttf': 'font/ttf', '.png': 'image/png', '.txt': 'text/plain; charset=utf-8', '.js': 'text/javascript',
};

createServer((req, res) => {
  const url = decodeURIComponent(new URL(req.url, 'http://x').pathname);
  let file = normalize(join(dist, url));
  if (!file.startsWith(normalize(dist))) return res.writeHead(403).end();
  if (existsSync(file) && statSync(file).isDirectory()) file = join(file, 'index.html');
  const found = existsSync(file);
  if (!found) file = join(dist, '404.html');
  res.writeHead(found ? 200 : 404, { 'Content-Type': types[extname(file)] ?? 'application/octet-stream' });
  res.end(readFileSync(file));
}).listen(port, '127.0.0.1', () => console.log(`http://localhost:${port}`));
