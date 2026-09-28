import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skDistraint } from '../../api/sk-distraint.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

/** RequestDistraintResultsByToken — fetch cached search results without re-charging. */
export class SkDistraintResultsByTokenPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.distraint.resultsByTokenTitle')}</h2>
      <p class="hint">${t('page.sk.distraint.resultsByTokenHint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="row">
          <b-input id="token" label="${t('common.field.token')}" required></b-input>
          <b-button variant="primary" id="run">${t('page.sk.distraint.resultsByTokenButton')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.$('#run')?.addEventListener('click', () => void this._run());
    this._unsubLocale = onLocaleChange(() => this.update());
    applyPendingPrefill(this.shadowRoot);
  }

  protected onUnmount() {
    this._unsubLocale?.();
  }

  private async _run() {
    if (this._busy) return;
    const input = this.$<BInput>('#token');
    const token = (input?.value ?? '').trim();
    if (!token) { toast.error(t('page.sk.distraint.tokenRequired')); input?.setAttribute('error', t('common.required')); input?.focus(); return; }
    input?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t('label.sk.distraint.resultsByToken'), () => skDistraint.resultsByToken(token)); }
    finally { this._busy = false; }
  }
}

define('sk-distraint-results-by-token-page', SkDistraintResultsByTokenPage);
