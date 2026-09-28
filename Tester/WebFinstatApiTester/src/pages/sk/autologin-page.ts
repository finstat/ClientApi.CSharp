import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skAuto } from '../../api/sk-auto.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class SkAutoLoginPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.autologin.title')}</h2>
      <p class="hint">${t('page.sk.autologin.hint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="grid grid-2">
          <b-input id="url" label="${t('common.field.url')}" placeholder="${t('common.field.urlPlaceholderSk')}" required></b-input>
          <b-input id="email" type="email" label="${t('common.field.email')}" placeholder="${t('common.field.emailPlaceholderSk')}"></b-input>
        </div>
        <div class="actions">
          <b-button variant="primary" id="run">${t('page.sk.autologin.button')}</b-button>
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
    const urlInput   = this.$<BInput>('#url');
    const emailInput = this.$<BInput>('#email');
    const url   = (urlInput?.value   ?? '').trim();
    const email = (emailInput?.value ?? '').trim() || undefined;
    if (!url) {
      toast.error(t('page.sk.autologin.enterUrl'));
      urlInput?.setAttribute('error', t('common.required'));
      urlInput?.focus();
      return;
    }
    urlInput?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t('label.sk.autoLogin'), () => skAuto.autoLogin(url, email)); }
    finally { this._busy = false; }
  }
}

define('sk-autologin-page', SkAutoLoginPage);
