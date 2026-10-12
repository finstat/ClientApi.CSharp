import { BaseComponent, define } from 'birko-web-core';
import type { BModal } from 'birko-web-components';

import { t, onLocaleChange } from '../i18n/index.js';

/** Dispatched on window when the user picks "About" in the user menu. */
export const OPEN_ABOUT_EVENT = 'finstat:open-about';

export const DOCS_URL             = 'https://finstat.github.io/api-documentation/';
export const CLIENT_REPO          = 'https://github.com/finstat/ClientApi.CSharp';
export const BIRKO_AUTHOR         = 'https://github.com/birko';
export const BIRKO_AUTHOR_AVATAR  = 'https://github.com/birko.png?size=48';
export const BIRKO_REPO           = 'https://github.com/Birko-Framework/Birko.Framework';
export const BIRKO_WEB_CORE       = 'https://github.com/Birko-Framework/Birko.Web/tree/main/Birko.Web.Core';
export const BIRKO_WEB_COMPONENTS = 'https://github.com/Birko-Framework/Birko.Web/tree/main/Birko.Web.Components';
export const BIRKO_WEB_SHELL      = 'https://github.com/Birko-Framework/Birko.Web/tree/main/Birko.Web.Shell';

/** Shape of GET api/about — versions read from the loaded assemblies, see AboutEndpoints.cs. */
interface AboutInfo { tester: string; clientSk: string; clientCz: string; runtime: string; }

/** GitHub mark (Octicons, MIT) inlined so the About modal works offline. */
const GH_ICON = `<svg class="gh-icon" viewBox="0 0 16 16" aria-hidden="true">
  <path d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0 0 16 8c0-4.42-3.58-8-8-8Z"/>
</svg>`;

export class AboutModal extends BaseComponent {
  private _windowListener?: () => void;

  static get styles() {
    return `
      :host { display: contents; }
      .stack { display: flex; flex-direction: column; gap: var(--b-space-md, 1rem); color: var(--b-text); }
      h3 { margin: 0 0 var(--b-space-xs, 0.25rem); font-size: var(--b-text-base, 0.875rem); color: var(--b-text-secondary); }
      ul { margin: 0; padding-left: 1.25rem; }
      li { margin: 0.15rem 0; }
      .brand { font-size: var(--b-text-xl, 1.125rem); font-weight: 600; }
      .muted { color: var(--b-text-muted); font-size: var(--b-text-sm, 0.8125rem); }
      a { color: var(--b-color-primary); text-decoration: none; display: inline-flex; align-items: center; gap: 0.35rem; }
      a:hover { text-decoration: underline; }
      /* b-modal focuses the first link on open; without this the browser draws its own black box. */
      a:focus-visible {
        outline: 2px solid var(--b-color-primary);
        outline-offset: 2px;
        border-radius: var(--b-radius-sm, 0.25rem);
      }
      dl { display: grid; grid-template-columns: auto 1fr; gap: 0.15rem var(--b-space-md, 1rem); margin: 0; }
      dt { color: var(--b-text-secondary); }
      dd { margin: 0; font-family: var(--b-font-mono, monospace); font-size: var(--b-text-sm, 0.8125rem); }
      .author { display: flex; align-items: center; gap: var(--b-space-sm, 0.5rem); }
      .author img { width: 2.5rem; height: 2.5rem; border-radius: var(--b-radius-full, 9999px); border: 1px solid var(--b-border); }
      .author .who { display: flex; flex-direction: column; }
      .gh-icon { width: 1em; height: 1em; vertical-align: -0.15em; fill: currentColor; }
    `;
  }

  render() {
    return `
      <b-modal id="modal" title="${t('about.title')}" size="sm">
        <div class="stack">
          <div>
            <div class="brand">${t('shell.brand')}</div>
            <div class="muted">${t('about.tagline')}</div>
          </div>

          ${this._info ? `
          <div>
            <h3>${t('about.versions')}</h3>
            <dl>
              <dt>${t('about.versionTester')}</dt>   <dd>${this._info.tester}</dd>
              <dt>${t('about.versionClientSk')}</dt> <dd>${this._info.clientSk}</dd>
              <dt>${t('about.versionClientCz')}</dt> <dd>${this._info.clientCz}</dd>
              <dt>${t('about.versionRuntime')}</dt>  <dd>${this._info.runtime}</dd>
            </dl>
          </div>` : ''}

          <div>
            <h3>${t('about.author')}</h3>
            <a class="author" href="${BIRKO_AUTHOR}" target="_blank" rel="noopener">
              <img src="${BIRKO_AUTHOR_AVATAR}" alt="" referrerpolicy="no-referrer">
              <span class="who">
                <strong>birko</strong>
                <span class="muted">${GH_ICON} github.com/birko</span>
              </span>
            </a>
          </div>

          <div>
            <h3>${t('about.poweredBy')}</h3>
            <ul>
              <li><a href="${BIRKO_REPO}"           target="_blank" rel="noopener">${GH_ICON} Birko.Framework</a></li>
              <li><a href="${BIRKO_WEB_CORE}"       target="_blank" rel="noopener">${GH_ICON} Birko.Web.Core</a></li>
              <li><a href="${BIRKO_WEB_COMPONENTS}" target="_blank" rel="noopener">${GH_ICON} Birko.Web.Components</a></li>
              <li><a href="${BIRKO_WEB_SHELL}"      target="_blank" rel="noopener">${GH_ICON} Birko.Web.Shell</a></li>
            </ul>
          </div>

          <div>
            <h3>${t('about.finstatApi')}</h3>
            <ul>
              <li><a href="${DOCS_URL}" target="_blank" rel="noopener">${t('about.documentation')}</a></li>
              <li><a href="${CLIENT_REPO}" target="_blank" rel="noopener">${GH_ICON} ${t('about.clientLibrary')}</a></li>
            </ul>
          </div>
        </div>
        <footer slot="footer">
          <b-button variant="primary" id="btn-close">${t('common.close')}</b-button>
        </footer>
      </b-modal>
    `;
  }

  private _unsubLocale?: () => void;
  private _info: AboutInfo | null = null;

  protected onMount() {
    this.$('#btn-close')?.addEventListener('click', () => this.close());

    this._windowListener = () => this.open();
    window.addEventListener(OPEN_ABOUT_EVENT, this._windowListener);
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    if (this._windowListener) window.removeEventListener(OPEN_ABOUT_EVENT, this._windowListener);
    this._unsubLocale?.();
  }

  async open() {
    // Versions are fetched once; a failed call just leaves the section out rather than blocking the modal.
    if (!this._info) {
      try {
        const resp = await fetch('api/about');
        if (resp.ok) { this._info = await resp.json() as AboutInfo; this.update(); }
      } catch { /* offline or older backend — show the modal without versions */ }
    }
    this.$<BModal>('#modal')?.open();
  }

  close() { this.$<BModal>('#modal')?.close(); }
}

define('about-modal', AboutModal);

// Mount a single instance into the body (outside the shell) so the overlay covers everything.
if (typeof document !== 'undefined' && !document.querySelector('about-modal')) {
  const mount = () => document.body.appendChild(document.createElement('about-modal'));
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount, { once: true });
  } else {
    mount();
  }
}
