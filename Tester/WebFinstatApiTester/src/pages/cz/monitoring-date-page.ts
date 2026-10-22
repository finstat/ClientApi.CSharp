import { BaseComponent, define } from 'birko-web-core';
import type { BDatePicker, BSelect } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { czMonitoring } from '../../api/cz-monitoring.js';
import { cacheStore, type MonitoringCategoryOption } from '../../api/cache-store.js';
import { applyLangOptions, applyPendingPrefill, cacheHintMarkup, emphasizeAction, langSelectMarkup, parseRouteSegments, readLang, runApiCall, toggleCacheHint } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Action = 'add' | 'remove';

function categoryOptions(cats: readonly MonitoringCategoryOption[]): { value: string; label: string }[] {
  return [
    { value: '', label: t('common.field.defaultCategory') },
    ...cats.map(c => ({ value: c.category, label: c.name })),
  ];
}

export class CzMonitoringDatePage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;
  private _unsubCache?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.cz.monitoring.dateTitle')}</h2>
      <p class="hint">${t('page.cz.monitoring.dateHint')}</p>

      <b-card>
        <div class="row">
          <b-date-picker id="date" label="${t('common.field.date')}" required></b-date-picker>
          <b-select      id="cat"  label="${t('common.field.categoryOpt')}"></b-select>
          ${langSelectMarkup()}
        </div>
${cacheHintMarkup({ id: 'cat-hint', action: 'load-cats', label: t('common.loadCategories') })}
        <div class="buttons">
          <b-button variant="primary" data-action="add">${t('page.cz.monitoring.dateAdd')}</b-button>
          <b-button variant="danger"  data-action="remove">${t('page.cz.monitoring.dateRemove')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
    this._unsubCache = cacheStore.on('czMonitoringCategories', cats => this._applyCategories(cats));
  }
  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
    this._unsubCache?.();
  }
  protected onUpdated() {
    applyLangOptions(this.shadowRoot);
    this._applyCategories(cacheStore.get('czMonitoringCategories'));
    this._applyRouteAction();
    applyPendingPrefill(this.shadowRoot);
  }

  private _applyCategories(cats: readonly MonitoringCategoryOption[]) {
    const sel = this.$<BSelect>('#cat');
    sel?.setOptions(categoryOptions(cats));
    if (!sel?.getAttribute('value')) sel?.setAttribute('value', '');
    toggleCacheHint(this.shadowRoot, 'cat-hint', cats.length > 0);
  }

  private _onHashChange = () => this._applyRouteAction();

  private _applyRouteAction() {
    const [, action] = parseRouteSegments('#/cz/monitoring/');
    emphasizeAction(this.shadowRoot, action, { focus: '#date' });
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    const action = btn?.dataset.action;
    if (!action) return;
    if (action === 'load-cats') { void this._loadCategories(); return; }
    void this._run(action as Action);
  };

  private async _loadCategories() {
    if (this._busy) return;
    this._busy = true;
    try { await runApiCall(t('label.cz.monitoring.categories'), () => czMonitoring.categories(readLang(this.shadowRoot)), { silent: true }); }
    finally { this._busy = false; }
  }

  private async _run(action: Action) {
    if (this._busy) return;
    const category = (this.$<BSelect>('#cat')?.value ?? '').trim() || undefined;
    const lang = readLang(this.shadowRoot);
    const dateInput = this.$<BDatePicker>('#date');
    const date = (dateInput?.value ?? '').trim();
    if (!date) {
      toast.error(t('page.cz.monitoring.pickDate'));
      dateInput?.setAttribute('error', t('common.required'));
      dateInput?.focus();
      return;
    }
    dateInput?.removeAttribute('error');

    this._busy = true;
    try {
      await runApiCall(
        action === 'add' ? t('label.cz.monitoring.dateAdd') : t('label.cz.monitoring.dateRemove'),
        () => action === 'add' ? czMonitoring.date.add(date, category, lang) : czMonitoring.date.remove(date, category, lang),
      );
    } finally { this._busy = false; }
  }
}

define('cz-monitoring-date-page', CzMonitoringDatePage);
