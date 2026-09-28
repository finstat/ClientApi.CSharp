import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skAuto } from '../../api/sk-auto.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class SkAutocompletePage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.autocomplete.title')}</h2>
      <p class="hint">${t('page.sk.autocomplete.hint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="row">
          <b-input id="query" label="${t('common.field.query')}" placeholder="${t('common.field.queryPlaceholder')}" required></b-input>
          <b-button variant="primary" id="run">${t('page.sk.autocomplete.button')}</b-button>
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
    applyPendingPrefill(this.shadowRoot);
  }

  protected onUnmount() {
    this._unsubLocale?.();
  }

  private async _run() {
    if (this._busy) return;
    const input = this.$<BInput>('#query');
    const query = (input?.value ?? '').trim();
    if (!query) {
      toast.error(t('page.sk.autocomplete.enterQuery'));
      input?.setAttribute('error', t('common.required'));
      input?.focus();
      return;
    }
    input?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t('label.sk.autoComplete'), () => skAuto.autocomplete(query)); }
    finally { this._busy = false; }
  }
}

define('sk-autocomplete-page', SkAutocompletePage);
