import { BaseComponent, define } from 'birko-web-core';
import { t } from '../i18n/index.js';

/** Dispatched on window to open / close the global loading overlay. Uses a counter
 *  so nested or parallel API calls don't prematurely hide the spinner. The start
 *  event may carry a `{ label, id }` pair; `id` lets `end` match the correct entry
 *  when multiple calls overlap. */
export const LOADING_START_EVENT = 'finstat:loading-start';
export const LOADING_END_EVENT   = 'finstat:loading-end';

export interface LoadingEventDetail {
  /** Human-readable description of what's loading (endpoint label). */
  label?: string;
  /** Unique id so end-events can pop the right entry — defaults to a generated id. */
  id?: string;
}

/**
 * Full-page overlay that shows a spinner while at least one `runApiCall` is in flight.
 * Mounts once to `document.body` at module load — the shell never sees it.
 */
export class LoadingOverlay extends BaseComponent {
  /** Stack of pending entries — the top entry's label is displayed. */
  private _pending: Array<{ id: string; label: string }> = [];

  static get styles() {
    return `
      :host {
        position: fixed; inset: 0;
        display: none;
        align-items: center; justify-content: center;
        background: rgba(0, 0, 0, 0.15);
        z-index: 9999;
        pointer-events: none;
      }
      :host([active]) { display: flex; }
      .pill {
        display: inline-flex; align-items: center; gap: var(--b-space-sm, 0.5rem);
        padding: var(--b-space-sm, 0.5rem) var(--b-space-lg, 1rem);
        background: var(--b-bg-elevated);
        border: 1px solid var(--b-border);
        border-radius: var(--b-radius-full, 9999px);
        box-shadow: var(--b-shadow-md, 0 4px 12px rgba(0,0,0,0.15));
        font-size: var(--b-text-sm, 0.8125rem);
        color: var(--b-text);
        max-width: calc(100vw - 2 * var(--b-space-xl, 2rem));
      }
      .label { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
      .count { color: var(--b-text-secondary); font-size: 0.9em; margin-left: var(--b-space-xs, 0.25rem); }
    `;
  }

  render() {
    return `<div class="pill"><b-spinner size="sm"></b-spinner><span class="label">${t('common.loading')}</span><span class="count"></span></div>`;
  }

  protected onMount() {
    window.addEventListener(LOADING_START_EVENT, this._onStart);
    window.addEventListener(LOADING_END_EVENT,   this._onEnd);
  }

  protected onUnmount() {
    window.removeEventListener(LOADING_START_EVENT, this._onStart);
    window.removeEventListener(LOADING_END_EVENT,   this._onEnd);
  }

  private _onStart = (e: Event) => {
    const d = (e as CustomEvent<LoadingEventDetail>).detail ?? {};
    this._pending.push({ id: d.id ?? Math.random().toString(36).slice(2), label: d.label ?? '' });
    this._sync();
  };

  private _onEnd = (e: Event) => {
    const id = (e as CustomEvent<LoadingEventDetail>).detail?.id;
    const idx = id ? this._pending.findIndex(p => p.id === id) : this._pending.length - 1;
    if (idx >= 0) this._pending.splice(idx, 1);
    this._sync();
  };

  private _sync() {
    if (this._pending.length === 0) { this.removeAttribute('active'); return; }
    this.setAttribute('active', '');
    const top = this._pending[this._pending.length - 1];
    const labelEl = this.shadowRoot?.querySelector<HTMLElement>('.label');
    const countEl = this.shadowRoot?.querySelector<HTMLElement>('.count');
    if (labelEl) labelEl.textContent = top.label || t('common.loading');
    if (countEl) countEl.textContent = this._pending.length > 1 ? `+${this._pending.length - 1}` : '';
  }
}

define('loading-overlay', LoadingOverlay);

if (typeof document !== 'undefined' && !document.querySelector('loading-overlay')) {
  const mount = () => document.body.appendChild(document.createElement('loading-overlay'));
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount, { once: true });
  } else {
    mount();
  }
}
