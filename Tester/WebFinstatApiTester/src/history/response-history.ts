import { Store } from 'birko-web-core/state';

/** One row in the response history (desktop's `ResponseItem` equivalent). */
export interface ResponseHistoryItem {
  id: string;
  timestamp: number;
  durationMs: number;
  method: 'GET' | 'POST';
  /** Endpoint path, e.g. `api/sk/detail/basic`. */
  endpoint: string;
  /** Human label shown in the sidebar, e.g. `SK Detail Basic`. */
  label: string;
  /** Parameters recorded for the Request pane. */
  params: unknown;
  status: number;
  ok: boolean;
  contentType: string;
  /** Parsed JSON tree, or the raw text when the response isn't JSON. */
  responseBody: unknown;
  /** Raw response text — used by the "Raw" tab. */
  responseText: string;
  /** Populated when the call failed — message from ProblemDetails or network error. */
  error?: string;
  /** FinstatApiException.FailType enum name when the backend classified the failure
   *  (e.g. "LicenseExpired", "GdprRestriction"). Drives the localized status badge
   *  in the response viewer and the failure toast. */
  failType?: string;
  /** Snapshot of the `X-Finstat-*` headers sent with this request. Lets the
   *  "Curl" button rebuild commands against the settings in effect at the time
   *  of the call, not whatever is in the Settings modal now. */
  headers?: Record<string, string>;
  /** Per-call response language override (sk|cs), replayed by Rerun; absent when the Settings value applied. */
  lang?: string;
  /** Environment profile the call went to (name, plus the port when one was set). */
  profile?: string;
  /** The comparison this call was made by; the sidebar lists it under that comparison. */
  groupId?: string;
}

/** One run of the compare page; the sidebar entry that leads back to its results. */
export interface CompareSession {
  id: string;
  timestamp: number;
  label: string;
  /** The compared request, kept here so the comparison survives removing its history entry. */
  request: ResponseHistoryItem;
  /** The compare page's setup and results, owned by the compare page. */
  state: unknown;
}

interface HistoryState {
  items: ResponseHistoryItem[];
  compares: CompareSession[];
  selectedId: string | null;
}

export const historyStore = new Store<HistoryState>({
  items: [],
  compares: [],
  selectedId: null,
});

/** Prepend a new item (desktop inserts at index 0). */
export function addHistory(item: ResponseHistoryItem): void {
  historyStore.set('items', [item, ...historyStore.get('items')]);
  historyStore.set('selectedId', item.id);
}

export function addCompare(session: CompareSession): void {
  historyStore.set('compares', [session, ...historyStore.get('compares')]);
}

export function findCompare(id: string): CompareSession | null {
  return historyStore.get('compares').find(c => c.id === id) ?? null;
}

export function removeHistory(id: string): void {
  historyStore.set('items', historyStore.get('items').filter(i => i.id !== id));
  if (historyStore.get('selectedId') === id) historyStore.set('selectedId', null);
}

export function clearHistory(): void {
  historyStore.set('compares', []);
  historyStore.set('items', []);
  historyStore.set('selectedId', null);
}

export function selectHistory(id: string | null): void {
  historyStore.set('selectedId', id);
}

export function getSelected(): ResponseHistoryItem | null {
  const id = historyStore.get('selectedId');
  if (!id) return null;
  return historyStore.get('items').find(i => i.id === id) ?? null;
}
