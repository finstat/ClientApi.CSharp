import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skDistraint } from '../../api/sk-distraint.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

/** RequestDistraintStoredDetail — fetch details of a previously stored result by id. */
export class SkDistraintStoredDetailPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.distraint.storedDetailTitle')}</h2>
      <p class="hint">${t('page.sk.distraint.storedDetailHint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="row">
          <b-input id="id" label="${t('common.field.id')}" required></b-input>
          <b-button variant="primary" id="run">${t('page.sk.distraint.storedDetailButton')}</b-button>
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
    const input = this.$<BInput>('#id');
    const id = (input?.value ?? '').trim();
    if (!id) { toast.error(t('page.sk.distraint.idRequired')); input?.setAttribute('error', t('common.required')); input?.focus(); return; }
    input?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t('label.sk.distraint.storedDetail'), () => skDistraint.storedDetail(id)); }
    finally { this._busy = false; }
  }
}

define('sk-distraint-stored-detail-page', SkDistraintStoredDetailPage);
