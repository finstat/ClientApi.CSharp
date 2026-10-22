import { BaseComponent, define } from 'birko-web-core';
import type { BTabs, BObjectTree } from 'birko-web-components';
import { toast } from 'birko-web-components';

import {
  historyStore,
  selectHistory,
  removeHistory,
  clearHistory,
  type ResponseHistoryItem,
} from './response-history.js';
import { rerunHistoryItem } from './rerun.js';
import { openCurlModal } from './curl-modal.js';
import { LOADING_END_EVENT, LOADING_START_EVENT } from '../shell/loading-overlay.js';
import { setPrefill } from '../pages/prefill.js';
import { failTypeLabel } from '../pages/page-helpers.js';
import { t, onLocaleChange } from '../i18n/index.js';

function formatTime(ts: number): string {
  return new Date(ts).toLocaleTimeString([], { hour12: false });
}

function prettyJson(value: unknown): string {
  try { return JSON.stringify(value, null, 2); } catch { return String(value); }
}

/** FinStat's own JSON body when it answered in JSON, else the parsed result. */
function rawJsonText(item: ResponseHistoryItem): string {
  if (item.contentType.includes('json') && item.responseText) {
    try { return prettyJson(JSON.parse(item.responseText)); } catch { return item.responseText; }
  }
  return prettyJson(item.responseBody);
}

/** `/history/:id` page — reads the id from the hash and renders the selected entry. */
export class ResponseViewer extends BaseComponent {
  private _unsubs: Array<() => void> = [];
  /** Id currently being rendered — set before the deferred render, checked inside
   *  the animation-frame callback so a fast second click doesn't overwrite a newer
   *  selection with stale content. */
  private _pendingItemId: string | null = null;

  static get styles() {
    return `
      :host { display: block; padding: var(--b-space-xl, 2rem); color: var(--b-text); }
      .toolbar {
        display: flex; justify-content: space-between; align-items: center;
        gap: var(--b-space-sm, 0.5rem); margin-bottom: var(--b-space-md, 1rem);
      }
      .toolbar h2 { margin: 0; }
      b-card { margin-bottom: var(--b-space-md, 1rem); }
      b-definition-list code { background: var(--b-bg-tertiary); padding: 0.05rem 0.3rem; border-radius: 0.25rem; }
      .status-ok   { color: var(--b-color-success, #16a34a); font-weight: 500; }
      .status-fail { color: var(--b-color-danger,  #dc2626); font-weight: 500; }
      .fail-badge {
        display: inline-block; margin-left: 0.5rem;
        padding: 0.05rem 0.45rem;
        border-radius: 0.75rem;
        background: var(--b-color-danger-bg, #fee2e2);
        color: var(--b-color-danger, #dc2626);
        font-size: 0.85em; font-weight: 500;
      }
      b-code-block, b-json-viewer, b-xml-viewer { display: block; max-height: 60vh; overflow: auto; }
      /* b-object-tree with show-header paints its own frame + scrolls its body via max-height attr. */
      .loading-shell {
        display: flex; align-items: center; gap: var(--b-space-sm, 0.5rem);
        padding: var(--b-space-xl, 2rem);
        color: var(--b-text-secondary);
        justify-content: center;
      }
    `;
  }

  render() { return `<div id="host"></div>`; }

  protected onMount() {
    this._unsubs.push(
      historyStore.on('items',      () => this._refresh()),
      historyStore.on('selectedId', () => this._refresh()),
      onLocaleChange(() => this._refresh()),
    );
    window.addEventListener('hashchange', this._onHashChange);
    this._syncFromHash();
  }

  protected onUnmount() {
    this._unsubs.forEach(u => u());
    this._unsubs = [];
    window.removeEventListener('hashchange', this._onHashChange);
  }

  private _onHashChange = () => this._syncFromHash();

  private _syncFromHash(): void {
    const m = window.location.hash.match(/^#\/history\/(.+)$/);
    const id = m?.[1] ?? null;
    if (id && historyStore.get('selectedId') !== id) selectHistory(id);
    else this._refresh();
  }

  private _refresh(): void {
    const host = this.$<HTMLElement>('#host');
    if (!host) return;
    const id = historyStore.get('selectedId');
    const item = id ? historyStore.get('items').find(i => i.id === id) ?? null : null;

    if (!item) {
      this._pendingItemId = null;
      host.innerHTML = this._renderEmpty();
      this._wireButtons(null);
      return;
    }

    // Show the global loading overlay (same pill used for API calls) while we
    // defer the heavy tree / JSON / XML render to the next frame. Double rAF is
    // intentional: rAF callbacks run *before* the upcoming paint, so a single
    // rAF would replace the host's contents before the overlay ever renders.
    // The second rAF gives the browser one paint cycle in between.
    this._pendingItemId = item.id;
    host.innerHTML = this._renderLoadingShell(item);

    const loadingId = crypto.randomUUID();
    window.dispatchEvent(new CustomEvent(LOADING_START_EVENT, {
      detail: { id: loadingId, label: t('history.loadingLabel', { label: item.label }) },
    }));

    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        try {
          if (this._pendingItemId !== item.id) return;
          const host2 = this.$<HTMLElement>('#host');
          if (!host2) return;
          host2.innerHTML = this._renderItem(item);
          this._wireButtons(item);
          this.$<BObjectTree>('#obj-tree')?.setData(item.responseBody);
          this._wireTabs(item);
        } finally {
          window.dispatchEvent(new CustomEvent(LOADING_END_EVENT, { detail: { id: loadingId } }));
        }
      });
    });
  }

  private _renderLoadingShell(item: ResponseHistoryItem): string {
    return `
      <div class="toolbar"><h2>${escapeHtml(item.label)}</h2></div>
      <div class="loading-shell">
        <b-spinner size="sm"></b-spinner>
        <span>${t('history.loading')}</span>
      </div>
    `;
  }

  private _renderEmpty(): string {
    const count = historyStore.get('items').length;
    const message = count === 0 ? t('history.empty.noCalls') : t('history.empty.pickEntry');
    return `
      <div class="toolbar"><h2>${t('history.title')}</h2></div>
      <b-empty icon="&#128194;" message="${escapeHtml(message)}"></b-empty>
    `;
  }

  private _renderItem(item: ResponseHistoryItem): string {
    const statusClass = item.ok ? 'status-ok' : 'status-fail';
    const paramsText  = prettyJson(item.params);
    const jsonText    = rawJsonText(item);
    const hasXml      = item.contentType.includes('xml') && !!item.responseText;
    const statusText  = item.ok ? t('history.meta.ok') : (item.error ?? t('history.meta.error'));
    const failBadge   = !item.ok && item.failType
      ? `<span class="fail-badge" title="${escapeHtml(item.failType)}">${escapeHtml(failTypeLabel(item.failType) ?? item.failType)}</span>`
      : '';
    const rawXmlEmpty = t('history.empty.rawXml', { contentType: item.contentType || 'missing' });

    return `
      <div class="toolbar">
        <h2>${escapeHtml(item.label)}</h2>
        <div>
          <b-button variant="primary" id="btn-open">${t('history.button.openAction')}</b-button>
          <b-button variant="primary" id="btn-rerun">${t('history.button.rerun')}</b-button>
          ${isDownload(item) ? '' : `<b-button variant="ghost" id="btn-compare">${t('history.button.compare')}</b-button>`}
          <b-button variant="ghost"   id="btn-curl">${t('history.button.curl')}</b-button>
          <b-button variant="ghost"   id="btn-remove">${t('history.button.remove')}</b-button>
          <b-button variant="danger"  id="btn-clear">${t('history.button.clearAll')}</b-button>
        </div>
      </div>
      <b-card>
        <b-definition-list layout="horizontal">
          <dt>${t('history.meta.endpoint')}</dt>    <dd><code>${escapeHtml(item.method)} ${escapeHtml(item.endpoint)}</code></dd>
          ${item.profile ? `<dt>${t('history.meta.profile')}</dt><dd>${escapeHtml(item.profile)}</dd>` : ''}
          <dt>${t('history.meta.status')}</dt>      <dd class="${statusClass}">${item.status} ${escapeHtml(statusText)}${failBadge}</dd>
          <dt>${t('history.meta.time')}</dt>        <dd>${formatTime(item.timestamp)} (${item.durationMs} ms)</dd>
          <dt>${t('history.meta.contentType')}</dt> <dd>${escapeHtml(item.contentType || '—')}</dd>
          ${renderHeaderRows(item.headers)}
        </b-definition-list>
      </b-card>
      <b-tabs id="tabs"></b-tabs>
      <div slot="request"><b-code-block language="json">${escapeHtml(paramsText)}</b-code-block></div>
      <div slot="tree"><b-object-tree id="obj-tree" expanded-depth="2" show-header header-title="${t('history.tab.tree')}" max-height="60vh"></b-object-tree></div>
      <div slot="json"><b-json-viewer expanded-depth="2">${escapeHtml(jsonText)}</b-json-viewer></div>
      <div slot="xml">${hasXml
        ? `<b-xml-viewer expanded-depth="2">${escapeHtml(item.responseText)}</b-xml-viewer>`
        : `<b-empty icon="&#128196;" message="${escapeHtml(rawXmlEmpty)}"></b-empty>`
      }</div>
    `;
  }

  private _wireTabs(_item: ResponseHistoryItem): void {
    // Move slotted children out of #host so <b-tabs> can find them as siblings.
    const host = this.$<HTMLElement>('#host');
    const tabs = this.$<BTabs>('#tabs');
    if (!host || !tabs) return;
    tabs.setTabs(
      [
        { id: 'request', label: t('history.tab.request') },
        { id: 'tree',    label: t('history.tab.tree') },
        { id: 'json',    label: t('history.tab.rawJson') },
        { id: 'xml',     label: t('history.tab.rawXml') },
      ],
      'tree',
    );
    // b-tabs uses light-DOM <slot name="<id>"> — move the slotted divs as children of the tab element.
    host.querySelectorAll('[slot]').forEach(el => tabs.appendChild(el));
  }

  private _wireButtons(item: ResponseHistoryItem | null): void {
    this.$('#btn-open')?.addEventListener('click', () => {
      if (!item) return;
      const route = endpointToRouteHash(item.endpoint);
      if (!route) { toast.info(t('history.toast.openActionNoRoute')); return; }
      const params = (item.params && typeof item.params === 'object')
        ? item.params as Record<string, unknown>
        : {};
      // the recorded params carry the sent `Lang` (Settings value included); only the per-call choice fills #lang
      setPrefill(route, item.lang ? { ...params, lang: item.lang } : params);
      window.location.hash = route;
    });
    this.$('#btn-rerun')?.addEventListener('click', async () => {
      if (!item) return;
      const res = await rerunHistoryItem(item);
      if (res.ok) toast.success(t('history.toast.rerunOk'));
      else {
        const failType = failTypeLabel(res.failType);
        const baseError = res.error ?? t('common.httpStatus', { status: res.status });
        const error = failType ? `${failType} — ${baseError}` : baseError;
        toast.error(t('history.toast.rerunFailed', { error }));
      }
      window.location.hash = `#/history/${res.historyId}`;
    });
    this.$('#btn-compare')?.addEventListener('click', () => {
      if (item) window.location.hash = `#/compare/${item.id}`;
    });
    this.$('#btn-curl')?.addEventListener('click', () => {
      if (!item) return;
      const path = item.endpoint.split('?')[0];
      openCurlModal(path, item.params, item.label, item.headers);
    });
    this.$('#btn-remove')?.addEventListener('click', () => {
      if (!item) return;
      removeHistory(item.id);
      toast.info(t('history.toast.entryRemoved'));
      window.location.hash = '#/history';
    });
    this.$('#btn-clear')?.addEventListener('click', () => {
      clearHistory();
      toast.info(t('history.toast.historyCleared'));
      window.location.hash = '#/history';
    });
  }
}

/** File downloads have no body to compare. */
function isDownload(item: ResponseHistoryItem): boolean {
  return item.endpoint.split('?')[0].endsWith('/download');
}

function escapeHtml(s: string): string {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

/**
 * Map a recorded backend endpoint path (e.g. `api/sk/monitoring/add?category=foo`)
 * to the front-end hash route used by the corresponding page (`#/sk/monitoring/add`).
 * Returns `null` for non-routable meta-endpoints like `api/_curl` or `api/settings/*`.
 */
function endpointToRouteHash(endpoint: string): string | null {
  if (!endpoint) return null;
  const path = endpoint.split('?')[0].replace(/^\/+/, '');
  if (!path.startsWith('api/')) return null;
  const rest = path.slice('api/'.length);
  // Meta endpoints that have no UI counterpart.
  if (rest.startsWith('_') || rest.startsWith('settings/')) return null;
  return `#/${rest}`;
}

/** Render one <dt>/<dd> pair per X-Finstat-* header recorded with this history entry.
 *  PrivateKey is masked to `•••` — displaying it would expose the secret any time
 *  someone shared a screenshot. Empty / missing headers are omitted. */
function renderHeaderRows(headers: Record<string, string> | undefined): string {
  if (!headers) return '';
  const rows: string[] = [];
  for (const [key, rawValue] of Object.entries(headers)) {
    if (!rawValue) continue;
    const label = escapeHtml(key);
    const value = /privatekey/i.test(key) ? '•••' : escapeHtml(rawValue);
    rows.push(`<dt>${label}</dt><dd><code>${value}</code></dd>`);
  }
  return rows.join('');
}

define('response-viewer', ResponseViewer);
