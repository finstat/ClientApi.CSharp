import { BaseComponent, define } from 'birko-web-core';
import type { BSelect, BTagInput, BInput } from 'birko-web-components';
import type { BTable, TableColumn } from 'birko-web-components/data';
import { toast } from 'birko-web-components';

import { hasCredentials } from '../../settings/settings-store.js';
import { OPEN_SETTINGS_EVENT } from '../../shell/app-shell.js';
import { BATCH_ENDPOINTS, batchEndpointOptions, findBatchEndpoint, type BatchEndpoint, type ExtraField } from './registry.js';
import { runBatch, type BatchHandle, type RowStatus } from './runner.js';
import { cacheStore } from '../../api/cache-store.js';
import { failTypeLabel } from '../page-helpers.js';
import { t, onLocaleChange } from '../../i18n/index.js';

const CONCURRENCY_OPTIONS = [1, 2, 4, 8, 16].map(n => ({ value: String(n), label: String(n) }));

interface ResultRow extends Record<string, unknown> {
  _i: number;
  value: string;
  state: RowStatus['state'];
  status?: number | string;
  durationMs?: number;
  error?: string;
  failType?: string;
  historyId?: string;
}

/**
 * Reusable batch runner body — lives inside `<batch-drawer>`. Owns its own state
 * (endpoint / concurrency / last results) so reopening the drawer preserves context.
 *
 * IMPORTANT: `render()` is the single source of truth for attributes like `value=`
 * on child `<b-select>` elements. The base component's morph algorithm strips
 * attributes not present in the fresh render, so we must always emit them here.
 */
export class BatchRunner extends BaseComponent {
  private _currentId: string | null = null;
  private _concurrency = '4';
  private _handle: BatchHandle | null = null;
  private _rows: RowStatus[] = [];
  private _done = 0;
  private _total = 0;
  private _running = false;

  static get styles() {
    return `
      :host { display: block; color: var(--b-text); }
      b-card { margin-bottom: var(--b-space-md, 1rem); }
      .grid { display: grid; gap: var(--b-space-md, 0.75rem); }
      .grid-2 { grid-template-columns: 1fr 1fr; }
      .buttons { display: flex; gap: var(--b-space-sm, 0.5rem); flex-wrap: wrap; margin-top: var(--b-space-md, 1rem); }
      .progress-area { margin-top: var(--b-space-sm, 0.5rem); }
      .status {
        display: flex; align-items: center; gap: var(--b-space-md, 1rem);
        margin-top: var(--b-space-sm, 0.5rem); font-size: var(--b-text-sm, 0.8125rem);
        color: var(--b-text-secondary);
      }
      .state-ok   { color: var(--b-color-success, #16a34a); }
      .state-fail { color: var(--b-color-danger,  #dc2626); }
      .values-wrap { margin-top: var(--b-space-sm, 0.5rem); }
    `;
  }

  /** Pre-select an endpoint — only changes anything when the id exists in the registry. */
  preselect(id: string | null) {
    if (!id || !findBatchEndpoint(id)) return;
    if (this._currentId === id) return;
    this._currentId = id;
    this._rows = [];
    this._done = 0; this._total = 0;
    this.update();
    this._syncTable();
  }

  render() {
    const ep = this._current();
    const extras = ep?.extras ?? [];
    const okCount   = this._rows.filter(r => r.state === 'ok').length;
    const failCount = this._rows.filter(r => r.state === 'fail').length;

    const epValueAttr = this._currentId ? ` value="${this._currentId}"` : '';
    const ccValueAttr = ` value="${this._concurrency}"`;

    const itemsLabel = ep ? t('batch.field.items', { inputLabel: ep.inputLabel }) : '';
    const itemsPlaceholder = ep?.placeholder ?? '';
    const formHeader = ep ? t('batch.entryLabel', { group: ep.group, label: ep.label }) : '';

    // Conditional templating instead of the `hidden` attribute — most birko components
    // define `:host { display: block }` which beats UA `[hidden] { display: none }` in
    // the cascade, so the attribute alone leaves an empty-chrome card/table visible.
    return `
      <b-card header="${escapeHtml(t('common.parameters'))}">
        <div class="grid grid-2">
          <b-select id="endpoint"    label="${t('batch.field.endpoint')}"    required${epValueAttr}></b-select>
          <b-select id="concurrency" label="${t('batch.field.concurrency')}" required${ccValueAttr}></b-select>
        </div>
      </b-card>

      ${ep ? `
        <b-card header="${escapeHtml(formHeader)}">
          ${extras.length > 0 ? `
            <div class="grid grid-2">
              ${extras.map(f => renderExtraField(f)).join('')}
            </div>` : ''
          }

          <div class="values-wrap">
            <b-tag-input id="values"
                         label="${itemsLabel}"
                         placeholder="${itemsPlaceholder}"></b-tag-input>
          </div>

          <div class="buttons">
            <b-button variant="primary" id="btn-run"    ${this._running ? 'loading' : ''}>${t('batch.button.run')}</b-button>
            <b-button variant="danger"  id="btn-cancel" ${this._running ? '' : 'disabled'}>${t('batch.button.cancel')}</b-button>
          </div>
        </b-card>
      ` : `
        <b-empty icon="&#9889;" message="${escapeHtml(t('batch.hint'))}"></b-empty>
      `}

      ${this._total > 0 ? `
        <div class="progress-area">
          <b-progress value="${this._done}" max="${this._total}" show-value value-format="fraction"
                      ${this._progressVariantAttr()}></b-progress>
          <div class="status">
            <span class="state-ok">&#10003; ${okCount}</span>
            <span class="state-fail">&#10005; ${failCount}</span>
            ${this._handle?.cancelled ? '<span class="state-fail">cancelled</span>' : ''}
          </div>
        </div>
      ` : ''}

      ${this._rows.length > 0 ? `<b-table id="results" hoverable></b-table>` : ''}
    `;
  }

  private _unsubLocale?: () => void;

  private _unsubCache: Array<() => void> = [];

  protected onMount() {
    // Populate selects once — their internal `_options` state persists across morphs.
    this.$<BSelect>('#endpoint')?.setOptions(batchEndpointOptions());
    this.$<BSelect>('#concurrency')?.setOptions(CONCURRENCY_OPTIONS);
    this._populateExtras();

    // Event delegation — listeners attach to the shadow root once, survive re-renders.
    this.shadowRoot?.addEventListener('change', this._onChange);
    this.shadowRoot?.addEventListener('click',  this._onClick);

    this._syncTable();

    // Keep cache-backed extras (e.g. category pickers) in sync with newly-loaded
    // categories — subscribing to both SK and CZ slots is cheap and lets us fire
    // a single `_populateExtras()` on any change.
    this._unsubCache.push(
      cacheStore.on('skMonitoringCategories', () => this._populateExtras()),
      cacheStore.on('czMonitoringCategories', () => this._populateExtras()),
    );

    this._unsubLocale = onLocaleChange(() => {
      this.$<BSelect>('#endpoint')?.setOptions(batchEndpointOptions());
      this.update();
      this._syncTable();
      this._populateExtras();
    });
  }

  protected onUpdated() {
    this._populateExtras();
  }

  protected onUnmount() {
    this.shadowRoot?.removeEventListener('change', this._onChange);
    this.shadowRoot?.removeEventListener('click',  this._onClick);
    this._handle?.cancel();
    this._unsubLocale?.();
    this._unsubCache.forEach(u => u());
    this._unsubCache = [];
  }

  /** Push fresh options into every extra field declared with a `source`. Preserves
   *  the user's current selection when it's still present in the new options. */
  private _populateExtras() {
    const ep = this._current();
    for (const f of ep?.extras ?? []) {
      if (!f.source) continue;
      const sel = this.$<BSelect>(`#extra-${f.name}`);
      if (!sel) continue;
      const prevValue = sel.value;
      sel.setOptions(f.source());
      if (prevValue && f.source().some(o => o.value === prevValue)) sel.setAttribute('value', prevValue);
      else sel.setAttribute('value', '');
    }
  }

  /** Focus the values input so "paste, Run" is the fastest path after the drawer opens. */
  focusValues() {
    queueMicrotask(() => this.$<BTagInput>('#values')?.focus());
  }

  private _current(): BatchEndpoint | null {
    return this._currentId ? findBatchEndpoint(this._currentId) ?? null : null;
  }

  /** Pick a b-progress variant based on the current run state. Empty while the batch is
   *  still running (default teal fill); resolves to success / warning / danger on finish. */
  private _progressVariantAttr(): string {
    if (this._running) return '';
    if (this._handle?.cancelled) return 'variant="warning"';
    const failed = this._rows.some(r => r.state === 'fail');
    if (this._done === this._total && this._done > 0) return failed ? 'variant="danger"' : 'variant="success"';
    return '';
  }

  /** Re-push columns + data into the `<b-table>`. Morph preserves its state across updates. */
  private _syncTable() {
    const table = this.$<BTable>('#results');
    if (!table) return;
    const ep = this._current();
    const valueLabel = ep?.inputLabel ?? t('common.value');

    const columns: TableColumn[] = [
      { key: '_i', label: '#', width: '3rem', align: 'right',
        render: (v) => `<span style="font-family:var(--b-font-family-mono,ui-monospace,Consolas,monospace)">${Number(v) + 1}</span>` },
      { key: 'value', label: valueLabel,
        render: (v) => `<span style="font-family:var(--b-font-family-mono,ui-monospace,Consolas,monospace)">${escapeHtml(String(v ?? ''))}</span>` },
      { key: 'status', label: t('history.meta.status'),
        render: (_v, row) => statusTag(row as unknown as RowStatus) },
      { key: 'durationMs', label: t('common.duration'), align: 'right',
        render: (v) => v != null ? `<span style="font-family:var(--b-font-family-mono,ui-monospace,Consolas,monospace)">${v} ms</span>` : '' },
      { key: 'error', label: t('history.meta.error'),
        render: (_v, row) => {
          const r = row as unknown as RowStatus;
          if (!r.error) return '';
          const label = failTypeLabel(r.failType);
          const badge = label
            ? `<span style="display:inline-block;margin-right:0.35rem;padding:0.05rem 0.4rem;border-radius:0.75rem;background:var(--b-color-danger-bg,#fee2e2);color:var(--b-color-danger,#dc2626);font-size:0.85em;font-weight:500" title="${escapeHtml(r.failType ?? '')}">${escapeHtml(label)}</span>`
            : '';
          return `${badge}<span style="color:var(--b-color-danger,#dc2626)">${escapeHtml(r.error)}</span>`;
        } },
      { key: 'historyId', label: '', width: '4rem',
        render: (v) => v ? `<a href="#/history/${v}" style="color:var(--b-color-primary);text-decoration:none">${t('common.view')}</a>` : '' },
    ];
    table.setColumns(columns);

    const data: ResultRow[] = this._rows.map((r, _i) => ({ ...r, _i }));
    table.setData(data);
  }

  private _onChange = (e: Event) => {
    const target = e.target as HTMLElement;
    if (target.id === 'endpoint') {
      const id = (e as CustomEvent<{ value: string }>).detail?.value
              ?? (target as unknown as BSelect).value;
      if (id && id !== this._currentId) this.preselect(id);
    } else if (target.id === 'concurrency') {
      const value = (e as CustomEvent<{ value: string }>).detail?.value
                 ?? (target as unknown as BSelect).value;
      if (value) this._concurrency = value;
    }
  };

  private _onClick = (e: Event) => {
    const target = e.target as HTMLElement;
    if (target.closest('#btn-run'))    { void this._run(); return; }
    if (target.closest('#btn-cancel')) { this._handle?.cancel(); this.update(); }
  };

  private async _run() {
    if (this._running) return;
    const ep = this._current();
    if (!ep) { toast.error(t('batch.validation.pickEndpoint')); return; }

    if (!hasCredentials()) {
      toast.warning(t('settings.toast.missingCreds'));
      window.dispatchEvent(new CustomEvent(OPEN_SETTINGS_EVENT));
      return;
    }

    const values = this.$<BTagInput>('#values')?.getTags() ?? [];
    if (values.length === 0) { toast.error(t('batch.validation.enterValues')); return; }

    const extras: Record<string, string> = {};
    for (const f of ep.extras ?? []) {
      const el = this.$<BInput | BSelect>(`#extra-${f.name}`);
      extras[f.name] = (el?.value ?? '').trim();
    }

    const concurrency = Math.max(1, Math.min(16, Number(this._concurrency)));

    this._rows = values.map(v => ({ state: 'pending', value: v }));
    this._total = values.length;
    this._done = 0;
    this._running = true;
    this.update();
    this._syncTable();

    this._handle = runBatch(
      values, concurrency,
      (value) => ep.run(value, extras),
      (i, status) => {
        this._rows[i] = status;
        if (status.state === 'ok' || status.state === 'fail') this._done++;
        this.update();
        this._syncTable();
      },
    );

    await this._handle.done;
    this._running = false;
    const ok = this._rows.filter(r => r.state === 'ok').length;
    const fail = this._rows.filter(r => r.state === 'fail').length;
    if (this._handle.cancelled) toast.info(t('batch.status.cancelled', { ok, fail, skipped: this._total - this._done }));
    else if (fail === 0)        toast.success(t('batch.status.complete', { ok }));
    else                        toast.warning(t('batch.status.completePartial', { ok, fail }));
    this.update();
    this._syncTable();
  }
}

function statusTag(r: RowStatus): string {
  switch (r.state) {
    case 'pending': return `<b-tag size="sm">${escapeHtml('…')}</b-tag>`;
    case 'running': return `<b-tag size="sm" color="var(--b-color-primary)">running</b-tag>`;
    case 'ok':      return `<b-tag size="sm" color="var(--b-color-success, #16a34a)">&#10003; ${escapeHtml(String(r.status ?? ''))}</b-tag>`;
    case 'fail':    return `<b-tag size="sm" color="var(--b-color-danger, #dc2626)">&#10005; ${escapeHtml(String(r.status ?? ''))}</b-tag>`;
  }
}

function escapeHtml(s: string): string {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

/** Render either a `b-select` (when the extra declares a `source`) or a plain
 *  `b-input`. The select is populated by `_populateExtras` after mount / on
 *  cache updates — `setOptions` preserves the user's existing selection. */
function renderExtraField(f: ExtraField): string {
  if (f.source) {
    return `<b-select id="extra-${escapeHtml(f.name)}" label="${escapeHtml(f.label)}"></b-select>`;
  }
  return `<b-input id="extra-${escapeHtml(f.name)}" label="${escapeHtml(f.label)}" placeholder="${escapeHtml(f.placeholder ?? '')}"></b-input>`;
}

void BATCH_ENDPOINTS;

define('batch-runner', BatchRunner);
