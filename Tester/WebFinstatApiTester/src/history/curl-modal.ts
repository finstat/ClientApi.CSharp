import { BaseComponent, define } from 'birko-web-core';
import type { BModal } from 'birko-web-components';

import { buildCurl } from '../api/curl-api.js';
import { t, onLocaleChange } from '../i18n/index.js';

/**
 * Shows three curl command variants (GET with query string, POST form, POST JSON)
 * mirroring DesktopFinstatApiTester's Curl window. Each has a Copy button.
 */
export class CurlModal extends BaseComponent {
  private _commands: string[] = [];
  private _label: string | null = null;
  private _unsubLocale?: () => void;

  private get _title(): string {
    return this._label ? t('curl.titleFor', { label: this._label }) : t('curl.title');
  }

  static get styles() {
    return `
      :host { display: contents; }
      h4 { margin: 0 0 0.3rem; font-size: var(--b-text-sm, 0.8125rem); color: var(--b-text-secondary); }
      .cmd { margin-bottom: var(--b-space-md, 1rem); }
    `;
  }

  render() {
    const parts = [t('curl.tab.get'), t('curl.tab.post'), t('curl.tab.postJson')];
    return `
      <b-modal id="modal" title="${this._title}" size="xl">
        ${this._commands.length === 0
          ? `<b-empty icon="&#9889;" message="${escapeHtml(t('curl.empty'))}"></b-empty>`
          : this._commands.map((cmd, i) => `
              <div class="cmd">
                <h4>${parts[i] ?? t('curl.commandN', { n: i + 1 })}</h4>
                <b-code-block language="bash" wrap>${escapeHtml(cmd)}</b-code-block>
              </div>
            `).join('')
        }
        <footer slot="footer">
          <b-button variant="primary" id="btn-close">${t('common.close')}</b-button>
        </footer>
      </b-modal>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    this._unsubLocale?.();
  }

  private _onClick = (e: Event) => {
    const target = e.target as HTMLElement;
    if (target.closest('#btn-close')) this.close();
  };

  async openFor(endpoint: string, params: unknown, label: string, headers?: Record<string, string>) {
    this._label = label;
    this._commands = [];
    this.update();
    this.$<BModal>('#modal')?.open();

    const result = await buildCurl(endpoint, params, headers);
    this._commands = result?.commands ?? [];
    this.update();
  }

  close() { this.$<BModal>('#modal')?.close(); }
}

function escapeHtml(s: string): string {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

define('curl-modal', CurlModal);

// Single instance mounted to body so the overlay covers the viewport.
if (typeof document !== 'undefined' && !document.querySelector('curl-modal')) {
  const mount = () => document.body.appendChild(document.createElement('curl-modal'));
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount, { once: true });
  } else {
    mount();
  }
}

/** Open the singleton curl modal for a given endpoint + params. Pass the history
 *  entry's `headers` snapshot so the rebuilt curl commands match the settings used
 *  for the original call. */
export function openCurlModal(endpoint: string, params: unknown, label: string, headers?: Record<string, string>) {
  const el = document.querySelector('curl-modal') as CurlModal | null;
  void el?.openFor(endpoint, params, label, headers);
}
