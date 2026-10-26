import { define } from 'birko-web-core';
import type { RibbonTab, SidebarItem, BRibbon } from 'birko-web-components';
import { registerProvider, type CommandItem } from 'birko-web-components/command';
import { BAppShell, registerThemes, BUILTIN_THEMES } from 'birko-web-shell/shell';
import type { MenuItem, ThemeOption } from 'birko-web-shell';

// Finstat branding only for now — build.js copies just css/themes/finstat.css and
// index.html links only that. See getAvailableThemes() below for why the registry
// call alone isn't enough.
registerThemes([BUILTIN_THEMES.finstat]);

import { TABS, resolveTabForHash, firstHrefForTab, ENVIRONMENT_TAB, RESPONSE_TAB, RESPONSE_SET } from './ribbon-tabs.js';
import { HISTORY_ACTIONS, HISTORY_ACTION_EVENT, type HistoryAction } from '../history/response-viewer.js';
import { historyStore, type ResponseHistoryItem } from '../history/response-history.js';
import { limitsStore } from '../api/limits-store.js';
import { settingsStore, activeProfile, useAndSaveSettings, profileColorCss } from '../settings/settings-store.js';
import { OPEN_ABOUT_EVENT, DOCS_URL } from './about-modal.js';
import './profile-switcher.js';
import { t, setLocale, getLocale, onLocaleChange, LOCALES, LOCALE_LABELS, LOCALE_ICONS, type Locale } from '../i18n/index.js';

/** Dispatched on window when the user picks "Settings" in the user menu. */
export const OPEN_SETTINGS_EVENT = 'finstat:open-settings';

/**
 * Maps the BAppShell framework's translation keys (nav.*, auth.*, status.*, comp.ribbon.*)
 * to this app's dictionary layout (shell.menu.*, shell.status.*, shell.comp.*).
 * Extends `this.t()` so the framework gets translated without touching its source.
 */
const FRAMEWORK_ALIASES: Record<string, string> = {
  'nav.profile':          'shell.menu.profile',
  'nav.settings':         'shell.menu.settings',
  'nav.notifications':    'shell.comp.notifications',
  'auth.signOut':         'shell.menu.signOut',
  'status.online':        'shell.status.online',
  'status.offline':       'shell.status.offline',
  'status.reconnecting':  'shell.status.reconnecting',
  'status.pending':       'shell.status.pending',
  'status.conflicts':     'shell.status.conflicts',
  'comp.ribbon.ribbon':   'shell.comp.ribbon.ribbon',
  'comp.ribbon.openNav':  'shell.comp.ribbon.openNav',
  'comp.ribbon.expand':   'shell.comp.ribbon.expand',
  'comp.ribbon.collapse': 'shell.comp.ribbon.collapse',
  'comp.ribbon.pin':         'shell.comp.ribbon.pin',
  'comp.ribbon.unpin':       'shell.comp.ribbon.unpin',
  'comp.ribbon.navigation':  'shell.comp.ribbon.navigation',
  'comp.ribbon.actions':     'shell.comp.ribbon.actions',
  'comp.ribbon.close':       'shell.comp.ribbon.close',
  'comp.ribbon.selectModule':'shell.comp.ribbon.selectModule',
  'comp.commandPalette.placeholder':   'shell.comp.commandPalette.placeholder',
  'comp.commandPalette.escClose':      'shell.comp.commandPalette.escClose',
  'comp.commandPalette.searching':     'shell.comp.commandPalette.searching',
  'comp.commandPalette.noResults':     'shell.comp.commandPalette.noResults',
  'comp.commandPalette.typeToSearch':  'shell.comp.commandPalette.typeToSearch',
  'comp.commandPalette.navigate':      'shell.comp.commandPalette.navigate',
  'comp.commandPalette.select':        'shell.comp.commandPalette.select',
  'comp.commandPalette.close':         'shell.comp.commandPalette.close',
};

/**
 * Ribbon shell for the FinStat API tester.
 */
export class AppShell extends BAppShell {
  // ── BCoreAppShell ─────────────────────────────────────────────────────────
  protected get brandName() { return t('shell.brand'); }
  protected get brandHref() { return '#/'; }
  protected get storagePrefix() { return 'finstat-tester'; }
  protected get showCommandPalette() { return true; }

  protected getUserName() { return 'Local'; }

  // active profile, switchable without opening Settings; its port is in the Environment tab
  protected renderHeaderActions() { return '<profile-switcher></profile-switcher>'; }

  /**
   * Finstat is the only theme on offer. The registry always keeps `'light'` (it is the
   * base `:root` in tokens.css and cannot be unregistered), so narrowing the list here
   * is what actually hides the switcher — it auto-hides below two entries. Registering
   * a second theme + dropping this override is all it takes to bring it back.
   */
  protected getAvailableThemes(): ThemeOption[] { return [BUILTIN_THEMES.finstat]; }
  protected t(key: string, params?: Record<string, string | number>) {
    return t(FRAMEWORK_ALIASES[key] ?? key, params);
  }
  protected onSignOut() { /* no auth */ }

  protected getUserMenuItems(): MenuItem[] {
    const current = getLocale();
    const items: MenuItem[] = [
      { id: 'settings', label: t('shell.menu.settings'), icon: '&#9881;' },
      { id: 'docs',     label: t('shell.menu.docs'),     icon: '&#128214;' },
    ];
    // BAppShell's MenuItem doesn't support nested sub-menus — render each locale
    // as a sibling entry with a country-flag icon and a check-mark on the active one.
    LOCALES.forEach((l, idx) => items.push({
      id: `lang:${l}`,
      label: `${l === current ? '&#10003; ' : ''}${LOCALE_LABELS[l]}`,
      icon: LOCALE_ICONS[l],
      divider: idx === 0,
    }));
    items.push({ id: 'about', label: t('shell.menu.about'), icon: '&#8505;', divider: true });
    return items;
  }

  protected onUserMenuSelect(id: string): void {
    if (id.startsWith('lang:')) {
      setLocale(id.slice('lang:'.length) as Locale);
      return;
    }
    switch (id) {
      case 'settings': window.dispatchEvent(new CustomEvent(OPEN_SETTINGS_EVENT)); break;
      case 'docs':     window.open(DOCS_URL, '_blank', 'noopener'); break;
      case 'about':    window.dispatchEvent(new CustomEvent(OPEN_ABOUT_EVENT)); break;
    }
  }

  // ── Status bar — active profile, its SK/CZ hosts + desktop tester's Daily/Monthly quota counters.
  protected getStatusText(): string {
    const parts: string[] = [activeProfile().name];
    const sk = settingsStore.get('finStatApiUrl');
    const cz = settingsStore.get('finStatApiUrlCZ');
    if (sk) parts.push(`SK: ${shortHost(sk)}`);
    if (cz) parts.push(`CZ: ${shortHost(cz)}`);
    const d = limitsStore.get('daily');
    const m = limitsStore.get('monthly');
    if (d) parts.push(t('shell.status.daily', { current: d.current, max: d.max }));
    if (m) parts.push(t('shell.status.monthly', { current: m.current, max: m.max }));
    return parts.join(' · ');
  }

  /** A stripe in the active profile's colour along the top of the status bar. */
  refreshStatusBar(): void {
    super.refreshStatusBar();
    const bar = this.$<HTMLElement>('.app-status-bar');
    const color = profileColorCss(activeProfile().color);
    if (bar) bar.style.boxShadow = color ? `inset 0 3px 0 ${color}` : '';
  }

  // ── BAppShell — ribbon ────────────────────────────────────────────────────
  /**
   * A picked tab that owns no route (Environment, the contextual Response tab). Without it every ribbon
   * refresh, e.g. the one a profile change in the Environment tab causes, would jump back to the tab of
   * the current page. Navigating clears it.
   */
  private _openTab: string | null = null;

  protected getRibbonTabs(): RibbonTab[] { return TABS(); }
  protected getActiveTabId(): string { return this._openTab ?? resolveTabForHash(window.location.hash); }
  protected onTabChange(tabId: string): void {
    if (tabId === ENVIRONMENT_TAB || tabId === RESPONSE_TAB) { this._openTab = tabId; return; }
    this._openTab = null;
    const href = firstHrefForTab(tabId);
    if (href) window.location.hash = href.slice(1); // strip leading '#'
  }

  protected onItemClick(_tabId: string, _groupId: string, itemId: string): void {
    if (itemId === 'rt-xml' || itemId === 'rt-json') {
      useAndSaveSettings({ responseType: itemId === 'rt-json' ? 'json' : 'xml' });
    } else if (itemId.startsWith('resp-') && HISTORY_ACTIONS.includes(itemId.slice(5) as HistoryAction)) {
      window.dispatchEvent(new CustomEvent(HISTORY_ACTION_EVENT, { detail: { action: itemId.slice(5) } }));
    } else if (itemId === 'settings') {
      window.dispatchEvent(new CustomEvent(OPEN_SETTINGS_EVENT));
    }
  }

  /** setTabs() in the base refresh rebuilds the tabs; the contextual Response tab follows the page again. */
  refreshRibbon(): void {
    super.refreshRibbon();
    this._syncContextual();
  }

  private get _ribbon(): BRibbon | null { return this.$<BRibbon>('#ribbon'); }

  /** The Response tab is shown while a history entry is open. */
  private _syncContextual(): void {
    const ribbon = this._ribbon;
    if (!ribbon) return;
    if (/^#\/history\/.+/.test(window.location.hash)) ribbon.showContextual(RESPONSE_SET);
    else {
      if (this._openTab === RESPONSE_TAB) this._openTab = null;
      ribbon.hideContextual(RESPONSE_SET);
    }
  }

  /** Field changes in the Environment tab are saved as "Use and Save" in Settings would. */
  private _onRibbonFieldChange = (e: Event): void => {
    const { itemId, value } = (e as CustomEvent<{ itemId: string; value: string }>).detail;
    if (itemId === 'env-port') {
      const port = Number(value);
      const active = activeProfile();
      const valid = value.trim() === '' ? null : Number.isInteger(port) && port >= 1 && port <= 65535 ? port : undefined;
      if (valid === undefined) { this.refreshRibbon(); return; }   // put the saved port back
      useAndSaveSettings({ profiles: settingsStore.get('profiles').map(p => p.id === active.id ? { ...p, port: valid } : p) });
    } else if (itemId === 'env-lang') {
      useAndSaveSettings({ lang: value === 'cs' || value === 'sk' ? value : '' });
    }
  };

  /**
   * The ribbon's contextual sets, Quick Access Toolbar key and events. Re-applied after every shell
   * render: its morph puts the ribbon's attributes back to the template, which has no qat-key.
   */
  private _setupRibbonExtras(first: boolean): void {
    const ribbon = this._ribbon;
    if (!ribbon) return;
    if (ribbon.getAttribute('qat-key') !== this.storagePrefix) ribbon.setAttribute('qat-key', this.storagePrefix);
    ribbon.setContextualSets([{ id: RESPONSE_SET, label: t('ribbon.contextual.response'), color: 'info' }]);
    this._syncContextual();
    if (!first) return;
    ribbon.setQuickAccess([{ id: 'settings', label: t('shell.menu.settings'), icon: '&#9881;', action: true }]);
    ribbon.addEventListener('item-change', this._onRibbonFieldChange);
    ribbon.addEventListener('group-launcher', () => window.dispatchEvent(new CustomEvent(OPEN_SETTINGS_EVENT)));
  }

  protected onUpdated(): void {
    super.onUpdated();
    this._setupRibbonExtras(false);
  }

  // ── BSidebarAppShell — left sidebar (response history) ────────────────────
  protected get showLeftSidebar() { return true; }
  /**
   * Calls and comparisons, newest first. A comparison's own calls are listed under it while it or one
   * of them is open, so the way back to the comparison stays in view; b-sidebar draws no nesting, the
   * arrow in the label does.
   */
  protected getLeftSidebarItems(): SidebarItem[] {
    const items = historyStore.get('items');
    const compares = historyStore.get('compares');
    const groups = new Set(compares.map(c => c.id));
    const active = this.getActiveLeftSidebarItem();
    const openGroup = groups.has(active) ? active : items.find(i => i.id === active)?.groupId;
    const time = (ts: number) => new Date(ts).toLocaleTimeString([], { hour12: false });
    const call = (it: ResponseHistoryItem, nested: boolean): SidebarItem => ({
      id: it.id,
      label: `${nested ? '&#8627; ' : ''}${it.label} · ${time(it.timestamp)}`,
      icon: it.ok ? '&#10003;' : '&#10005;',
      href: `#/history/${it.id}`,
    });
    const top: Array<{ timestamp: number; entries: () => SidebarItem[] }> = [
      ...items.filter(i => !i.groupId || !groups.has(i.groupId)).map(it => ({ timestamp: it.timestamp, entries: () => [call(it, false)] })),
      ...compares.map(c => ({
        timestamp: c.timestamp,
        entries: () => [
          { id: c.id, label: `${c.label} · ${time(c.timestamp)}`, icon: '&#8644;', href: `#/compare/${c.id}` },
          ...(c.id === openGroup ? items.filter(i => i.groupId === c.id).map(it => call(it, true)) : []),
        ],
      })),
    ];
    return top.sort((x, y) => y.timestamp - x.timestamp).slice(0, 50).flatMap(e => e.entries());
  }
  protected getActiveLeftSidebarItem() {
    return window.location.hash.match(/^#\/(?:history|compare)\/(.+)$/)?.[1] ?? '';
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────
  protected onMount() {
    // Pin the ribbon by default on first run. BAppShell.onMount reads this
    // localStorage key and calls `ribbon.pin()` when it's `"true"`. Seeding the
    // value (only when absent) gives a pinned default while still honouring any
    // explicit user change afterwards.
    const pinKey = `${this.storagePrefix}-ribbon-pinned`;
    if (localStorage.getItem(pinKey) === null) localStorage.setItem(pinKey, 'true');

    super.onMount();
    // super.onMount() restores the persisted theme, which for an existing install can
    // still be a theme this build no longer ships CSS for. Force the one we do.
    if (this.currentTheme !== BUILTIN_THEMES.finstat.id) this.setTheme(BUILTIN_THEMES.finstat.id);
    // History-entry navigation (#/history/<id>) doesn't own a ribbon tab, so
    // resolveTabForHash falls back to 'detail' → refreshRibbon() sees
    // prevActive === nextActive and re-expands the panel. When unpinned, that
    // overlays ribbon-panel-inner over the page on every sidebar click.
    const onHashChange = () => {
      this._openTab = null;
      if (!window.location.hash.startsWith('#/history/')) this.refreshRibbon();
      else this._syncContextual();
      this.refreshLeftSidebar();
    };
    this._setupRibbonExtras(true);
    window.addEventListener('hashchange', onHashChange);
    this._unsubs.push(
      () => window.removeEventListener('hashchange', onHashChange),
      historyStore.onChange('items',      () => this.refreshLeftSidebar()),
      historyStore.onChange('compares',   () => this.refreshLeftSidebar()),
      historyStore.onChange('selectedId', () => this.refreshLeftSidebar()),
      limitsStore.onChange('daily',          () => this.refreshStatusBar()),
      limitsStore.onChange('monthly',        () => this.refreshStatusBar()),
      settingsStore.onChange('finStatApiUrl',   () => this.refreshStatusBar()),
      settingsStore.onChange('finStatApiUrlCZ', () => this.refreshStatusBar()),
      settingsStore.onChange('activeProfileId', () => { this.refreshStatusBar(); this.refreshRibbon(); }),
      // the Environment tab and the tooltips show these, and the profile list may have changed
      settingsStore.onChange('profiles',     () => { this.refreshRibbon(); this.refreshStatusBar(); }),
      settingsStore.onChange('responseType', () => this.refreshRibbon()),
      settingsStore.onChange('lang',         () => this.refreshRibbon()),
      onLocaleChange(() => {
        this.refreshRibbon();
        this.refreshStatusBar();
        this.refreshLeftSidebar();
        this.refreshUserMenu();
        // Re-render so brand name, etc. pick up the new locale.
        this.update();
      }),
    );
  }
}

define('app-shell', AppShell);

/** Trim a URL to just its host + port for compact status-bar display. Falls back
 *  to the original string if it's not parseable (e.g. blank or malformed). */
function shortHost(url: string): string {
  try {
    const u = new URL(url);
    return u.port ? `${u.hostname}:${u.port}` : u.hostname;
  } catch {
    return url;
  }
}

// Command-palette provider — flatten ribbon tabs into searchable endpoint items.
// Registered at module load so Ctrl+K picks everything up before the first render.
registerProvider({
  id: 'endpoints',
  label: 'Endpoints',
  order: 10,
  search(query: string): CommandItem[] {
    const q = query.trim().toLowerCase();
    const items: CommandItem[] = [];
    for (const tab of TABS()) {
      for (const group of tab.groups) {
        for (const it of group.items) {
          if (!it.href) continue;
          items.push({
            id: it.id,
            label: it.label,
            description: `${tab.label} · ${group.label}`,
            icon: it.icon,
            category: tab.label,
            href: it.href,
          });
        }
      }
    }
    if (!q) return items;
    return items.filter(it =>
      it.label.toLowerCase().includes(q)
      || (it.description?.toLowerCase().includes(q) ?? false),
    );
  },
});
