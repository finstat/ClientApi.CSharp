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
}

interface HistoryState {
  items: ResponseHistoryItem[];
  selectedId: string | null;
}

export const historyStore = new Store<HistoryState>({
  items: [],
  selectedId: null,
});

/** Prepend a new item (desktop inserts at index 0). */
export function addHistory(item: ResponseHistoryItem): void {
  historyStore.set('items', [item, ...historyStore.get('items')]);
  historyStore.set('selectedId', item.id);
}

export function removeHistory(id: string): void {
  historyStore.set('items', historyStore.get('items').filter(i => i.id !== id));
  if (historyStore.get('selectedId') === id) historyStore.set('selectedId', null);
}

export function clearHistory(): void {
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
