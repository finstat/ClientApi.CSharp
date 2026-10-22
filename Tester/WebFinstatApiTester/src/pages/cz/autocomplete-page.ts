import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { czAuto } from '../../api/cz-auto.js';
import { applyLangOptions, applyPendingPrefill, langSelectMarkup, readLang, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class CzAutocompletePage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.cz.autocomplete.title')}</h2>
      <p class="hint">${t('page.cz.autocomplete.hint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="row">
          <b-input id="query" label="${t('common.field.query')}" placeholder="${t('common.field.queryPlaceholder')}" required></b-input>
          ${langSelectMarkup()}
          <b-button variant="primary" id="run">${t('page.cz.autocomplete.button')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.$('#run')?.addEventListener('click', () => void this._run());
    this.$<BInput>('#query')?.addEventListener('keydown', (e: Event) => {
      if ((e as KeyboardEvent).key === 'Enter') void this._run();
    });
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    this._unsubLocale?.();
  }

  protected onUpdated() { applyLangOptions(this.shadowRoot); applyPendingPrefill(this.shadowRoot); }

  private async _run() {
    if (this._busy) return;
    const input = this.$<BInput>('#query');
    const query = (input?.value ?? '').trim();
    if (!query) { toast.error(t('page.cz.autocomplete.enterQuery')); input?.setAttribute('error', t('common.required')); input?.focus(); return; }
    input?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t('label.cz.autoComplete'), () => czAuto.autocomplete(query, readLang(this.shadowRoot))); }
    finally { this._busy = false; }
  }
}

define('cz-autocomplete-page', CzAutocompletePage);
