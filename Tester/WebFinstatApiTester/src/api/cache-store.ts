import { Store } from 'birko-web-core/state';
import { settingsStore } from '../settings/settings-store.js';

/**
 * Caches response data from "producer" endpoints so other pages can offer
 * click-to-fill suggestions for their dependent inputs. SK and CZ endpoints
 * stay in separate slots because they hit distinct FinStat services.
 */

export interface MonitoringCategoryOption { category: string; name: string; }
export interface ReportingTopicOption     { id: string; name: string; group?: string; }
export interface StatementItemOption      { year: number; templates: string[]; }

interface CacheState {
  skMonitoringCategories: MonitoringCategoryOption[];
  czMonitoringCategories: MonitoringCategoryOption[];
  skReportingTopics:      ReportingTopicOption[];
  /** Statement years are keyed by the ICO that produced them — swapping ICO invalidates the hint. */
  skStatementItems:       { ico: string; items: StatementItemOption[] } | null;
  /** Diff filenames are keyed by kind (daily / ultimate / statement / statement2014). */
  skDiffFiles:            { kind: string; files: string[] } | null;
  /** Reporting filenames are keyed by topic id. */
  skReportingFiles:       { topic: string; files: string[] } | null;
}

export const cacheStore = new Store<CacheState>({
  skMonitoringCategories: [],
  czMonitoringCategories: [],
  skReportingTopics:      [],
  skStatementItems:       null,
  skDiffFiles:            null,
  skReportingFiles:       null,
});

function asArray(data: unknown): unknown[] {
  return Array.isArray(data) ? data : [];
}

function str(v: unknown): string {
  return typeof v === 'string' ? v : '';
}

function num(v: unknown): number | null {
  if (typeof v === 'number' && Number.isFinite(v)) return v;
  if (typeof v === 'string') {
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }
  return null;
}

/** Pick a string value, tolerating either camelCase or PascalCase keys. */
function pickStr(o: Record<string, unknown>, ...keys: string[]): string {
  for (const k of keys) {
    const v = o[k];
    if (typeof v === 'string' && v.length) return v;
  }
  return '';
}

function toCategory(raw: unknown): MonitoringCategoryOption | null {
  if (!raw || typeof raw !== 'object') return null;
  const o = raw as Record<string, unknown>;
  const category = pickStr(o, 'category', 'Category');
  const name     = pickStr(o, 'name', 'Name') || category;
  if (!category) return null;
  return { category, name };
}

function toTopic(raw: unknown): ReportingTopicOption | null {
  if (!raw || typeof raw !== 'object') return null;
  const o = raw as Record<string, unknown>;
  const id    = pickStr(o, 'id', 'ID', 'Id');
  const name  = pickStr(o, 'name', 'Name') || id;
  const group = pickStr(o, 'group', 'Group');
  if (!id) return null;
  return group ? { id, name, group } : { id, name };
}

function toStatementItem(raw: unknown): StatementItemOption | null {
  if (!raw || typeof raw !== 'object') return null;
  const o = raw as Record<string, unknown>;
  const year = num(o.year ?? o.Year);
  if (year === null) return null;
  const tplRaw = asArray(o.templates ?? o.Templates);
  const templates = tplRaw.map(str).filter(Boolean);
  return { year, templates };
}

export function setSkMonitoringCategories(data: unknown): void {
  const items = asArray(data).map(toCategory).filter((x): x is MonitoringCategoryOption => x !== null);
  cacheStore.set('skMonitoringCategories', items);
}

export function setCzMonitoringCategories(data: unknown): void {
  const items = asArray(data).map(toCategory).filter((x): x is MonitoringCategoryOption => x !== null);
  cacheStore.set('czMonitoringCategories', items);
}

export function setSkReportingTopics(data: unknown): void {
  const items = asArray(data).map(toTopic).filter((x): x is ReportingTopicOption => x !== null);
  cacheStore.set('skReportingTopics', items);
}

export function setSkStatementItems(ico: string, data: unknown): void {
  const items = asArray(data).map(toStatementItem).filter((x): x is StatementItemOption => x !== null);
  cacheStore.set('skStatementItems', { ico, items });
}

/** Extract fileName values from a list response, tolerating the Diff envelope (`{ files: [...] }`)
 *  and the Reporting envelope (a bare array). */
function toFileNames(data: unknown): string[] {
  const list = Array.isArray(data)
    ? data
    : (data && typeof data === 'object' ? asArray((data as Record<string, unknown>).files ?? (data as Record<string, unknown>).Files) : []);
  const out: string[] = [];
  for (const raw of list) {
    if (!raw || typeof raw !== 'object') continue;
    const name = pickStr(raw as Record<string, unknown>, 'fileName', 'FileName');
    if (name) out.push(name);
  }
  return out;
}

export function setSkDiffFiles(kind: string, data: unknown): void {
  cacheStore.set('skDiffFiles', { kind, files: toFileNames(data) });
}

export function setSkReportingFiles(topic: string, data: unknown): void {
  cacheStore.set('skReportingFiles', { topic, files: toFileNames(data) });
}

export function clearSkCache(): void {
  cacheStore.set('skMonitoringCategories', []);
  cacheStore.set('skReportingTopics',      []);
  cacheStore.set('skStatementItems',       null);
  cacheStore.set('skDiffFiles',            null);
  cacheStore.set('skReportingFiles',       null);
}

export function clearCzCache(): void {
  cacheStore.set('czMonitoringCategories', []);
}

export function clearAllCache(): void {
  clearSkCache();
  clearCzCache();
}

/**
 * Invalidate cached responses when the settings that would change the data source
 * are edited:
 *  - apiKey / privateKey swap → different account; clear everything.
 *  - finStatApiUrl swap       → different SK backend; clear SK only.
 *  - finStatApiUrlCZ swap     → different CZ backend; clear CZ only.
 * Runs at module-load so every page import picks it up without extra wiring.
 */
settingsStore.onChange('apiKey',          () => clearAllCache());
settingsStore.onChange('privateKey',      () => clearAllCache());
settingsStore.onChange('finStatApiUrl',   () => clearSkCache());
settingsStore.onChange('finStatApiUrlCZ', () => clearCzCache());
