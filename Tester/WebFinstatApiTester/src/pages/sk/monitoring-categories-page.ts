import { BaseComponent, define } from 'birko-web-core';
import { skMonitoring } from '../../api/sk-monitoring.js';
import { runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

export class SkMonitoringCategoriesPage extends BaseComponent {
  private _unsubLocale?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.monitoring.categoriesTitle')}</h2>
      <p class="hint">${t('page.sk.monitoring.categoriesHint')}</p>
      <b-card>
        <b-button variant="primary" id="run">${t('page.sk.monitoring.categoriesButton')}</b-button>
      </b-card>
    `;
  }

  protected onMount() {
    this.$('#run')?.addEventListener('click', () =>
      void runApiCall(t('label.sk.monitoring.categories'), () => skMonitoring.categories()));
    this._unsubLocale = onLocaleChange(() => this.update());
  }

  protected onUnmount() {
    this._unsubLocale?.();
  }
}

define('sk-monitoring-categories-page', SkMonitoringCategoriesPage);
