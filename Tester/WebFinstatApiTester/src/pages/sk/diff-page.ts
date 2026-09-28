import { BaseComponent, define } from 'birko-web-core';
import type { BInput, BSelect } from 'birko-web-components';
import { toast } from 'birko-web-components';

import { skDiff } from '../../api/sk-diff.js';
import { cacheStore } from '../../api/cache-store.js';
import { applyPendingPrefill, emphasizeAction, parseRouteSegments, runApiCall } from '../page-helpers.js';
import { PAGE_STYLES } from '../page-styles.js';
import { t, onLocaleChange } from '../../i18n/index.js';

type Kind = 'daily' | 'ultimate' | 'statement' | 'statement2014';

const NAME_KEYS: Record<Kind, string> = {
  daily:         'page.sk.diff.daily',
  ultimate:      'page.sk.diff.ultimate',
  statement:     'page.sk.diff.statement',
  statement2014: 'page.sk.diff.statement2014',
};

function name(kind: Kind): string { return t(NAME_KEYS[kind]); }

const HAS_LEGEND: Record<Kind, boolean> = {
  daily: false, ultimate: false, statement: true, statement2014: true,
};

function resolveKind(hash: string): Kind {
  const m = /^#\/sk\/diff\/([^/]+)/.exec(hash);
  const k = m?.[1] as Kind | undefined;
  return k && k in NAME_KEYS ? k : 'daily';
}

/**
 * One page per Diff family variant (daily / ultimate / statement / statement2014).
 * Same tag registered via multiple routes — the page reads `kind` from the hash
 * and adapts (legend card only shows for the two statement variants).
 */
export class SkDiffPage extends BaseComponent {
  private _busy = false;
  private _kind: Kind = 'daily';
  private _hashListener = () => this._sync();
  private _unsubLocale?: () => void;
  private _unsubCache?: () => void;

  static get styles() { return PAGE_STYLES; }

  protected onMount() {
    this._sync();
    window.addEventListener('hashchange', this._hashListener);
    this.shadowRoot?.addEventListener('click', this._onClick);
    this._unsubLocale = onLocaleChange(() => this.update());
    this._unsubCache = cacheStore.on('skDiffFiles', () => this._applyFiles());
  }

  protected onUnmount() {
    window.removeEventListener('hashchange', this._hashListener);
    this.shadowRoot?.removeEventListener('click', this._onClick);
    this._unsubLocale?.();
    this._unsubCache?.();
  }

  private _sync() {
    const k = resolveKind(window.location.hash);
    if (k !== this._kind) { this._kind = k; this.update(); return; }
    // Kind unchanged but action segment may have — re-apply emphasis without re-rendering.
    this._applyRouteAction();
  }

  protected onUpdated() {
    // Populate the legend-language select whenever it's present (render toggles it per kind).
    const lang = this.$<BSelect>('#lang');
    if (lang && !lang.getAttribute('value')) {
      lang.setOptions([
        { value: 'sk', label: 'sk' },
        { value: 'en', label: 'en' },
        { value: 'cz', label: 'cz' },
      ]);
      lang.setAttribute('value', 'sk');
    }
    this._applyRouteAction();
    this._applyFiles();
    applyPendingPrefill(this.shadowRoot);
  }

  /** Push filename suggestions onto the download input — only when the cache matches the current kind. */
  private _applyFiles() {
    const input = this.$<BInput>('#file');
    if (!input) return;
    const cache = cacheStore.get('skDiffFiles');
    input.setSuggestions(cache && cache.kind === this._kind ? cache.files : []);
  }

  private _applyRouteAction() {
    const [, action] = parseRouteSegments('#/sk/diff/');
    const focus = action === 'download' ? '#file' : action === 'legend' ? '#lang' : undefined;
    emphasizeAction(this.shadowRoot, action, { focus });
  }

  render() {
    const legendCard = HAS_LEGEND[this._kind] ? `
      <b-card header="${t('ribbon.item.legend')}">
        <div class="row">
          <b-select id="lang" label="${t('common.field.language')}"></b-select>
          <b-button variant="primary" data-action="legend">${t('page.sk.diff.loadLegend')}</b-button>
        </div>
      </b-card>` : '';

    return `
      <h2>SK ${name(this._kind)}</h2>
      <p class="hint">${t('page.sk.diff.hint')}</p>

      <b-card header="${t('common.files')}">
        <div class="buttons">
          <b-button variant="primary" data-action="list">${t('page.sk.diff.listButton')}</b-button>
          <b-button variant="ghost"   data-action="list-quiet">${t('common.runWithoutRedirect')}</b-button>
        </div>
      </b-card>

      <b-card header="${t('ribbon.item.download')}">
        <div class="row">
          <b-input id="file" label="${t('common.field.fileName')}" placeholder="${t('common.field.fileNamePlaceholder')}" required></b-input>
          <b-button variant="primary" data-action="download">${t('page.sk.diff.downloadButton')}</b-button>
        </div>
      </b-card>

      ${legendCard}
    `;
  }

  private _onClick = (e: Event) => {
    const btn = (e.target as HTMLElement).closest('[data-action]') as HTMLElement | null;
    const action = btn?.dataset.action;
    if (action) void this._run(action);
  };

  private async _run(action: string) {
    if (this._busy) return;
    const kind = this._kind;

    if (action === 'list' || action === 'list-quiet') {
      this._busy = true;
      try {
        await runApiCall(
          t(`label.sk.diff.${kind}List`),
          () => skDiff.list(kind),
          { silent: action === 'list-quiet' },
        );
      } finally { this._busy = false; }
      return;
    }

    if (action === 'download') {
      const input = this.$<BInput>('#file');
      const fileName = (input?.value ?? '').trim();
      if (!fileName) { toast.error(t('page.sk.diff.enterFilename')); input?.setAttribute('error', t('common.required')); input?.focus(); return; }
      input?.removeAttribute('error');

      this._busy = true;
      try { await runApiCall(t(`label.sk.diff.${kind}Download`), () => skDiff.download(kind, fileName)); }
      finally { this._busy = false; }
      return;
    }

    if (action === 'legend' && HAS_LEGEND[kind]) {
      const lang = this.$<BSelect>('#lang')?.value || 'sk';
      this._busy = true;
      try { await runApiCall(t(`label.sk.diff.${kind}Legend`), () => skDiff.legend(kind as 'statement' | 'statement2014', lang)); }
      finally { this._busy = false; }
    }
  }
}

define('sk-diff-page', SkDiffPage);
