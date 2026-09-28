import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skDistraint, type DistraintQuery } from '../../api/sk-distraint.js';
import { confirmCharge } from '../charge-gate.js';
import { applyPendingPrefill, emphasizeAction, parseRouteSegments, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Action = 'search' | 'results';

/**
 * Shared by two ribbon items — `Search` and `Results` — since both take the same
 * 6-parameter query. Two buttons, same inputs.
 */
export class SkDistraintQueryPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.distraint.queryTitle')}</h2>
      <p class="hint">${t('page.sk.distraint.queryHint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="grid grid-3">
          <b-input id="ico"     label="${t('common.field.ico')}"           placeholder="${t('common.field.icoPlaceholder')}"></b-input>
          <b-input id="surname" label="${t('common.field.surname')}"       placeholder="person"></b-input>
          <b-date-picker id="dob" label="${t('common.field.dob')}"></b-date-picker>
          <b-input id="city"    label="${t('common.field.city')}"></b-input>
          <b-input id="company" label="${t('common.field.companyName')}"></b-input>
          <b-input id="ref"     label="${t('common.field.fileReference')}"></b-input>
        </div>
        <div class="buttons">
          <b-button variant="primary" data-action="search">${t('ribbon.item.search')}</b-button>
          <b-button variant="primary" data-action="results">${t('ribbon.item.results')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
  }
  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
  }
  protected onUpdated() { this._applyRouteAction(); applyPendingPrefill(this.shadowRoot); }

  private _onHashChange = () => this._applyRouteAction();

  private _applyRouteAction() {
    const [action] = parseRouteSegments('#/sk/distraint/');
    emphasizeAction(this.shadowRoot, action, { focus: '#ico' });
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    const action = btn?.dataset.action as Action | undefined;
    if (action) void this._run(action);
  };

  private _val(id: string) { return (this.$<BInput>('#' + id)?.value ?? '').trim() || undefined; }

  private async _run(action: Action) {
    if (this._busy) return;

    const q: DistraintQuery = {
      ico:           this._val('ico'),
      surname:       this._val('surname'),
      dateOfBirth:   this._val('dob'),
      city:          this._val('city'),
      companyName:   this._val('company'),
      fileReference: this._val('ref'),
    };
    if (!Object.values(q).some(v => v)) {
      toast.error(t('page.sk.distraint.fillOne'));
      return;
    }

    // Search bills the account; Results only re-reads an earlier search. Same gate the
    // desktop tester puts in front of the two paid distraint endpoints.
    if (action === 'search' && !await confirmCharge()) return;

    this._busy = true;
    try {
      await runApiCall(
        action === 'search' ? t('label.sk.distraint.search') : t('label.sk.distraint.results'),
        () => action === 'search' ? skDistraint.search(q) : skDistraint.results(q),
      );
    } finally { this._busy = false; }
  }
}

define('sk-distraint-query-page', SkDistraintQueryPage);
