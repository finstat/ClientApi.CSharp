import { BaseComponent, define, diffJson, xmlToTree, type JsonDiffOptions, type JsonDiffResult } from 'birko-web-core';
import type { BSelect, BInput, BCheckbox, BJsonDiff, BTagInput, BAccordion, BTabs } from 'birko-web-components';
import { historyStore, type ResponseHistoryItem } from './response-history.js';
import { rerunHistoryItem } from './rerun.js';
import { runBatch, type BatchHandle } from '../pages/batch/runner.js';
import { PAGE_STYLES } from '../pages/page-styles.js';
import { failTypeLabel } from '../pages/page-helpers.js';
import { t, onLocaleChange } from '../i18n/index.js';
import { settingsStore, activeProfile, findProfile, type EnvProfile } from '../settings/settings-store.js';
import type { CallResult } from '../api/api-client.js';

type Format = 'json' | 'xml';

/** One call of a comparison: the profile it went to and what came back. */
interface Cell {
  profile: EnvProfile;
  result: CallResult;
  /** Against the baseline cell of the same run; null for the baseline and when a call failed. */
  diff: JsonDiffResult | null;
}

/** The request for one IČO (or the recorded request as is), sent to the baseline and every compared profile. */
interface Run {
  /** The IČO sent; null when the request has no IČO parameter. */
  value: string | null;
  /** Index 0 is the baseline; undefined while the run is pending. */
  cells: Cell[] | undefined;
}

const CONCURRENCY_OPTIONS = [1, 2, 4, 8].map(n => ({ value: String(n), label: String(n) }));

/**
 * `/compare/:id` — sends the request recorded in a history entry to a baseline profile and to any
 * number of other profiles, for one IČO or a list of them, and compares each body with the baseline's.
 */
export class ComparePage extends BaseComponent {
  private _unsubs: Array<() => void> = [];
  private _runs: Run[] = [];
  /** Shown diff: run index and column (1… = compared profile); -1 = none. */
  private _selected = { run: -1, col: -1 };
  /** Format of the calls that produced _runs; the bodies are parsed by it. */
  private _format: Format = 'json';
  private _handle: BatchHandle | null = null;
  /** Rows of the "fields that differ" tab; 0 hides the tab. */
  private _fieldCount = 0;

  static get styles() {
    return PAGE_STYLES + `
      .title { display: flex; align-items: center; gap: var(--b-space-sm, 0.5rem); margin: 0 0 var(--b-space-md, 1rem); }
      .title h2 { margin: 0; }
      .info { cursor: help; color: var(--b-text-secondary); }
      .side { display: flex; gap: var(--b-space-sm, 0.5rem); align-items: flex-end; max-width: 36rem; margin-bottom: var(--b-space-sm, 0.5rem); }
      .side b-select { flex: 1 1 auto; }
      .side b-input { flex: 0 0 7rem; }
      .values { max-width: 48rem; margin-bottom: var(--b-space-sm, 0.5rem); }
      h3 { margin: var(--b-space-md, 1rem) 0 var(--b-space-sm, 0.5rem); font-size: var(--b-text-base, 0.875rem); color: var(--b-text-secondary); }
      table { border-collapse: collapse; margin: var(--b-space-md, 1rem) 0; }
      th, td { text-align: left; padding: var(--b-space-xs, 0.25rem) var(--b-space-md, 0.75rem); border-bottom: 1px solid var(--b-border); vertical-align: top; }
      td.selected { background: var(--b-bg-tertiary); }
      /* an error message can be long (FinStat quotes the server's answer); wrap it instead of widening the page */
      .fail { color: var(--b-color-danger, #dc2626); max-width: 24rem; overflow-wrap: anywhere; font-size: var(--b-text-sm, 0.8125rem); }
      .ok    { color: var(--b-color-success, #16a34a); }
      .equal { color: var(--b-text-secondary); }
      .add-row { margin: 0 0 var(--b-space-sm, 0.5rem); }
      .progress { display: flex; align-items: center; gap: var(--b-space-sm, 0.5rem); margin: var(--b-space-md, 1rem) 0; }
      .progress b-progress { flex: 1 1 auto; max-width: 30rem; }
      .diff-title { margin-top: 0; }
      .caption { color: var(--b-text-secondary); margin: 0; }
    `;
  }

  private get _item(): ResponseHistoryItem | null {
    const id = window.location.hash.match(/^#\/compare\/(.+)$/)?.[1];
    return id ? historyStore.get('items').find(i => i.id === id) ?? null : null;
  }

  render() {
    const item = this._item;
    if (!item) return `<h2>${t('history.button.compare')}</h2><b-empty icon="&#128194;" message="${escapeHtml(t('compare.notFound'))}"></b-empty>`;
    const icoKey = icoParam(item);
    return `
      <div class="title">
        <h2>${escapeHtml(t('compare.title', { label: item.label }))}</h2>
        <b-tooltip position="bottom" text="${escapeHtml(`${t('compare.formatHint')} ${t('compare.cost')}`)}"><span class="info" aria-hidden="true">&#9432;</span></b-tooltip>
      </div>
      <b-accordion id="setup" multiple>
        <div slot="setup">
          <h3>${t('compare.baseline')}</h3>
          ${sideMarkup('base')}
          <h3>${t('compare.compareWith')}</h3>
          <div id="rows"></div>
          <div class="add-row"><b-button variant="ghost" size="sm" id="btn-add">${t('compare.addProfile')}</b-button></div>
          ${icoKey ? `
            <div class="values">
              <b-tag-input id="values" label="${escapeHtml(t('compare.values'))}"
                hint="${escapeHtml(t('compare.valuesHint'))}"></b-tag-input>
            </div>` : ''}
          <div class="side">
            <b-select id="format" label="${escapeHtml(t('settings.field.responseType'))}"></b-select>
            ${icoKey ? `<b-select id="concurrency" label="${escapeHtml(t('batch.field.concurrency'))}"></b-select>` : ''}
          </div>
          <b-checkbox id="hide-hosts" checked label="${escapeHtml(t('compare.hideHosts'))}"
            hint="${escapeHtml(t('compare.hideHostsHint', { sk: '{sk}', cz: '{cz}' }))}"></b-checkbox>
          <div class="buttons"><b-button variant="primary" id="btn-run"></b-button></div>
        </div>
      </b-accordion>
      <div id="progress"></div>
      <div id="results"></div>
      <b-tabs id="detail" style="display: none">
        <div slot="diff">
          <h3 class="diff-title" id="diff-title"></h3>
          <b-json-diff id="diff" style="display: none"></b-json-diff>
        </div>
        <div slot="fields" id="fields"></div>
      </b-tabs>
    `;
  }

  protected onMount() {
    const item = this._item;
    if (!item) return;
    this._setupAccordion(true);
    const profiles = settingsStore.get('profiles');
    this._fillSide('base', profileOfItem(item, profiles));
    this._addRow(this._nextProfile());

    const format = this.$<BSelect>('#format');
    format?.setOptions([{ value: 'json', label: 'JSON' }, { value: 'xml', label: 'XML' }]);
    // the format the request was made in, so a comparison sees what that call saw
    if (format) format.value = (item.headers?.['X-Finstat-ResponseType'] ?? settingsStore.get('responseType')) === 'json' ? 'json' : 'xml';
    const concurrency = this.$<BSelect>('#concurrency');
    concurrency?.setOptions(CONCURRENCY_OPTIONS);
    if (concurrency) concurrency.value = '2';
    const icoKey = icoParam(item);
    if (icoKey) this.$<BTagInput>('#values')?.setTags([String((item.params as Record<string, unknown>)[icoKey])]);

    // a picked profile brings its own port; the port can still be changed for this comparison
    this.shadowRoot?.addEventListener('change', (e: Event) => {
      const select = e.target as BSelect;
      const side = select.id?.startsWith('profile-') ? select.id.slice('profile-'.length) : null;
      if (side) this._setPort(side, findProfile(settingsStore.get('profiles'), select.value));
      this._updateRunLabel();
    });
    this.$('#rows')?.addEventListener('click', (e: Event) => {
      const key = (e.target as HTMLElement).closest<HTMLElement>('[data-remove]')?.dataset.remove;
      if (key) { this.$(`#row-${key}`)?.remove(); this._updateRunLabel(); }
    });
    this.$('#results')?.addEventListener('click', (e: Event) => {
      const cell = (e.target as HTMLElement).closest<HTMLElement>('[data-run]');
      if (!cell) return;
      this._show(Number(cell.dataset.run), Number(cell.dataset.col));
      this.$<BTabs>('#detail')?.setAttribute('active', 'diff');
    });
    this.$('#progress')?.addEventListener('click', (e: Event) => {
      if ((e.target as HTMLElement).closest('#btn-stop')) this._handle?.cancel();
    });
    this.$('#btn-add')?.addEventListener('click', () => { this._addRow(this._nextProfile()); this._updateRunLabel(); });
    this.$('#btn-run')?.addEventListener('click', () => this._run(item));
    this.$<BCheckbox>('#hide-hosts')?.addEventListener('change', () => this._recompare());
    this._updateRunLabel();

    this._unsubs.push(onLocaleChange(() => this._relocalize()));
  }

  protected onUnmount() {
    this._handle?.cancel();
    this._unsubs.forEach(u => u());
    this._unsubs = [];
  }

  private _setupAccordion(open: boolean) {
    this.$<BAccordion>('#setup')?.setItems([{ id: 'setup', header: t('compare.setup'), open }]);
  }

  /** update() morphs back to render(), which has no rows: keep the picked sides and add the rows again. */
  private _relocalize() {
    const base = this._side('base');
    const rows = this._rowKeys().map(key => this._side(key));
    const open = this.$<BAccordion>('#setup')?.getOpen().includes('setup') ?? true;
    this.update();
    this._setupAccordion(open);
    this._fillSide('base', base);
    for (const row of rows) this._addRow(row);
    this._updateRunLabel();
    this._renderResults();
    this._setDetailTabs();
  }

  /** Diff tab, plus the fields tab once a list of IČOs found differences; hidden before the first result. */
  private _setDetailTabs(active?: string) {
    const tabs = this.$<BTabs>('#detail');
    if (!tabs) return;
    const shown = this._runs.some(r => r.cells);
    tabs.style.display = shown ? '' : 'none';
    if (!shown) return;
    const current = active ?? tabs.getAttribute('active') ?? 'diff';
    tabs.setTabs(
      [
        { id: 'diff', label: t('compare.tab.diff') },
        ...(this._fieldCount > 0 ? [{ id: 'fields', label: t('compare.tab.fields', { count: this._fieldCount }) }] : []),
      ],
      current === 'fields' && this._fieldCount > 0 ? 'fields' : 'diff',
    );
  }

  /** The first profile not picked on any side yet; the active one when every profile is already in use. */
  private _nextProfile(): EnvProfile {
    const used = new Set(['base', ...this._rowKeys()].map(side => this.$<BSelect>(`#profile-${side}`)?.value));
    return settingsStore.get('profiles').find(p => !used.has(p.id)) ?? activeProfile();
  }

  private _rowKeys(): string[] {
    return [...(this.$('#rows')?.querySelectorAll<HTMLElement>('[data-side]') ?? [])].map(r => r.dataset.side!);
  }

  private _addRow(profile: EnvProfile) {
    const key = crypto.randomUUID().slice(0, 8);
    const row = document.createElement('div');
    row.id = `row-${key}`;
    row.dataset.side = key;
    row.innerHTML = sideMarkup(key, `<b-button variant="ghost" size="sm" data-remove="${key}">${t('compare.remove')}</b-button>`);
    this.$('#rows')?.appendChild(row);
    // the new elements upgrade synchronously once attached, so they can be filled right away
    this._fillSide(key, profile);
  }

  private _fillSide(side: string, profile: EnvProfile) {
    const select = this.$<BSelect>(`#profile-${side}`);
    select?.setOptions(settingsStore.get('profiles').map(p => ({ value: p.id, label: p.name })));
    if (select) select.value = profile.id;
    this._setPort(side, profile);
  }

  private _setPort(side: string, profile: EnvProfile) {
    const port = this.$<BInput>(`#port-${side}`);
    if (port) port.value = profile.port ? String(profile.port) : '';
  }

  /** The profile picked on one side, with the port typed next to it (only for this comparison). */
  private _side(side: string): EnvProfile {
    const profile = findProfile(settingsStore.get('profiles'), this.$<BSelect>(`#profile-${side}`)?.value ?? '');
    const port = Number(this.$<BInput>(`#port-${side}`)?.value?.trim() || NaN);
    return { ...profile, port: Number.isInteger(port) && port >= 1 && port <= 65535 ? port : null };
  }

  private _values(item: ResponseHistoryItem): (string | null)[] {
    if (!icoParam(item)) return [null];
    const tags = this.$<BTagInput>('#values')?.getTags() ?? [];
    return tags.length > 0 ? tags : [null];
  }

  /** The button says how many calls a comparison makes: each IČO goes to every profile. */
  private _updateRunLabel() {
    const item = this._item;
    const button = this.$<HTMLElement>('#btn-run');
    if (!item || !button) return;
    const calls = this._values(item).length * (1 + this._rowKeys().length);
    button.textContent = t('compare.runCalls', { calls });
  }

  private async _run(item: ResponseHistoryItem) {
    if (this._handle && !this._handle.cancelled) return;
    const profiles = [this._side('base'), ...this._rowKeys().map(key => this._side(key))];
    const format: Format = this.$<BSelect>('#format')?.value === 'xml' ? 'xml' : 'json';
    const values = this._values(item);
    const icoKey = icoParam(item);
    // one IČO is a single comparison and goes to history like any call; a list would flood it
    const record = values.length === 1;
    const concurrency = Math.max(1, Number(this.$<BSelect>('#concurrency')?.value ?? 2));

    this._format = format;
    this._runs = values.map(value => ({ value, cells: undefined }));
    this._selected = { run: -1, col: -1 };
    this._fieldCount = 0;
    this._renderFields();
    this._setupAccordion(false);
    this._renderResults();
    this._renderProgress(0, values.length, true);

    let done = 0;
    this._handle = runBatch(values.map(v => v ?? ''), concurrency, async (_v, i) => {
      const value = values[i];
      const request = value !== null && icoKey ? { ...item, params: { ...(item.params as Record<string, unknown>), [icoKey]: value } } : item;
      const results = await Promise.all(profiles.map(profile => rerunHistoryItem(request, {
        profile, responseType: format, record,
        label: `${t('history.button.compare')}: ${item.label} (${profileLabel(profile)})`,
      })));
      this._runs[i].cells = profiles.map((profile, c) => ({ profile, result: results[c], diff: null }));
      this._diffRun(this._runs[i]);
      return results[0];
    }, (_i, status) => {
      if (status.state !== 'ok' && status.state !== 'fail') return;
      done++;
      this._renderProgress(done, values.length, true);
      this._renderResults();
      this._setDetailTabs();
      if (this._selected.run < 0) this._showFirst();
    });
    await this._handle.done;
    this._renderProgress(done, values.length, false);
    this._renderFields();
    this._handle = null;
  }

  /** Diff every compared profile of one run against its baseline. A failed call is not compared. */
  private _diffRun(run: Run) {
    if (!run.cells) return;
    const [base, ...others] = run.cells;
    const baseBody = base.result.ok ? parseBody(base.result.rawText, this._format) : undefined;
    for (const o of others) {
      o.diff = base.result.ok && o.result.ok
        ? diffJson(baseBody, parseBody(o.result.rawText, this._format), this._options(base.profile, o.profile))
        : null;
    }
  }

  private _recompare() {
    for (const run of this._runs) this._diffRun(run);
    this._renderResults();
    this._renderFields();
    const { run, col } = this._selected;
    if (this._runs[run]?.cells?.[col]?.diff) this._show(run, col);
    else this._showFirst();
  }

  private _options(a: EnvProfile, b: EnvProfile): JsonDiffOptions {
    const hide = this.$<BCheckbox>('#hide-hosts')?.checked ?? true;
    return hide ? { normalize: [{ path: '**', normalize: hostNormalizer([...settingsStore.get('profiles'), a, b]) }] } : {};
  }

  private _renderProgress(done: number, total: number, running: boolean) {
    const host = this.$<HTMLElement>('#progress');
    if (!host) return;
    host.innerHTML = total > 1 || running
      ? `<div class="progress">
          <b-progress value="${done}" max="${total}"></b-progress>
          <span>${escapeHtml(t('compare.progress', { done, total }))}</span>
          ${running ? `<b-button variant="ghost" size="sm" id="btn-stop">${t('compare.stop')}</b-button>` : ''}
        </div>`
      : '';
  }

  /** One row per IČO, one column per profile; a compared cell shows its result and opens its diff. */
  private _renderResults() {
    const host = this.$<HTMLElement>('#results');
    const first = this._runs.find(r => r.cells)?.cells;
    if (!host || !first) { if (host) host.innerHTML = ''; return; }
    const head = first.map((c, i) => `<th>${escapeHtml(profileLabel(c.profile))}${i === 0 ? ` <span class="equal">(${t('compare.baseline')})</span>` : ''}</th>`).join('');
    const showValue = this._runs.some(r => r.value !== null);
    const rows = this._runs.map((run, r) => {
      const cells = run.cells
        ? run.cells.map((c, col) => this._cellMarkup(c, r, col)).join('')
        : `<td colspan="${first.length}" class="equal">…</td>`;
      return `<tr>${showValue ? `<td>${escapeHtml(run.value ?? '')}</td>` : ''}${cells}</tr>`;
    }).join('');
    host.innerHTML = `
      <table>
        <thead><tr>${showValue ? `<th>${t('compare.values')}</th>` : ''}${head}</tr></thead>
        <tbody>${rows}</tbody>
      </table>`;
  }

  private _cellMarkup(c: Cell, run: number, col: number): string {
    if (!c.result.ok) return `<td class="fail">${escapeHtml(callError(c.result))}</td>`;
    if (col === 0) return `<td class="ok">${t('history.meta.ok')}</td>`;
    if (!c.diff) return `<td class="equal">${t('compare.notCompared')}</td>`;
    const selected = this._selected.run === run && this._selected.col === col;
    const text = c.diff.equal ? `<span class="equal">${t('compare.equal')}</span>` : escapeHtml(t('compare.summary', { ...c.diff.summary }));
    const button = `<b-button variant="${selected ? 'secondary' : 'ghost'}" size="sm" data-run="${run}" data-col="${col}">${t('compare.showDiff')}</b-button>`;
    return `<td class="${selected ? 'selected' : ''}">${text} ${button}</td>`;
  }

  /**
   * Which fields differ, and in how many IČOs, per compared profile. List positions are folded
   * into [*] so "the third contact differs" and "the fifth contact differs" count as one field.
   */
  private _renderFields() {
    const host = this.$<HTMLElement>('#fields');
    if (!host) return;
    const runs = this._runs.filter(r => r.cells);
    if (runs.length < 2) {
      host.innerHTML = '';
      this._fieldCount = 0;
      this._setDetailTabs();
      return;
    }
    const counts = new Map<string, { profile: string; path: string; values: number }>();
    for (const run of runs) {
      for (const c of run.cells!.slice(1)) {
        if (!c.diff) continue;
        const paths = new Set(c.diff.changes.map(ch => ch.path.replace(/\[\d+\]/g, '[*]')));
        for (const path of paths) {
          const key = `${profileLabel(c.profile)}\u0000${path}`;
          const entry = counts.get(key) ?? { profile: profileLabel(c.profile), path, values: 0 };
          entry.values++;
          counts.set(key, entry);
        }
      }
    }
    const sorted = [...counts.values()].sort((a, b) => b.values - a.values || a.path.localeCompare(b.path));
    this._fieldCount = sorted.length;
    this._setDetailTabs();
    host.innerHTML = sorted.length === 0 ? '' : `
      <p class="caption">${escapeHtml(t('compare.fields', { total: runs.length }))}</p>
      <table>
        <thead><tr><th>${t('compare.col.profile')}</th><th>${t('compare.col.field')}</th><th>${t('compare.col.values')}</th></tr></thead>
        <tbody>${sorted.slice(0, 50).map(f => `<tr><td>${escapeHtml(f.profile)}</td><td><code>${escapeHtml(f.path || '/')}</code></td><td>${f.values}</td></tr>`).join('')}</tbody>
      </table>`;
  }

  /** The first compared cell that differs, else the first compared one. */
  private _showFirst() {
    let fallback: [number, number] | null = null;
    for (let r = 0; r < this._runs.length; r++) {
      const cells = this._runs[r].cells ?? [];
      for (let c = 1; c < cells.length; c++) {
        if (!cells[c].diff) continue;
        if (!cells[c].diff!.equal) { this._show(r, c); return; }
        fallback ??= [r, c];
      }
    }
    if (fallback) this._show(...fallback);
    else this._show(-1, -1);
  }

  /** Show the diff of one cell against the baseline of its run; -1 hides the diff. */
  private _show(run: number, col: number) {
    const cells = this._runs[run]?.cells;
    const base = cells?.[0];
    const other = cells?.[col];
    const diff = this.$<BJsonDiff>('#diff');
    const title = this.$<HTMLElement>('#diff-title');
    if (!diff || !title) return;
    this._selected = { run, col };
    if (!base || !other?.diff) {
      diff.style.display = 'none';
      title.textContent = '';
      this._renderResults();
      return;
    }
    const value = this._runs[run].value;
    diff.style.display = '';
    title.textContent = `${value ? `${value}: ` : ''}${t('compare.diffTitle', { a: profileLabel(base.profile), b: profileLabel(other.profile) })}`;
    diff.setAttribute('label-a', profileLabel(base.profile));
    diff.setAttribute('label-b', profileLabel(other.profile));
    // raw bodies: in XML mode b-json-diff diffs the converted tree and shows the original XML side by side
    diff.setAttribute('format', this._format);
    diff.setOptions(this._options(base.profile, other.profile));
    diff.setData(base.result.rawText, other.result.rawText);
    this._renderResults();
  }
}

function sideMarkup(side: string, extra = ''): string {
  return `
    <div class="side">
      <b-select id="profile-${side}" label="${escapeHtml(t('settings.field.profile'))}"></b-select>
      <b-input id="port-${side}" type="number" min="1" max="65535" placeholder="443" label="${escapeHtml(t('settings.field.port'))}"></b-input>
      ${extra}
    </div>`;
}

/** The IČO parameter of a recorded POST request, whatever its case; bulk comparison swaps its value. */
function icoParam(item: ResponseHistoryItem): string | null {
  if (item.method !== 'POST' || !item.params || typeof item.params !== 'object') return null;
  return Object.keys(item.params).find(k => k.toLowerCase() === 'ico') ?? null;
}

/** History records the profile by name (plus ":port" when one was set); older entries have none. */
function profileOfItem(item: ResponseHistoryItem, profiles: readonly EnvProfile[]): EnvProfile {
  if (!item.profile) return activeProfile();
  const [name, port] = item.profile.split(/:(?=\d+$)/);
  const profile = profiles.find(p => p.name === name) ?? activeProfile();
  return port ? { ...profile, port: Number(port) } : profile;
}

function profileLabel(p: EnvProfile): string {
  return p.port ? `${p.name}:${p.port}` : p.name;
}

/**
 * The error of a failed call, short enough for a table cell. When the server answered with a web page
 * instead of the API (FinStat quotes it after "Server response:"), the page is replaced by a note:
 * the endpoint usually does not exist on that profile yet.
 */
function callError(r: CallResult): string {
  const failType = failTypeLabel(r.failType);
  let error = r.error ?? t('common.httpStatus', { status: r.status });
  const [message, quoted] = error.split(/\s*Server response:\s*/, 2);
  if (quoted !== undefined) {
    error = /<(!doctype|html)/i.test(quoted) ? `${message} ${t('compare.htmlAnswer')}` : message;
  }
  if (error.length > 300) error = `${error.slice(0, 300)}…`;
  return failType ? `${failType} — ${error}` : error;
}

/** A body that does not parse (an HTML error page) is compared as one text value. */
function parseBody(text: string, format: Format): unknown {
  if (format === 'xml') return xmlToTree(text) ?? text;
  try { return JSON.parse(text); } catch { return text; }
}

/**
 * Replaces the hosts of every profile (with or without www., with any port) by {sk} / {cz}: a dev server
 * links to the production domain (zdrojakcz.eu answers with www.finstat.cz links), so the hosts of the two
 * compared profiles alone are not enough. Longest first, so cz.finstat.sk is not half-replaced as cz.{sk}.
 */
function hostNormalizer(profiles: EnvProfile[]): (value: unknown) => unknown {
  const hosts = new Map<string, string>();
  for (const p of profiles) {
    hosts.set(bareHost(p.skHost), '{sk}');
    hosts.set(bareHost(p.czHost), '{cz}');
  }
  const names = [...hosts.keys()].sort((x, y) => y.length - x.length);
  const pattern = new RegExp(`(?:www\\.)?(${names.map(escapeRegExp).join('|')})(?::\\d+)?`, 'gi');
  return value => typeof value === 'string'
    ? value.replace(pattern, (_m, host: string) => hosts.get(host.toLowerCase()) ?? host)
    : value;
}

function bareHost(host: string): string {
  return host.toLowerCase().replace(/^www\./, '');
}

function escapeRegExp(s: string): string {
  return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function escapeHtml(s: string): string {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

define('compare-page', ComparePage);
