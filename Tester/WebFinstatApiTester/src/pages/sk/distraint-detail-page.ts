import { BaseComponent, define } from 'birko-web-core';
import type { BInput, BTagInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skDistraint } from '../../api/sk-distraint.js';
import { confirmCharge } from '../charge-gate.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

/** RequestDistraintDetail — needs a token from a prior Search, plus a comma-separated list of result ids. */
export class SkDistraintDetailPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.distraint.detailTitle')}</h2>
      <p class="hint">${t('page.sk.distraint.detailHint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="grid grid-2">
          <b-input id="token" label="${t('common.field.token')}" required></b-input>
          <b-tag-input id="ids" label="${t('common.field.ids')}" placeholder="${t('common.field.idsPlaceholder')}" required></b-tag-input>
        </div>
        <div class="actions">
          <b-button variant="primary" id="run">${t('page.sk.distraint.detailButton')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.$('#run')?.addEventListener('click', () => void this._run());
    // Accept space as a separator too (matches legacy "1 2 3" paste behaviour); keep defaults for comma / newline / tab.
    this.$<BTagInput>('#ids')?.setAttribute('separators', ' ,\n\t');
    this._unsubLocale = onLocaleChange(() => this.update());
    applyPendingPrefill(this.shadowRoot);
  }

  protected onUnmount() {
    this._unsubLocale?.();
  }

  private async _run() {
    if (this._busy) return;
    const tokenInput = this.$<BInput>('#token');
    const idsInput   = this.$<BTagInput>('#ids');
    const token = (tokenInput?.value ?? '').trim();

    if (!token) { toast.error(t('page.sk.distraint.tokenRequired')); tokenInput?.setAttribute('error', t('common.required')); tokenInput?.focus(); return; }
    tokenInput?.removeAttribute('error');

    const ids = (idsInput?.getTags() ?? []).map(Number);
    if (ids.length === 0 || ids.some(n => !Number.isFinite(n))) {
      toast.error(t('page.sk.distraint.idsInvalid'));
      idsInput?.setAttribute('error', t('common.invalid'));
      return;
    }
    idsInput?.removeAttribute('error');

    // Paid endpoint — same gate as the desktop tester (and as Distraint Search).
    if (!await confirmCharge()) return;

    this._busy = true;
    try { await runApiCall(t('label.sk.distraint.detail'), () => skDistraint.detail(token, ids)); }
    finally { this._busy = false; }
  }
}

define('sk-distraint-detail-page', SkDistraintDetailPage);
