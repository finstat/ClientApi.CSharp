import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skBankruptcy } from '../../api/sk-bankruptcy.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class SkBankruptcyCompanyPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.bankruptcy.companyTitle')}</h2>
      <p class="hint">${t('page.sk.bankruptcy.companyHint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="grid grid-2">
          <b-input id="ico"  label="${t('common.field.ico')}"          placeholder="${t('common.field.icoPlaceholder')}"></b-input>
          <b-input id="name" label="${t('common.field.companyName')}"                              ></b-input>
        </div>
        <div class="actions">
          <b-button variant="primary" id="run">${t('page.sk.bankruptcy.companyButton')}</b-button>
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
    const icoI  = this.$<BInput>('#ico');
    const nameI = this.$<BInput>('#name');
    const ico   = (icoI?.value  ?? '').trim();
    const name  = (nameI?.value ?? '').trim();

    if (!ico && !name) {
      toast.error(t('page.sk.bankruptcy.companyFillOne'));
      icoI?.setAttribute('error', t('common.required'));
      nameI?.setAttribute('error', t('common.required'));
      icoI?.focus();
      return;
    }
    icoI?.removeAttribute('error');
    nameI?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t('label.sk.bankruptcy.company'), () => skBankruptcy.company(ico, name)); }
    finally { this._busy = false; }
  }
}

define('sk-bankruptcy-company-page', SkBankruptcyCompanyPage);
