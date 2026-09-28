import { BaseComponent, define } from 'birko-web-core';
import type { BDrawer } from 'birko-web-components';

import { BatchRunner } from './batch-runner.js';
import './batch-runner.js';
import { t, onLocaleChange } from '../../i18n/index.js';

/** Event dispatched on window to open the batch drawer, optionally suggesting an endpoint id. */
export const OPEN_BATCH_EVENT = 'finstat:open-batch';

export interface OpenBatchDetail { suggestedEndpointId?: string | null; }

export class BatchDrawer extends BaseComponent {
  private _windowListener?: (e: Event) => void;

  static get styles() {
    return `:host { display: contents; }`;
  }

  render() {
    return `
      <b-drawer id="drawer" title="${t('batch.title')}" size="lg" modal>
        <batch-runner id="runner"></batch-runner>
      </b-drawer>
    `;
  }

  private _unsubLocale?: () => void;

  protected onMount() {
    this._windowListener = (e: Event) => {
      const detail = (e as CustomEvent<OpenBatchDetail>).detail;
      const id = detail?.suggestedEndpointId ?? null;
      this.$<BatchRunner>('#runner')?.preselect(id);
      this.$<BDrawer>('#drawer')?.open();
      this.$<BatchRunner>('#runner')?.focusValues();
    };
    window.addEventListener(OPEN_BATCH_EVENT, this._windowListener);
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    if (this._windowListener) window.removeEventListener(OPEN_BATCH_EVENT, this._windowListener);
    this._unsubLocale?.();
  }
}

define('batch-drawer', BatchDrawer);

// Self-mount a single instance into the body so the drawer overlay covers the viewport.
if (typeof document !== 'undefined' && !document.querySelector('batch-drawer')) {
  const mount = () => document.body.appendChild(document.createElement('batch-drawer'));
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount, { once: true });
  } else {
    mount();
  }
}
