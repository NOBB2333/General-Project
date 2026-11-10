import { createReadStream, existsSync, statSync } from 'node:fs';
import { createServer, request as httpRequest } from 'node:http';
import { extname, join, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const scriptDir = fileURLToPath(new URL('.', import.meta.url));
const frontendDir = resolve(process.env.FRONTEND_DIR || join(scriptDir, '..', 'web-general'));
const apiTarget = process.env.API_TARGET || 'http://127.0.0.1:5010';
const listenHost = process.env.HOST || '0.0.0.0';
const listenPort = Number(process.env.PORT || 5011);
const apiUrl = new URL(apiTarget);

const mimeTypes = {
  '.css': 'text/css; charset=utf-8',
  '.gif': 'image/gif',
  '.html': 'text/html; charset=utf-8',
  '.ico': 'image/x-icon',
  '.jpeg': 'image/jpeg',
  '.jpg': 'image/jpeg',
  '.js': 'text/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.map': 'application/json; charset=utf-8',
  '.png': 'image/png',
  '.svg': 'image/svg+xml',
  '.ttf': 'font/ttf',
  '.txt': 'text/plain; charset=utf-8',
  '.webp': 'image/webp',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
};

function isApiRequest(pathname) {
  return (
    pathname === '/api'
    || pathname.startsWith('/api/')
    || pathname === '/health'
    || pathname.startsWith('/health/')
  );
}

function proxyToApi(req, res) {
  const headers = { ...req.headers, host: apiUrl.host };
  const proxyReq = httpRequest(
    {
      hostname: apiUrl.hostname,
      port: apiUrl.port || 80,
      path: req.url,
      method: req.method,
      headers,
    },
    (proxyRes) => {
      res.writeHead(proxyRes.statusCode || 502, proxyRes.headers);
      proxyRes.pipe(res);
    },
  );
  proxyReq.on('error', (error) => {
    console.error('API proxy error:', error.message);
    if (!res.headersSent) {
      res.writeHead(502, { 'content-type': 'text/plain; charset=utf-8' });
    }
    res.end('Bad Gateway');
  });
  req.pipe(proxyReq);
}

function fileForPath(pathname) {
  const decoded = decodeURIComponent(pathname);
  const relative = decoded.replace(/^\/+/, '');
  const candidate = resolve(frontendDir, relative);
  if (candidate !== frontendDir && !candidate.startsWith(frontendDir + sep)) {
    return null;
  }
  if (existsSync(candidate) && statSync(candidate).isDirectory()) {
    return join(candidate, 'index.html');
  }
  return candidate;
}

function sendFile(req, res, filePath) {
  if (!filePath || !existsSync(filePath) || !statSync(filePath).isFile()) {
    return false;
  }
  const extension = extname(filePath).toLowerCase();
  const isIndex = filePath.endsWith(`${sep}index.html`);
  res.writeHead(200, {
    'content-type': mimeTypes[extension] || 'application/octet-stream',
    'cache-control': isIndex
      ? 'no-cache'
      : extension === '.html'
        ? 'no-cache'
        : 'public, max-age=31536000, immutable',
  });
  if (req.method === 'HEAD') {
    res.end();
    return true;
  }
  createReadStream(filePath).pipe(res);
  return true;
}

function serveFrontend(req, res, pathname) {
  const direct = fileForPath(pathname);
  if (direct && sendFile(req, res, direct)) {
    return;
  }

  // SPA fallback: only for routes without a file extension, so missing assets still 404.
  if (!extname(pathname)) {
    const index = join(frontendDir, 'index.html');
    if (sendFile(req, res, index)) {
      return;
    }
  }

  res.writeHead(404, { 'content-type': 'text/plain; charset=utf-8' });
  res.end('Not Found');
}

const server = createServer((req, res) => {
  let pathname;
  try {
    pathname = new URL(req.url || '/', `http://${req.headers.host || 'localhost'}`).pathname;
  } catch {
    res.writeHead(400, { 'content-type': 'text/plain; charset=utf-8' });
    res.end('Bad Request');
    return;
  }

  if (isApiRequest(pathname)) {
    proxyToApi(req, res);
    return;
  }
  serveFrontend(req, res, pathname);
});

server.listen(listenPort, listenHost, () => {
  console.log(
    `General frontend listening on http://${listenHost}:${listenPort} (dir=${frontendDir}, api=${apiTarget})`,
  );
});
