import { build, context } from 'esbuild';
import { cpSync, mkdirSync, readFileSync, writeFileSync, existsSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));

const args = process.argv.slice(2);
const minify = args.includes('--minify');
const watch = args.includes('--watch');

// Locate the Birko\Web source bucket WITHOUT a machine-specific path: prefer
// the BIRKO_WEB_SRC env var (used by CI/Docker), otherwise walk up from this file to
// find the Birko/Web checkout. Safe to commit — no absolute dev-box path is
// baked in; works at any depth and on any machine that keeps the bucket layout.
function resolveBirkoSrc() {
  const env = process.env.BIRKO_WEB_SRC;
  if (env) return env.replace(/[\\/]+$/, '').replaceAll('\\', '/');
  for (let d = __dirname; d !== dirname(d); d = dirname(d)) {
    const candidate = resolve(d, 'Birko/Web');
    if (existsSync(resolve(candidate, 'Birko.Web.Core'))) return candidate.replaceAll('\\', '/');
  }
  throw new Error('Cannot locate Birko\\Web. Set the BIRKO_WEB_SRC env var to its path.');
}
const BIRKO_SRC = resolveBirkoSrc();

const aliases = {
  // Birko.Web.Core
  'birko-web-core':          `${BIRKO_SRC}/Birko.Web.Core/src/index.ts`,
  'birko-web-core/base':     `${BIRKO_SRC}/Birko.Web.Core/src/base/index.ts`,
  'birko-web-core/state':    `${BIRKO_SRC}/Birko.Web.Core/src/state/index.ts`,
  'birko-web-core/http':     `${BIRKO_SRC}/Birko.Web.Core/src/http/index.ts`,
  'birko-web-core/router':   `${BIRKO_SRC}/Birko.Web.Core/src/router/index.ts`,
  'birko-web-core/i18n':     `${BIRKO_SRC}/Birko.Web.Core/src/i18n/index.ts`,
  'birko-web-core/offline':  `${BIRKO_SRC}/Birko.Web.Core/src/offline/index.ts`,
  // Birko.Web.Components
  'birko-web-components':            `${BIRKO_SRC}/Birko.Web.Components/src/index.ts`,
  'birko-web-components/inputs':     `${BIRKO_SRC}/Birko.Web.Components/src/inputs/index.ts`,
  'birko-web-components/layout':     `${BIRKO_SRC}/Birko.Web.Components/src/layout/index.ts`,
  'birko-web-components/data':       `${BIRKO_SRC}/Birko.Web.Components/src/data/index.ts`,
  'birko-web-components/feedback':   `${BIRKO_SRC}/Birko.Web.Components/src/feedback/index.ts`,
  'birko-web-components/nav':        `${BIRKO_SRC}/Birko.Web.Components/src/nav/index.ts`,
  'birko-web-components/command':    `${BIRKO_SRC}/Birko.Web.Components/src/command/index.ts`,
  // dialogs is a lean subpath (b-confirm-dialog + b-modal + b-button only), not a src/<dir>/index.ts,
  // so the tsconfig `birko-web-components/*` wildcard covers it but this map needs it spelled out.
  'birko-web-components/dialogs':    `${BIRKO_SRC}/Birko.Web.Components/src/dialogs/index.ts`,
  'birko-web-components/form-utils': `${BIRKO_SRC}/Birko.Web.Components/src/form-utils/index.ts`,
  // Birko.Web.Shell
  'birko-web-shell':                `${BIRKO_SRC}/Birko.Web.Shell/src/index.ts`,
  'birko-web-shell/shell':          `${BIRKO_SRC}/Birko.Web.Shell/src/shell/index.ts`,
  'birko-web-shell/auth':           `${BIRKO_SRC}/Birko.Web.Shell/src/auth/index.ts`,
  'birko-web-shell/modules':        `${BIRKO_SRC}/Birko.Web.Shell/src/modules/index.ts`,
  'birko-web-shell/tenants':        `${BIRKO_SRC}/Birko.Web.Shell/src/tenants/index.ts`,
  'birko-web-shell/notifications':  `${BIRKO_SRC}/Birko.Web.Shell/src/notifications/index.ts`,
  'birko-web-shell/connection':     `${BIRKO_SRC}/Birko.Web.Shell/src/connection/index.ts`,
  'birko-web-shell/commands':       `${BIRKO_SRC}/Birko.Web.Shell/src/commands/index.ts`,
  'birko-web-shell/feedback':       `${BIRKO_SRC}/Birko.Web.Shell/src/feedback/index.ts`,
  'birko-web-shell/pages':          `${BIRKO_SRC}/Birko.Web.Shell/src/pages/index.ts`,
  'birko-web-shell/dashboard':      `${BIRKO_SRC}/Birko.Web.Shell/src/dashboard/index.ts`,
};

// Copy Birko base CSS (tokens + reset) into wwwroot/css/ so <link rel="stylesheet"> works.
mkdirSync('wwwroot/css', { recursive: true });
cpSync(`${BIRKO_SRC}/Birko.Web.Components/css/tokens.css`, 'wwwroot/css/tokens.css');
cpSync(`${BIRKO_SRC}/Birko.Web.Components/css/reset.css`,  'wwwroot/css/reset.css');
// Alternate themes are separate files; ship only the ones the shell registers
// (see getAvailableThemes in src/shell/app-shell.ts). Keep this list, the
// registerThemes call and the <link> tags in index.html in step.
const THEMES = ['finstat'];
mkdirSync('wwwroot/css/themes', { recursive: true });
for (const theme of THEMES) {
  cpSync(`${BIRKO_SRC}/Birko.Web.Components/css/themes/${theme}.css`, `wwwroot/css/themes/${theme}.css`);
}

const options = {
  entryPoints: ['src/app.ts'],
  bundle: true,
  outfile: 'wwwroot/app.js',
  format: 'esm',
  target: 'es2022',
  sourcemap: true,
  minify,
  alias: aliases,
  loader: { '.ts': 'ts', '.css': 'text' },
  logLevel: 'info',
};

/**
 * Stamp the assets referenced from `wwwroot/index.html` with a short content hash
 * (`?v=<hash>`). Browser caches bust whenever file content changes and stays cached
 * otherwise. Idempotent — a pre-existing `?v=…` query is replaced in place, so
 * repeated builds don't accumulate junk. Ignores files that don't exist.
 */
function stampAssetHashes() {
  const indexPath = 'wwwroot/index.html';
  const assets = ['app.js', 'css/tokens.css', ...THEMES.map(x => `css/themes/${x}.css`), 'css/reset.css', 'css/theme.css'];

  let html;
  try { html = readFileSync(indexPath, 'utf8'); } catch { return; }

  for (const asset of assets) {
    let content;
    try { content = readFileSync(`wwwroot/${asset}`); } catch { continue; }
    const hash = createHash('sha1').update(content).digest('hex').slice(0, 10);
    const escaped = asset.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const re = new RegExp(`(href|src)="${escaped}(\\?v=[^"]*)?"`, 'g');
    html = html.replace(re, `$1="${asset}?v=${hash}"`);
  }

  writeFileSync(indexPath, html);
}

if (watch) {
  const ctx = await context({
    ...options,
    plugins: [{
      name: 'stamp-hashes',
      setup(b) { b.onEnd(() => stampAssetHashes()); },
    }],
  });
  await ctx.watch();
  console.log('watching src/ for changes…');
} else {
  await build(options);
  stampAssetHashes();
  console.log(`✓ Built${minify ? ' (minified)' : ''}`);
}
