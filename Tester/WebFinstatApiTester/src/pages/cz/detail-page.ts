import { BaseComponent, define } from 'birko-web-core';
import type { BInput } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { czDetail } from '../../api/cz-detail.js';
import { applyPendingPrefill, emphasizeAction, parseRouteSegments, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Kind = 'basic' | 'detail' | 'premium' | 'elite' | 'ultimate';

const BUTTON_KINDS: ReadonlyArray<Kind> = ['basic', 'detail', 'premium', 'elite', 'ultimate'];

export class CzDetailPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.cz.detail.title')}</h2>
      <p class="hint">${t('page.cz.detail.hint')}</p>
      <b-card header="${t('common.parameters')}">
        <div class="row">
          <b-input id="ico" label="${t('common.field.icoCz')}" placeholder="${t('common.field.icoCzPlaceholder')}" required></b-input>
        </div>
        <div class="buttons">
          ${BUTTON_KINDS.map(kind => `<b-button variant="primary" data-kind="${kind}">${t('ribbon.item.' + kind)}</b-button>`).join('')}
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
  }
  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
  }
  protected onUpdated() { this._applyRouteAction(); applyPendingPrefill(this.shadowRoot); }

  private _onHashChange = () => this._applyRouteAction();

  private _applyRouteAction() {
    const [action] = parseRouteSegments('#/cz/detail/');
    emphasizeAction(this.shadowRoot, action, { focus: '#ico' });
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-kind]') as HTMLElement | null;
    const kind = btn?.dataset.kind as Kind | undefined;
    if (kind) void this._run(kind);
  };

  private async _run(kind: Kind) {
    if (this._busy) return;
    const icoInput = this.$<BInput>('#ico');
    const ico = (icoInput?.value ?? '').trim();
    if (!ico) {
      toast.error(t('page.cz.detail.enterIco'));
      icoInput?.setAttribute('error', t('common.required'));
      icoInput?.focus();
      return;
    }
    icoInput?.removeAttribute('error');

    this._busy = true;
    try { await runApiCall(t(`label.cz.detail.${kind}`), () => czDetail[kind](ico)); }
    finally { this._busy = false; }
  }
}

define('cz-detail-page', CzDetailPage);
