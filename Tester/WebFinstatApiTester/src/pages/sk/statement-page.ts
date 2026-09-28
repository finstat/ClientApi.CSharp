import { BaseComponent, define } from 'birko-web-core';
import type { BInput, BSelect } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skStatement, TEMPLATES, type Template } from '../../api/sk-statement.js';
import { cacheStore } from '../../api/cache-store.js';
import { applyPendingPrefill, emphasizeAction, parseRouteSegments, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Action = 'list' | 'list-quiet' | 'detail' | 'legend';

function snap() { return cacheStore.get('skStatementItems'); }
type StatementCache = ReturnType<typeof snap>;

export class SkStatementPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;
  private _unsubCache?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.statement.title')}</h2>
      <p class="hint">${t('page.sk.statement.hint')}</p>

      <b-card header="${t('common.parameters')}">
        <div class="grid grid-4">
          <b-input  id="ico"  label="${t('common.field.ico')}"  placeholder="${t('common.field.icoPlaceholder')}"></b-input>
          <b-input  id="year" type="number" label="${t('common.field.year')}" placeholder="${t('common.field.yearPlaceholder')}"></b-input>
          <b-select id="tpl"  label="${t('common.field.template')}"></b-select>
          <b-select id="lang" label="${t('common.field.language')}"></b-select>
        </div>
        <div class="buttons">
          <b-button variant="primary" data-action="list">${t('ribbon.item.list')}</b-button>
          <b-button variant="ghost"   data-action="list-quiet">${t('common.runWithoutRedirect')}</b-button>
          <b-button variant="primary" data-action="detail">${t('ribbon.item.detail')}</b-button>
          <b-button variant="primary" data-action="legend">${t('ribbon.item.legend')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    const lang = this.$<BSelect>('#lang');
    lang?.setOptions([{ value: 'sk', label: 'sk' }, { value: 'en', label: 'en' }]);
    lang?.setAttribute('value', 'sk');
    this.shadowRoot?.addEventListener('click', this._onClick);
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
    this._unsubCache = cacheStore.on('skStatementItems', items => this._applyCache(items));
    this._applyCache(snap());
  }

  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
    this._unsubCache?.();
  }
  protected onUpdated() {
    this._applyCache(snap());
    this._applyRouteAction();
    applyPendingPrefill(this.shadowRoot);
  }

  private _applyCache(cache: StatementCache) {
    this._applyYears(cache);
    this._applyTemplates(cache);
  }

  private _applyYears(cache: StatementCache) {
    const inp = this.$<BInput>('#year');
    if (!inp) return;
    const items = cache?.items ?? [];
    if (!items.length) {
      inp.removeAttribute('hint');
      inp.setSuggestions([]);
      return;
    }
    inp.setSuggestions(items.map(it => String(it.year)));
    inp.setAttribute('hint', t('page.sk.statement.yearFromIco', { ico: cache?.ico ?? '' }));
    if (!inp.value) inp.value = String(items[0].year);
  }

  private _applyTemplates(cache: StatementCache) {
    const sel = this.$<BSelect>('#tpl');
    if (!sel) return;
    const items = cache?.items ?? [];
    const cachedTpls = items.length
      ? Array.from(new Set(items.flatMap(it => it.templates))).filter((x): x is Template => (TEMPLATES as readonly string[]).includes(x))
      : [];
    const available: readonly Template[] = cachedTpls.length ? cachedTpls : TEMPLATES;
    sel.setOptions(available.map(t2 => ({ value: t2, label: t2 })));
    const current = sel.value as Template | '';
    const next: Template = current && available.includes(current as Template)
      ? current as Template
      : (available.includes('Template2014' as Template) ? 'Template2014' as Template : available[0]);
    sel.setAttribute('value', next);
  }

  private _onHashChange = () => this._applyRouteAction();

  private _applyRouteAction() {
    const [action] = parseRouteSegments('#/sk/statement/');
    const focus = action === 'legend' ? '#lang' : '#ico';
    emphasizeAction(this.shadowRoot, action, { focus });
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    const action = btn?.dataset.action as Action | undefined;
    if (action) void this._run(action);
  };

  private async _run(action: Action) {
    if (this._busy) return;
    const icoI  = this.$<BInput>('#ico');
    const yearI = this.$<BInput>('#year');
    const tpl   = (this.$<BSelect>('#tpl')?.value ?? 'Template2014') as Template;
    const ico   = (icoI?.value  ?? '').trim();
    const year  = Number((yearI?.value ?? '').toString().trim());
    const lang  = this.$<BSelect>('#lang')?.value || 'sk';

    if (action === 'list' || action === 'list-quiet') {
      if (!ico) { toast.error(t('page.sk.statement.icoRequired')); icoI?.setAttribute('error', t('common.required')); icoI?.focus(); return; }
      icoI?.removeAttribute('error');
      this._busy = true;
      try {
        await runApiCall(
          t('label.sk.statement.list'),
          () => skStatement.list(ico),
          { silent: action === 'list-quiet' },
        );
      } finally { this._busy = false; }
      return;
    }

    if (action === 'detail') {
      if (!ico) { toast.error(t('page.sk.statement.icoRequired')); icoI?.setAttribute('error', t('common.required')); icoI?.focus(); return; }
      if (!Number.isFinite(year) || year <= 0) {
        toast.error(t('page.sk.statement.yearInvalid')); yearI?.setAttribute('error', t('common.invalid')); return;
      }
      icoI?.removeAttribute('error'); yearI?.removeAttribute('error');
      this._busy = true;
      try { await runApiCall(t('label.sk.statement.detail'), () => skStatement.detail(ico, year, tpl)); }
      finally { this._busy = false; }
      return;
    }

    this._busy = true;
    try { await runApiCall(t('label.sk.statement.legend'), () => skStatement.legend(tpl, lang)); }
    finally { this._busy = false; }
  }
}

define('sk-statement-page', SkStatementPage);
