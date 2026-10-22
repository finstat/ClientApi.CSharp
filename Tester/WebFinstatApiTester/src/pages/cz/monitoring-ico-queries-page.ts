import { BaseComponent, define } from 'birko-web-core';
import type { BSelect } from 'birko-web-components';

import { czMonitoring } from '../../api/cz-monitoring.js';
import { cacheStore, type MonitoringCategoryOption } from '../../api/cache-store.js';
import { applyLangOptions, applyPendingPrefill, cacheHintMarkup, emphasizeAction, langSelectMarkup, parseRouteSegments, readLang, runApiCall, toggleCacheHint } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Action = 'list' | 'report';

function categoryOptions(cats: readonly MonitoringCategoryOption[]): { value: string; label: string }[] {
  return [
    { value: '', label: t('common.field.defaultCategory') },
    ...cats.map(c => ({ value: c.category, label: c.name })),
  ];
}

export class CzMonitoringIcoQueriesPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;
  private _unsubCache?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.cz.monitoring.icoQueriesTitle')}</h2>
      <p class="hint">${t('page.cz.monitoring.icoQueriesHint')}</p>

      <b-card header="${t('common.queries')}">
        <div class="row">
          <b-select id="cat" label="${t('common.field.categoryOpt')}"></b-select>
          ${langSelectMarkup()}
        </div>
${cacheHintMarkup({ id: 'cat-hint', action: 'load-cats', label: t('common.loadCategories') })}
        <div class="buttons">
          <b-button variant="primary" data-action="list">${t('ribbon.item.list')}</b-button>
          <b-button variant="primary" data-action="report">${t('ribbon.item.report')}</b-button>
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
    const [action] = parseRouteSegments('#/cz/monitoring/');
    emphasizeAction(this.shadowRoot, action, { focus: '#cat' });
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

    this._busy = true;
    try {
      await runApiCall(
        t(`label.cz.monitoring.${action}`),
        () => action === 'list' ? czMonitoring.list(category, lang) : czMonitoring.report(category, lang),
      );
    } finally { this._busy = false; }
  }
}

define('cz-monitoring-ico-queries-page', CzMonitoringIcoQueriesPage);
