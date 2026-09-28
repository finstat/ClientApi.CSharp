import { BaseComponent, define } from 'birko-web-core';

import { hasCredentials, settingsStore } from '../settings/settings-store.js';
import { OPEN_SETTINGS_EVENT } from './app-shell.js';
import { DOCS_URL } from './about-modal.js';
import { t, onLocaleChange } from '../i18n/index.js';

/**
 * Root ("/") landing page shown when the user first opens the tester. Gives a one-paragraph
 * orientation, surfaces whether credentials are set, and offers quick links into the most
 * common action pages. No FinStat calls are made here.
 */
export class LandingPage extends BaseComponent {
  private _unsubLocale?: () => void;
  private _unsubs: Array<() => void> = [];

  static get styles() {
    return `
      :host { display: block; padding: var(--b-space-xl, 2rem); color: var(--b-text); }
      h1 { margin: 0 0 var(--b-space-sm, 0.5rem); font-size: var(--b-text-2xl, 1.5rem); }
      .tagline { color: var(--b-text-secondary); margin: 0 0 var(--b-space-lg, 1.5rem); font-size: var(--b-text-base, 0.875rem); }

      b-card { margin-bottom: var(--b-space-md, 1rem); }
      h3 { margin: 0 0 var(--b-space-sm, 0.5rem); font-size: var(--b-text-base, 0.9375rem); color: var(--b-text-secondary); }

      .creds { display: flex; align-items: center; gap: var(--b-space-md, 0.75rem); flex-wrap: wrap; }
      .creds-state { font-weight: 600; }
      .creds-state.ok   { color: var(--b-color-success, #16a34a); }
      .creds-state.miss { color: var(--b-color-warning, #d97706); }

      .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(16rem, 1fr));
        gap: var(--b-space-md, 0.75rem);
      }
      a.tile {
        display: block; padding: var(--b-space-md, 0.75rem);
        border: 1px solid var(--b-border); border-radius: var(--b-radius, 0.375rem);
        text-decoration: none; color: var(--b-text); background: var(--b-bg-elevated);
        transition: border-color 150ms ease, background 150ms ease;
      }
      a.tile:hover { border-color: var(--b-color-primary); background: var(--b-bg-tertiary); }
      .tile .title { font-weight: 600; margin-bottom: 0.15rem; }
      .tile .desc  { color: var(--b-text-secondary); font-size: var(--b-text-sm, 0.8125rem); }
      .tile .icon  { margin-right: var(--b-space-xs, 0.25rem); }

      .footer-links { display: flex; gap: var(--b-space-md, 1rem); flex-wrap: wrap; font-size: var(--b-text-sm, 0.8125rem); }
      .footer-links a { color: var(--b-color-primary); text-decoration: none; }
      .footer-links a:hover { text-decoration: underline; }

      .endpoints dt { font-weight: 600; color: var(--b-text-secondary); }
      .endpoints dd { margin: 0 0 var(--b-space-xs, 0.25rem); }
      .endpoints code {
        background: var(--b-bg-tertiary); padding: 0.1rem 0.3rem;
        border-radius: var(--b-radius-sm, 4px); word-break: break-all;
      }
    `;
  }

  render() {
    const creds = hasCredentials();
    const sk    = settingsStore.get('finStatApiUrl');
    const cz    = settingsStore.get('finStatApiUrlCZ');
    return `
      <h1>${t('page.landing.title')}</h1>
      <p class="tagline">${t('page.landing.tagline')}</p>

      <b-card header="${t('page.landing.gettingStarted')}">
        <div class="creds">
          <span class="creds-state ${creds ? 'ok' : 'miss'}">
            ${creds ? '&#10003;' : '&#9888;'} ${creds ? t('page.landing.credsOk') : t('page.landing.credsMissing')}
          </span>
          <b-button variant="${creds ? 'ghost' : 'primary'}" id="btn-settings">
            ${creds ? t('page.landing.editSettings') : t('page.landing.openSettings')}
          </b-button>
        </div>
        <p class="tagline" style="margin-top: var(--b-space-sm, 0.5rem);">${t('page.landing.credsHelp')}</p>
      </b-card>

      <b-card header="${t('page.landing.endpoints')}">
        <dl class="endpoints">
          <dt>${t('settings.field.finStatApiUrl')}</dt>
          <dd><code>${escapeHtml(sk || t('page.landing.endpointUnset'))}</code></dd>
          <dt>${t('settings.field.finStatApiUrlCZ')}</dt>
          <dd><code>${escapeHtml(cz || t('page.landing.endpointUnset'))}</code></dd>
        </dl>
      </b-card>

      <b-card header="${t('page.landing.quickLinks')}">
        <div class="grid">
          <a class="tile" href="#/sk/detail/basic">
            <div class="title"><span class="icon">&#127970;</span>${t('page.landing.link.skDetail')}</div>
            <div class="desc">${t('page.landing.link.skDetailDesc')}</div>
          </a>
          <a class="tile" href="#/sk/autocomplete">
            <div class="title"><span class="icon">&#128269;</span>${t('page.landing.link.skAutocomplete')}</div>
            <div class="desc">${t('page.landing.link.skAutocompleteDesc')}</div>
          </a>
          <a class="tile" href="#/sk/monitoring/categories">
            <div class="title"><span class="icon">&#128065;</span>${t('page.landing.link.skMonitoring')}</div>
            <div class="desc">${t('page.landing.link.skMonitoringDesc')}</div>
          </a>
          <a class="tile" href="#/sk/statement/list">
            <div class="title"><span class="icon">&#128209;</span>${t('page.landing.link.skStatement')}</div>
            <div class="desc">${t('page.landing.link.skStatementDesc')}</div>
          </a>
          <a class="tile" href="#/sk/distraint/search">
            <div class="title"><span class="icon">&#9878;</span>${t('page.landing.link.skDistraint')}</div>
            <div class="desc">${t('page.landing.link.skDistraintDesc')}</div>
          </a>
          <a class="tile" href="#/cz/detail/basic">
            <div class="title"><span class="icon">&#127970;</span>${t('page.landing.link.czDetail')}</div>
            <div class="desc">${t('page.landing.link.czDetailDesc')}</div>
          </a>
        </div>
      </b-card>

      <div class="footer-links">
        <a href="${DOCS_URL}" target="_blank" rel="noopener">${t('page.landing.apiDocs')} &#8599;</a>
      </div>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    this._unsubLocale = onLocaleChange(() => this.update());
    // Re-render when credentials or endpoint URLs are changed in Settings so the
    // landing cards stay in sync without needing a full reload.
    this._unsubs.push(
      settingsStore.onChange('apiKey',          () => this.update()),
      settingsStore.onChange('privateKey',      () => this.update()),
      settingsStore.onChange('finStatApiUrl',   () => this.update()),
      settingsStore.onChange('finStatApiUrlCZ', () => this.update()),
    );
  }

  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    this._unsubLocale?.();
    this._unsubs.forEach(u => u());
    this._unsubs = [];
  }

  private _onClick = (e: Event) => {
    const target = e.target as HTMLElement;
    if (target.closest('#btn-settings')) {
      window.dispatchEvent(new CustomEvent(OPEN_SETTINGS_EVENT));
    }
  };
}

function escapeHtml(s: string): string {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

define('landing-page', LandingPage);
