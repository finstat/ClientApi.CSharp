import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skBankruptcy } from '../../api/sk-bankruptcy.js';
import { applyPendingPrefill, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class SkBankruptcyPersonsPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.bankruptcy.personsTitle')}</h2>
      <p class="hint">${t('page.sk.bankruptcy.personsHint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="grid grid-3">
          <b-input id="name"    label="${t('common.field.name')}"          required></b-input>
          <b-input id="surname" label="${t('common.field.surname')}"       required></b-input>
          <b-date-picker id="dob" label="${t('common.field.dob')}" required></b-date-picker>
        </div>
        <div class="actions">
          <b-button variant="primary" id="run">${t('page.sk.bankruptcy.personsButton')}</b-button>
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
    const nameI    = this.$<BInput>('#name');
    const surnameI = this.$<BInput>('#surname');
    const dobI     = this.$<BInput>('#dob');
    const name    = (nameI?.value    ?? '').trim();
    const surname = (surnameI?.value ?? '').trim();
    const dob     = (dobI?.value     ?? '').trim();

    const missing: Array<[BInput | null, string]> = [
      [nameI ?? null, name], [surnameI ?? null, surname], [dobI ?? null, dob],
    ];
    const bad = missing.find(([, v]) => !v);
    if (bad) {
      toast.error(t('page.sk.bankruptcy.fillPerson'));
      bad[0]?.setAttribute('error', t('common.required'));
      bad[0]?.focus();
      return;
    }
    missing.forEach(([el]) => el?.removeAttribute('error'));

    this._busy = true;
    try { await runApiCall(t('label.sk.bankruptcy.persons'), () => skBankruptcy.person(name, surname, dob)); }
    finally { this._busy = false; }
  }
}

define('sk-bankruptcy-persons-page', SkBankruptcyPersonsPage);
