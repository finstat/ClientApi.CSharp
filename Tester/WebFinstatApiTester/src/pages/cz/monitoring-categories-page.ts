import { BaseComponent, define } from 'birko-web-core';
import { czMonitoring } from '../../api/cz-monitoring.js';
import { runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class CzMonitoringCategoriesPage extends BaseComponent {
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.cz.monitoring.categoriesTitle')}</h2>
      <p class="hint">${t('page.cz.monitoring.categoriesHint')}</p>
      <b-card>
        <b-button variant="primary" id="run">${t('page.cz.monitoring.categoriesButton')}</b-button>
      </b-card>
    `;
  }

  protected onMount() {
    this.$('#run')?.addEventListener('click', () =>
      void runApiCall(t('label.cz.monitoring.categories'), () => czMonitoring.categories()));
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    this._unsubLocale?.();
  }
}

define('cz-monitoring-categories-page', CzMonitoringCategoriesPage);
