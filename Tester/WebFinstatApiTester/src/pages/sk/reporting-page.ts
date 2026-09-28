import { BaseComponent, define } from 'birko-web-core';
import type { BInput, BSelect } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skReporting } from '../../api/sk-reporting.js';
import { cacheStore, type ReportingTopicOption } from '../../api/cache-store.js';
import { applyPendingPrefill, emphasizeAction, parseRouteSegments, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Action = 'topics' | 'topics-quiet' | 'list' | 'list-quiet' | 'download';

function topicOptions(topics: readonly ReportingTopicOption[]): { value: string; label: string; group?: string }[] {
  return topics.map(tp => tp.group
    ? { value: tp.id, label: tp.name, group: tp.group }
    : { value: tp.id, label: tp.name });
}

export class SkReportingPage extends BaseComponent {
  private _busy = false;
  private _unsubLocale?: () => void;
  private _unsubCache?: () => void;
  private _unsubFiles?: () => void;

  static get styles() { return PAGE_STYLES; }

  render() {
    return `
      <h2>${t('page.sk.reporting.title')}</h2>
      <p class="hint">${t('page.sk.reporting.hint')}</p>

      <b-card header="${t('ribbon.item.topics')}">
        <div class="buttons">
          <b-button variant="primary" data-action="topics">${t('page.sk.reporting.loadTopics')}</b-button>
          <b-button variant="ghost"   data-action="topics-quiet">${t('common.runWithoutRedirect')}</b-button>
        </div>
      </b-card>

      <b-card header="${t('page.sk.reporting.listByTopic')}">
        <div class="row">
          <b-select id="topic" label="${t('page.sk.reporting.topic')}" placeholder="${t('page.sk.reporting.topicPlaceholder')}" required></b-select>
          <b-button variant="primary" data-action="list">${t('page.sk.reporting.loadList')}</b-button>
          <b-button variant="ghost"   data-action="list-quiet">${t('common.runWithoutRedirect')}</b-button>
        </div>
      </b-card>

      <b-card header="${t('ribbon.item.download')}">
        <div class="row">
          <b-input id="file" label="${t('common.field.fileName')}" placeholder="${t('common.field.fileNamePlaceholder')}" required></b-input>
          <b-button variant="primary" data-action="download">${t('page.sk.reporting.download')}</b-button>
        </div>
      </b-card>
    `;
  }

  protected onMount() {
    this.shadowRoot?.addEventListener('click', this._onClick);
    this.shadowRoot?.addEventListener('change', this._onTopicChange);
    window.addEventListener('hashchange', this._onHashChange);
    this._unsubLocale = onLocaleChange(() => this.update());
    this._unsubCache  = cacheStore.on('skReportingTopics', tp => this._applyTopics(tp));
    this._unsubFiles  = cacheStore.on('skReportingFiles',  () => this._applyFiles());
  }
  protected onUnmount() {
    this.shadowRoot?.removeEventListener('click', this._onClick);
    this.shadowRoot?.removeEventListener('change', this._onTopicChange);
    window.removeEventListener('hashchange', this._onHashChange);
    this._unsubLocale?.();
    this._unsubCache?.();
    this._unsubFiles?.();
  }
  protected onUpdated() {
    this._applyTopics(cacheStore.get('skReportingTopics'));
    this._applyFiles();
    this._applyRouteAction();
    applyPendingPrefill(this.shadowRoot);
  }

  /** Push filename suggestions onto the download input — only when the cache topic matches the selected topic. */
  private _applyFiles() {
    const input = this.$<BInput>('#file');
    if (!input) return;
    const topic = (this.$<BSelect>('#topic')?.value ?? '').trim();
    const cache = cacheStore.get('skReportingFiles');
    input.setSuggestions(topic && cache && cache.topic === topic ? cache.files : []);
  }

  /** Re-apply suggestions when the user picks a different topic — the cache may or may not match. */
  private _onTopicChange = (e: Event) => {
    if ((e.target as HTMLElement | null)?.id === 'topic') this._applyFiles();
  };

  private _applyTopics(topics: readonly ReportingTopicOption[]) {
    this.$<BSelect>('#topic')?.setOptions(topicOptions(topics));
  }

  private _onHashChange = () => this._applyRouteAction();

  private _applyRouteAction() {
    const [action] = parseRouteSegments('#/sk/reporting/');
    const focus = action === 'list' ? '#topic' : action === 'download' ? '#file' : undefined;
    emphasizeAction(this.shadowRoot, action as Action | undefined, { focus });
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    const action = btn?.dataset.action as Action | undefined;
    if (action) void this._run(action);
  };

  private async _run(action: Action) {
    if (this._busy) return;

    if (action === 'topics' || action === 'topics-quiet') {
      this._busy = true;
      try {
        await runApiCall(
          t('label.sk.reporting.topics'),
          () => skReporting.topics(),
          { silent: action === 'topics-quiet' },
        );
      } finally { this._busy = false; }
      return;
    }

    if (action === 'list' || action === 'list-quiet') {
      const input = this.$<BSelect>('#topic');
      const topic = (input?.value ?? '').trim();
      if (!topic) { toast.error(t('page.sk.reporting.topicRequired')); input?.setAttribute('error', t('common.required')); return; }
      input?.removeAttribute('error');
      this._busy = true;
      try {
        await runApiCall(
          t('label.sk.reporting.list'),
          () => skReporting.list(topic),
          { silent: action === 'list-quiet' },
        );
      } finally { this._busy = false; }
      return;
    }

    const input = this.$<BInput>('#file');
    const file = (input?.value ?? '').trim();
    if (!file) { toast.error(t('page.sk.reporting.filenameRequired')); input?.setAttribute('error', t('common.required')); input?.focus(); return; }
    input?.removeAttribute('error');
    this._busy = true;
    try { await runApiCall(t('label.sk.reporting.download'), () => skReporting.download(file)); }
    finally { this._busy = false; }
  }
}

define('sk-reporting-page', SkReportingPage);
