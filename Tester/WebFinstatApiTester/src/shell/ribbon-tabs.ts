import type { RibbonTab } from 'birko-web-components';

import { t } from '../i18n/index.js';

/** HTML-entity icons used across the ribbon. Centralised so similar actions share a glyph. */
const ICON = {
  // Tabs
  detail:     '&#127970;', // 🏢 office building
  monitoring: '&#128065;', // 👁 eye
  reporting:  '&#128202;', // 📊 bar chart
  statement:  '&#128209;', // 📑 bookmark tabs
  diff:       '&#916;',    // Δ delta
  distraint:  '&#9878;',   // ⚖ balance scale
  bankruptcy: '&#9888;',   // ⚠ warning

  // Common item actions
  list:        '&#128203;', // 📋 clipboard
  add:         '&#10133;',  // ➕ plus
  remove:      '&#10134;',  // ➖ minus
  report:      '&#128196;', // 📄 page
  proceedings: '&#9878;',   // ⚖ scale
  categories:  '&#128215;', // 📗 book
  search:      '&#128269;', // 🔍 magnifying glass
  download:    '&#11015;',  // ⬇ down arrow
  legend:      '&#128273;', // 🔑 key
  person:      '&#128100;', // 👤 person
  company:     '&#127970;', // 🏢
  tier:        '&#128290;', // 🔢 input numbers
  login:       '&#128273;', // 🔑
} as const;

/**
 * Ribbon structure mirroring DesktopFinstatApiTester's Fluent ribbon.
 * Returns a fresh array each call so labels re-resolve when the locale changes.
 */
export function TABS(): RibbonTab[] {
  return [
    {
      id: 'detail', label: t('ribbon.tab.detail'), icon: ICON.detail,
      groups: [
        {
          id: 'sk-detail', label: t('ribbon.group.skDetail'), items: [
            { id: 'sk-detail-basic',    label: t('ribbon.item.basic'),    icon: ICON.tier, href: '#/sk/detail/basic' },
            { id: 'sk-detail-detail',   label: t('ribbon.item.detail'),   icon: ICON.tier, href: '#/sk/detail/detail' },
            { id: 'sk-detail-extended', label: t('ribbon.item.extended'), icon: ICON.tier, href: '#/sk/detail/extended' },
            { id: 'sk-detail-ultimate', label: t('ribbon.item.ultimate'), icon: ICON.tier, href: '#/sk/detail/ultimate' },
          ],
        },
        {
          id: 'sk-other', label: t('ribbon.group.skOther'), items: [
            { id: 'sk-autocomplete', label: t('ribbon.item.autoComplete'), icon: ICON.search, href: '#/sk/autocomplete' },
            { id: 'sk-autologin',    label: t('ribbon.item.autoLogin'),    icon: ICON.login,  href: '#/sk/autologin' },
          ],
        },
        {
          id: 'cz-detail', label: t('ribbon.group.czDetail'), items: [
            { id: 'cz-detail-basic',   label: t('ribbon.item.basic'),   icon: ICON.tier, href: '#/cz/detail/basic' },
            { id: 'cz-detail-detail',  label: t('ribbon.item.detail'),  icon: ICON.tier, href: '#/cz/detail/detail' },
            { id: 'cz-detail-premium', label: t('ribbon.item.premium'), icon: ICON.tier, href: '#/cz/detail/premium' },
            { id: 'cz-detail-elite',   label: t('ribbon.item.elite'),   icon: ICON.tier, href: '#/cz/detail/elite' },
            { id: 'cz-detail-ultimate', label: t('ribbon.item.ultimate'), icon: ICON.tier, href: '#/cz/detail/ultimate' },
          ],
        },
        {
          id: 'cz-other', label: t('ribbon.group.czOther'), items: [
            { id: 'cz-autocomplete', label: t('ribbon.item.autoComplete'), icon: ICON.search, href: '#/cz/autocomplete' },
            { id: 'cz-autologin',    label: t('ribbon.item.autoLogin'),    icon: ICON.login,  href: '#/cz/autologin' },
          ],
        },
      ],
    },
    {
      id: 'monitoring', label: t('ribbon.tab.monitoring'), icon: ICON.monitoring,
      groups: [
        {
          id: 'sk-mon-cat', label: t('ribbon.group.skCategories'), items: [
            { id: 'sk-mon-categories', label: t('ribbon.item.categories'), icon: ICON.categories, href: '#/sk/monitoring/categories' },
          ],
        },
        {
          id: 'sk-mon-ico', label: t('ribbon.group.skIco'), items: [
            { id: 'sk-mon-add',         label: t('ribbon.item.add'),         icon: ICON.add,         href: '#/sk/monitoring/add' },
            { id: 'sk-mon-remove',      label: t('ribbon.item.remove'),      icon: ICON.remove,      href: '#/sk/monitoring/remove' },
            { id: 'sk-mon-list',        label: t('ribbon.item.list'),        icon: ICON.list,        href: '#/sk/monitoring/list' },
            { id: 'sk-mon-report',      label: t('ribbon.item.report'),      icon: ICON.report,      href: '#/sk/monitoring/report' },
            { id: 'sk-mon-proceedings', label: t('ribbon.item.proceedings'), icon: ICON.proceedings, href: '#/sk/monitoring/proceedings' },
          ],
        },
        {
          id: 'sk-mon-date', label: t('ribbon.group.skDate'), items: [
            { id: 'sk-mon-date-add',         label: t('ribbon.item.add'),         icon: ICON.add,         href: '#/sk/monitoring/date/add' },
            { id: 'sk-mon-date-remove',      label: t('ribbon.item.remove'),      icon: ICON.remove,      href: '#/sk/monitoring/date/remove' },
            { id: 'sk-mon-date-list',        label: t('ribbon.item.list'),        icon: ICON.list,        href: '#/sk/monitoring/date/list' },
            { id: 'sk-mon-date-report',      label: t('ribbon.item.report'),      icon: ICON.report,      href: '#/sk/monitoring/date/report' },
            { id: 'sk-mon-date-proceedings', label: t('ribbon.item.proceedings'), icon: ICON.proceedings, href: '#/sk/monitoring/date/proceedings' },
          ],
        },
        {
          id: 'cz-mon-cat', label: t('ribbon.group.czCategories'), items: [
            { id: 'cz-mon-categories', label: t('ribbon.item.categories'), icon: ICON.categories, href: '#/cz/monitoring/categories' },
          ],
        },
        {
          id: 'cz-mon-ico', label: t('ribbon.group.czIco'), items: [
            { id: 'cz-mon-add',    label: t('ribbon.item.add'),    icon: ICON.add,    href: '#/cz/monitoring/add' },
            { id: 'cz-mon-remove', label: t('ribbon.item.remove'), icon: ICON.remove, href: '#/cz/monitoring/remove' },
            { id: 'cz-mon-list',   label: t('ribbon.item.list'),   icon: ICON.list,   href: '#/cz/monitoring/list' },
            { id: 'cz-mon-report', label: t('ribbon.item.report'), icon: ICON.report, href: '#/cz/monitoring/report' },
          ],
        },
        {
          id: 'cz-mon-date', label: t('ribbon.group.czDate'), items: [
            { id: 'cz-mon-date-add',    label: t('ribbon.item.add'),    icon: ICON.add,    href: '#/cz/monitoring/date/add' },
            { id: 'cz-mon-date-remove', label: t('ribbon.item.remove'), icon: ICON.remove, href: '#/cz/monitoring/date/remove' },
            { id: 'cz-mon-date-list',   label: t('ribbon.item.list'),   icon: ICON.list,   href: '#/cz/monitoring/date/list' },
            { id: 'cz-mon-date-report', label: t('ribbon.item.report'), icon: ICON.report, href: '#/cz/monitoring/date/report' },
          ],
        },
      ],
    },
    {
      id: 'reporting', label: t('ribbon.tab.reporting'), icon: ICON.reporting,
      groups: [
        {
          id: 'reporting', label: t('ribbon.group.reporting'), items: [
            { id: 'rep-topics',   label: t('ribbon.item.topics'),   icon: ICON.categories, href: '#/sk/reporting/topics' },
            { id: 'rep-list',     label: t('ribbon.item.list'),     icon: ICON.list,       href: '#/sk/reporting/list' },
            { id: 'rep-download', label: t('ribbon.item.download'), icon: ICON.download,   href: '#/sk/reporting/download' },
          ],
        },
      ],
    },
    {
      id: 'statement', label: t('ribbon.tab.statement'), icon: ICON.statement,
      groups: [
        {
          id: 'statement', label: t('ribbon.group.statement'), items: [
            { id: 'st-list',   label: t('ribbon.item.list'),   icon: ICON.list,   href: '#/sk/statement/list' },
            { id: 'st-detail', label: t('ribbon.item.detail'), icon: ICON.report, href: '#/sk/statement/detail' },
            { id: 'st-legend', label: t('ribbon.item.legend'), icon: ICON.legend, href: '#/sk/statement/legend' },
          ],
        },
      ],
    },
    {
      id: 'diff', label: t('ribbon.tab.diff'), icon: ICON.diff,
      groups: [
        {
          id: 'daily', label: t('ribbon.group.daily'), items: [
            { id: 'diff-daily-list',     label: t('ribbon.item.list'),     icon: ICON.list,     href: '#/sk/diff/daily/list' },
            { id: 'diff-daily-download', label: t('ribbon.item.download'), icon: ICON.download, href: '#/sk/diff/daily/download' },
          ],
        },
        {
          id: 'ultimate', label: t('ribbon.group.ultimate'), items: [
            { id: 'diff-ult-list',     label: t('ribbon.item.list'),     icon: ICON.list,     href: '#/sk/diff/ultimate/list' },
            { id: 'diff-ult-download', label: t('ribbon.item.download'), icon: ICON.download, href: '#/sk/diff/ultimate/download' },
          ],
        },
        {
          id: 'diff-statement', label: t('ribbon.group.diffStatement'), items: [
            { id: 'diff-st-list',     label: t('ribbon.item.list'),     icon: ICON.list,     href: '#/sk/diff/statement/list' },
            { id: 'diff-st-download', label: t('ribbon.item.download'), icon: ICON.download, href: '#/sk/diff/statement/download' },
            { id: 'diff-st-legend',   label: t('ribbon.item.legend'),   icon: ICON.legend,   href: '#/sk/diff/statement/legend' },
          ],
        },
        {
          id: 'diff-statement-2014', label: t('ribbon.group.diffStatement2014'), items: [
            { id: 'diff-st14-list',     label: t('ribbon.item.list'),     icon: ICON.list,     href: '#/sk/diff/statement2014/list' },
            { id: 'diff-st14-download', label: t('ribbon.item.download'), icon: ICON.download, href: '#/sk/diff/statement2014/download' },
            { id: 'diff-st14-legend',   label: t('ribbon.item.legend'),   icon: ICON.legend,   href: '#/sk/diff/statement2014/legend' },
          ],
        },
      ],
    },
    {
      id: 'distraint', label: t('ribbon.tab.distraint'), icon: ICON.distraint,
      groups: [
        {
          id: 'dis-paid', label: t('ribbon.group.disPaid'), items: [
            { id: 'dis-search', label: t('ribbon.item.search'), icon: ICON.search, href: '#/sk/distraint/search' },
            { id: 'dis-detail', label: t('ribbon.item.detail'), icon: ICON.report, href: '#/sk/distraint/detail' },
          ],
        },
        {
          id: 'dis-results', label: t('ribbon.group.disResults'), items: [
            { id: 'dis-results-run',   label: t('ribbon.item.results'),        icon: ICON.list,   href: '#/sk/distraint/results' },
            { id: 'dis-results-token', label: t('ribbon.item.resultsByToken'), icon: ICON.login,  href: '#/sk/distraint/results-by-token' },
            { id: 'dis-stored',        label: t('ribbon.item.storedDetail'),   icon: ICON.report, href: '#/sk/distraint/stored-detail' },
          ],
        },
      ],
    },
    {
      id: 'bankruptcy', label: t('ribbon.tab.bankruptcy'), icon: ICON.bankruptcy,
      groups: [
        {
          id: 'bk-persons', label: t('ribbon.group.bkPersons'), items: [
            { id: 'bk-person', label: t('ribbon.item.personProceedings'), icon: ICON.person, href: '#/sk/bankruptcy/persons' },
          ],
        },
        {
          id: 'bk-ico', label: t('ribbon.group.bkIco'), items: [
            { id: 'bk-company', label: t('ribbon.item.company'), icon: ICON.company, href: '#/sk/bankruptcy/company' },
          ],
        },
      ],
    },
  ];
}

/** Flat list of hrefs keyed by tab — keeps resolveTabForHash cheap & language-agnostic. */
const TAB_HREFS: Record<string, readonly string[]> = {
  detail:     ['#/sk/detail/basic', '#/sk/detail/detail', '#/sk/detail/extended', '#/sk/detail/ultimate',
               '#/sk/autocomplete', '#/sk/autologin',
               '#/cz/detail/basic', '#/cz/detail/detail', '#/cz/detail/premium', '#/cz/detail/elite', '#/cz/detail/ultimate',
               '#/cz/autocomplete', '#/cz/autologin'],
  monitoring: ['#/sk/monitoring/categories', '#/sk/monitoring/list', '#/sk/monitoring/add',
               '#/sk/monitoring/remove', '#/sk/monitoring/report', '#/sk/monitoring/proceedings',
               '#/sk/monitoring/date/list', '#/sk/monitoring/date/add', '#/sk/monitoring/date/remove',
               '#/sk/monitoring/date/report', '#/sk/monitoring/date/proceedings',
               '#/cz/monitoring/categories', '#/cz/monitoring/list', '#/cz/monitoring/add',
               '#/cz/monitoring/remove', '#/cz/monitoring/report',
               '#/cz/monitoring/date/list', '#/cz/monitoring/date/add',
               '#/cz/monitoring/date/remove', '#/cz/monitoring/date/report'],
  reporting:  ['#/sk/reporting/topics', '#/sk/reporting/list', '#/sk/reporting/download'],
  statement:  ['#/sk/statement/list', '#/sk/statement/detail', '#/sk/statement/legend'],
  diff:       ['#/sk/diff/daily/list', '#/sk/diff/daily/download',
               '#/sk/diff/ultimate/list', '#/sk/diff/ultimate/download',
               '#/sk/diff/statement/list', '#/sk/diff/statement/download', '#/sk/diff/statement/legend',
               '#/sk/diff/statement2014/list', '#/sk/diff/statement2014/download', '#/sk/diff/statement2014/legend'],
  distraint:  ['#/sk/distraint/search', '#/sk/distraint/detail',
               '#/sk/distraint/results', '#/sk/distraint/results-by-token', '#/sk/distraint/stored-detail'],
  bankruptcy: ['#/sk/bankruptcy/persons', '#/sk/bankruptcy/company'],
};

const TAB_ORDER = ['detail', 'monitoring', 'reporting', 'statement', 'diff', 'distraint', 'bankruptcy'] as const;

/** Resolve the ribbon tab that owns the given hash (e.g. "#/sk/detail/basic"). */
export function resolveTabForHash(hash: string): string {
  for (const tabId of TAB_ORDER) {
    if (TAB_HREFS[tabId].includes(hash)) return tabId;
  }
  return TAB_ORDER[0];
}

/** The first item's href in a tab — used when the user clicks a bare tab header. */
export function firstHrefForTab(tabId: string): string | undefined {
  return TAB_HREFS[tabId]?.[0];
}
