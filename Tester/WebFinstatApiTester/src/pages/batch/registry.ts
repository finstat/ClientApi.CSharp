import type { CallResult } from '../../api/api-client.js';
import { skDetail } from '../../api/sk-detail.js';
import { czDetail } from '../../api/cz-detail.js';
import { skAuto } from '../../api/sk-auto.js';
import { czAuto } from '../../api/cz-auto.js';
import { skMonitoring } from '../../api/sk-monitoring.js';
import { czMonitoring } from '../../api/cz-monitoring.js';
import { skStatement } from '../../api/sk-statement.js';
import { skDistraint } from '../../api/sk-distraint.js';
import { skDiff } from '../../api/sk-diff.js';
import { skReporting } from '../../api/sk-reporting.js';
import { cacheStore, type MonitoringCategoryOption } from '../../api/cache-store.js';
import { t } from '../../i18n/index.js';

export interface ExtraOption { value: string; label: string; }

export interface ExtraField {
  name: string;
  label: string;
  placeholder?: string;
  /** When present, the batch runner renders a `<b-select>` populated from these
   *  options (typically sourced from the shared cache store) instead of a free-
   *  text input. Re-read on every render so options reflect the latest cache. */
  source?: () => ExtraOption[];
  /** Key in the cache store whose changes should trigger a re-render of the batch
   *  form so the select picks up fresh options. Optional when `source` is static. */
  cacheKey?: 'skMonitoringCategories' | 'czMonitoringCategories';
}

export interface BatchEndpoint {
  id: string;
  group: string;
  label: string;
  inputLabel: string;
  placeholder: string;
  extras?: ExtraField[];
  run: (value: string, extras: Record<string, string>) => Promise<CallResult>;
}

function categoryOptions(cats: readonly MonitoringCategoryOption[]): ExtraOption[] {
  return [
    { value: '', label: t('common.field.defaultCategory') },
    ...cats.map(c => ({ value: c.category, label: c.name })),
  ];
}

const SK_CATEGORY_EXTRA = (): ExtraField => ({
  name: 'category',
  label: t('common.field.categoryOpt'),
  source: () => categoryOptions(cacheStore.get('skMonitoringCategories')),
  cacheKey: 'skMonitoringCategories',
});

const CZ_CATEGORY_EXTRA = (): ExtraField => ({
  name: 'category',
  label: t('common.field.categoryOpt'),
  source: () => categoryOptions(cacheStore.get('czMonitoringCategories')),
  cacheKey: 'czMonitoringCategories',
});

/**
 * Every entry here gets listed in the Batch tab's endpoint dropdown. Labels are
 * re-read from the i18n layer on each call so language switching is live.
 */
export function batchEndpoints(): BatchEndpoint[] {
  const ICO = t('batchInput.ico');
  const ICO_CZ = t('batchInput.icoCz');
  const FN = t('batchInput.filename');
  const Q = t('batchInput.query');
  const DATE = t('batchInput.date');

  const skGroup = t('batchGroup.skDetail');
  const czGroup = t('batchGroup.czDetail');
  const skMon = t('batchGroup.skMonitoring');
  const skMonDate = t('batchGroup.skMonitoringDate');
  const czMon = t('batchGroup.czMonitoring');
  const czMonDate = t('batchGroup.czMonitoringDate');
  const skOther = t('batchGroup.skOther');
  const czOther = t('batchGroup.czOther');
  const skRep = t('batchGroup.skReporting');
  const skSt = t('batchGroup.skStatement');
  const skDi = t('batchGroup.skDiff');
  const skDis = t('batchGroup.skDistraint');

  return [
    // ── SK Detail ───────────────────────────────────────────────────────────
    { id: 'sk-basic',    group: skGroup, label: t('ribbon.item.basic'),    inputLabel: ICO, placeholder: '35757442',
      run: (ico) => skDetail.basic(ico) },
    { id: 'sk-detail',   group: skGroup, label: t('ribbon.item.detail'),   inputLabel: ICO, placeholder: '35757442',
      run: (ico) => skDetail.detail(ico) },
    { id: 'sk-extended', group: skGroup, label: t('ribbon.item.extended'), inputLabel: ICO, placeholder: '35757442',
      run: (ico) => skDetail.extended(ico) },
    { id: 'sk-ultimate', group: skGroup, label: t('ribbon.item.ultimate'), inputLabel: ICO, placeholder: '35757442',
      run: (ico) => skDetail.ultimate(ico) },

    // ── CZ Detail ───────────────────────────────────────────────────────────
    { id: 'cz-basic',   group: czGroup, label: t('ribbon.item.basic'),   inputLabel: ICO_CZ, placeholder: '27074358',
      run: (ico) => czDetail.basic(ico) },
    { id: 'cz-detail',  group: czGroup, label: t('ribbon.item.detail'),  inputLabel: ICO_CZ, placeholder: '27074358',
      run: (ico) => czDetail.detail(ico) },
    { id: 'cz-premium', group: czGroup, label: t('ribbon.item.premium'), inputLabel: ICO_CZ, placeholder: '27074358',
      run: (ico) => czDetail.premium(ico) },
    { id: 'cz-elite',   group: czGroup, label: t('ribbon.item.elite'),   inputLabel: ICO_CZ, placeholder: '27074358',
      run: (ico) => czDetail.elite(ico) },
    { id: 'cz-ultimate', group: czGroup, label: t('ribbon.item.ultimate'), inputLabel: ICO_CZ, placeholder: '27074358',
      run: (ico) => czDetail.ultimate(ico) },

    // ── Monitoring (SK + CZ) — ICO with optional shared category ───────────
    { id: 'sk-mon-add',    group: skMon, label: t('ribbon.item.add'),    inputLabel: ICO, placeholder: '35757442',
      extras: [SK_CATEGORY_EXTRA()], run: (ico, x) => skMonitoring.add(ico, x.category || undefined) },
    { id: 'sk-mon-remove', group: skMon, label: t('ribbon.item.remove'), inputLabel: ICO, placeholder: '35757442',
      extras: [SK_CATEGORY_EXTRA()], run: (ico, x) => skMonitoring.remove(ico, x.category || undefined) },
    { id: 'cz-mon-add',    group: czMon, label: t('ribbon.item.add'),    inputLabel: ICO_CZ, placeholder: '27074358',
      extras: [CZ_CATEGORY_EXTRA()], run: (ico, x) => czMonitoring.add(ico, x.category || undefined) },
    { id: 'cz-mon-remove', group: czMon, label: t('ribbon.item.remove'), inputLabel: ICO_CZ, placeholder: '27074358',
      extras: [CZ_CATEGORY_EXTRA()], run: (ico, x) => czMonitoring.remove(ico, x.category || undefined) },

    // ── SK Monitoring Date ──────────────────────────────────────────────────
    { id: 'sk-mon-date-add',    group: skMonDate, label: t('ribbon.item.add'),    inputLabel: DATE, placeholder: '2026-04-20',
      extras: [SK_CATEGORY_EXTRA()], run: (date, x) => skMonitoring.date.add(date, x.category || undefined) },
    { id: 'sk-mon-date-remove', group: skMonDate, label: t('ribbon.item.remove'), inputLabel: DATE, placeholder: '2026-04-20',
      extras: [SK_CATEGORY_EXTRA()], run: (date, x) => skMonitoring.date.remove(date, x.category || undefined) },

    // ── CZ Monitoring Date ──────────────────────────────────────────────────
    { id: 'cz-mon-date-add',    group: czMonDate, label: t('ribbon.item.add'),    inputLabel: DATE, placeholder: '2026-04-20',
      extras: [CZ_CATEGORY_EXTRA()], run: (date, x) => czMonitoring.date.add(date, x.category || undefined) },
    { id: 'cz-mon-date-remove', group: czMonDate, label: t('ribbon.item.remove'), inputLabel: DATE, placeholder: '2026-04-20',
      extras: [CZ_CATEGORY_EXTRA()], run: (date, x) => czMonitoring.date.remove(date, x.category || undefined) },

    // ── AutoComplete ────────────────────────────────────────────────────────
    { id: 'sk-autocomplete', group: skOther, label: t('ribbon.item.autoComplete'), inputLabel: Q, placeholder: 'finstat',
      run: (q) => skAuto.autocomplete(q) },
    { id: 'cz-autocomplete', group: czOther, label: t('ribbon.item.autoComplete'), inputLabel: Q, placeholder: 'finstat',
      run: (q) => czAuto.autocomplete(q) },

    // ── Statement / Distraint ───────────────────────────────────────────────
    { id: 'sk-statement-list',   group: skSt,  label: t('ribbon.item.list'),         inputLabel: ICO, placeholder: '35757442',
      run: (ico) => skStatement.list(ico) },
    { id: 'sk-distraint-stored', group: skDis, label: t('ribbon.item.storedDetail'), inputLabel: t('common.field.id'), placeholder: 'stored-entry-id',
      run: (id)  => skDistraint.storedDetail(id) },

    // ── Downloads ───────────────────────────────────────────────────────────
    { id: 'sk-diff-daily-download',         group: skDi, label: t('ribbon.group.daily'),             inputLabel: FN, placeholder: 'diff_2024-01-01.zip',
      run: (fn) => skDiff.download('daily', fn) },
    { id: 'sk-diff-ultimate-download',      group: skDi, label: t('ribbon.item.ultimate'),           inputLabel: FN, placeholder: 'ultimate_2024-01-01.zip',
      run: (fn) => skDiff.download('ultimate', fn) },
    { id: 'sk-diff-statement-download',     group: skDi, label: t('ribbon.group.diffStatement'),     inputLabel: FN, placeholder: 'statement_2024-01-01.zip',
      run: (fn) => skDiff.download('statement', fn) },
    { id: 'sk-diff-statement2014-download', group: skDi, label: t('ribbon.group.diffStatement2014'), inputLabel: FN, placeholder: 'statement2014_2024-01-01.zip',
      run: (fn) => skDiff.download('statement2014', fn) },
    { id: 'sk-reporting-download',          group: skRep, label: t('ribbon.item.download'),          inputLabel: FN, placeholder: 'report.csv',
      run: (fn) => skReporting.download(fn) },
  ];
}

/** Legacy alias — kept for the side-effect import in batch-runner.ts. */
export const BATCH_ENDPOINTS = batchEndpoints;

export function findBatchEndpoint(id: string): BatchEndpoint | undefined {
  return batchEndpoints().find(e => e.id === id);
}

/** Options formatted for `<b-select>`. */
export function batchEndpointOptions(): { value: string; label: string }[] {
  return batchEndpoints().map(e => ({ value: e.id, label: t('batch.entryLabel', { group: e.group, label: e.label }) }));
}

/**
 * Map the current hash to a batch endpoint id. Used by the FAB to pre-select
 * the most relevant endpoint based on the page the user is viewing.
 */
export function resolveBatchFromHash(hash: string): string | null {
  const h = hash.startsWith('#') ? hash.slice(1) : hash;

  // SK/CZ Detail tiers — `/sk/detail/basic` → `sk-basic`.
  let m = h.match(/^\/(sk|cz)\/detail\/(\w+)$/);
  if (m) return `${m[1]}-${m[2]}`;

  // Monitoring Add/Remove (SK + CZ).
  m = h.match(/^\/(sk|cz)\/monitoring\/(add|remove)$/);
  if (m) return `${m[1]}-mon-${m[2]}`;

  // SK + CZ Monitoring Date Add/Remove.
  m = h.match(/^\/(sk|cz)\/monitoring\/date\/(add|remove)$/);
  if (m) return `${m[1]}-mon-date-${m[2]}`;

  // AutoComplete (SK + CZ).
  m = h.match(/^\/(sk|cz)\/autocomplete$/);
  if (m) return `${m[1]}-autocomplete`;

  // Daily Diff downloads.
  m = h.match(/^\/sk\/diff\/(daily|ultimate|statement|statement2014)\/download$/);
  if (m) return `sk-diff-${m[1]}-download`;

  if (h === '/sk/statement/list')          return 'sk-statement-list';
  if (h === '/sk/distraint/stored-detail') return 'sk-distraint-stored';
  if (h === '/sk/reporting/download')      return 'sk-reporting-download';

  return null;
}
