import { BaseComponent, define } from 'birko-web-core';

import { resolveBatchFromHash } from './registry.js';
import { OPEN_BATCH_EVENT, type OpenBatchDetail } from './batch-drawer.js';
import { t, onLocaleChange } from '../../i18n/index.js';

/**
 * Floating action button anchored to the viewport bottom-right. Clicking it
 * opens the batch drawer with the endpoint matching the current hash pre-selected.
 */
export class BatchFab extends BaseComponent {
  static get styles() {
    return `
      :host {
        position: fixed;
        right: var(--b-space-lg, 1rem);
        bottom: calc(1.75rem + var(--b-space-lg, 1rem));
        z-index: var(--b-z-sticky, 200);
      }
      button {
        width: 3.25rem; height: 3.25rem;
        border-radius: var(--b-radius-full, 9999px);
        border: none; cursor: pointer;
        background: var(--b-color-primary);
        color: var(--b-text-inverse, #fff);
        font-size: 1.5rem; line-height: 1;
        display: flex; align-items: center; justify-content: center;
        box-shadow: var(--b-shadow-lg, 0 10px 15px -3px rgba(0,0,0,.2));
        transition: transform 150ms ease, box-shadow 150ms ease;
      }
      button:hover {
        transform: translateY(-2px);
        box-shadow: var(--b-shadow-xl, 0 20px 25px -5px rgba(0,0,0,.25));
      }
      button:focus-visible { outline: none; box-shadow: var(--b-focus-ring); }
      button::after {
        content: attr(data-tooltip);
        position: absolute; right: 120%; top: 50%;
        transform: translateY(-50%);
        background: var(--b-text); color: var(--b-bg);
        padding: 0.2rem 0.5rem; border-radius: var(--b-radius);
        font-size: var(--b-text-xs, 0.6875rem); white-space: nowrap;
        opacity: 0; pointer-events: none;
        transition: opacity 150ms ease;
      }
      button:hover::after, button:focus-visible::after { opacity: 1; }
    `;
  }

  render() {
    return `<button type="button" aria-label="${t('batch.openTooltip')}" data-tooltip="${t('batch.tooltipHint', { hint: this._shortHint() })}">&#9889;</button>`;
  }

  private _unsubLocale?: () => void;

  protected onMount() {
    this.$('button')?.addEventListener('click', () => this._open());
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
  }

  private _onHashChange = () => this.update();

  private _shortHint(): string {
    const id = resolveBatchFromHash(window.location.hash);
    return id ? t('batch.preFilled', { id }) : t('batch.pickEndpoint');
  }

  private _open() {
    const detail: OpenBatchDetail = { suggestedEndpointId: resolveBatchFromHash(window.location.hash) };
    window.dispatchEvent(new CustomEvent(OPEN_BATCH_EVENT, { detail }));
  }
}

define('batch-fab', BatchFab);

// Self-mount into body so it floats over any route.
if (typeof document !== 'undefined' && !document.querySelector('batch-fab')) {
  const mount = () => document.body.appendChild(document.createElement('batch-fab'));
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount, { once: true });
  } else {
    mount();
  }
}
