import { BaseComponent, define } from 'birko-web-core';
import type { BInput, BSelect } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skMonitoring } from '../../api/sk-monitoring.js';
import { cacheStore, type MonitoringCategoryOption } from '../../api/cache-store.js';
import { applyPendingPrefill, cacheHintMarkup, emphasizeAction, parseRouteSegments, runApiCall, toggleCacheHint } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type IcoAction = 'add' | 'remove';

function categoryOptions(cats: readonly MonitoringCategoryOption[]): { value: string; label: string }[] {
  return [
    { value: '', label: t('common.field.defaultCategory') },
    ...cats.map(c => ({ value: c.category, label: c.name })),
  ];
}

export class SkMonitoringIcoPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;
  private _unsubCache?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.monitoring.icoTitle')}</h2>
      <p class="hint">${t('page.sk.monitoring.icoHint')}</p>

      <b-card>
        <div class="row">
          <b-input  id="ico" label="${t('common.field.ico')}" placeholder="${t('common.field.icoPlaceholder')}" required></b-input>
          <b-select id="cat" label="${t('common.field.categoryOpt')}"></b-select>
        </div>
${cacheHintMarkup({ id: 'cat-hint', action: 'load-cats', label: t('common.loadCategories') })}
        <div class="buttons">
          <b-button variant="primary" data-action="add">${t('ribbon.item.add')}</b-button>
          <b-button variant="danger"  data-action="remove">${t('ribbon.item.remove')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
    this._unsubCache = cacheStore.on('skMonitoringCategories', cats => this._applyCategories(cats));
  }
  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
    this._unsubCache?.();
  }
  protected onUpdated() {
    this._applyCategories(cacheStore.get('skMonitoringCategories'));
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
    const [action] = parseRouteSegments('#/sk/monitoring/');
    emphasizeAction(this.shadowRoot, action, { focus: '#ico' });
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    const action = btn?.dataset.action;
    if (!action) return;
    if (action === 'load-cats') { void this._loadCategories(); return; }
    void this._run(action as IcoAction);
  };

  private async _loadCategories() {
    if (this._busy) return;
    this._busy = true;
    try { await runApiCall(t('label.sk.monitoring.categories'), () => skMonitoring.categories(), { silent: true }); }
    finally { this._busy = false; }
  }

  private async _run(action: IcoAction) {
    if (this._busy) return;
    const category = (this.$<BSelect>('#cat')?.value ?? '').trim() || undefined;

    const icoInput = this.$<BInput>('#ico');
    const ico = (icoInput?.value ?? '').trim();
    if (!ico) {
      toast.error(t('page.sk.detail.enterIco'));
      icoInput?.setAttribute('error', t('common.required'));
      icoInput?.focus();
      return;
    }
    icoInput?.removeAttribute('error');

    this._busy = true;
    try {
      await runApiCall(
        action === 'add' ? t('label.sk.monitoring.add') : t('label.sk.monitoring.remove'),
        () => action === 'add'
          ? skMonitoring.add(ico, category)
          : skMonitoring.remove(ico, category),
      );
    } finally { this._busy = false; }
  }
}

define('sk-monitoring-ico-page', SkMonitoringIcoPage);
