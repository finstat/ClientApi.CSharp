/**
 * Lightweight prefill store used by the history viewer's "Open action" button.
 * Stashes the params of a past API call so the target page can pre-populate its
 * inputs on the next mount. A single pending entry is kept; it survives a page
 * reload via sessionStorage, and is consumed on first read so navigation-by-URL
 * without a pending value doesn't accidentally reuse stale data.
 *
 * Pages that want to pick up the prefill call `applyPendingPrefill(this.shadowRoot)`
 * from `onMount` — helper lives in page-helpers.ts so existing pages only need
 * to add a single import + one call.
 */

const STORAGE_KEY = 'finstat-pending-prefill';

export interface PendingPrefill {
  /** Target hash route, e.g. '#/sk/detail/basic'. Used to scope the apply so an
   *  unrelated page doesn't pick up a prefill intended for a different target. */
  route: string;
  /** Flat map of param name → value. Matched against inputs by element id. */
  params: Record<string, unknown>;
}

let pending: PendingPrefill | null = null;

export function setPrefill(route: string, params: Record<string, unknown>): void {
  pending = { route, params };
  try { sessionStorage.setItem(STORAGE_KEY, JSON.stringify(pending)); } catch { /* full / disabled */ }
}

/**
 * Consume the pending prefill for the given route. Returns `null` when nothing
 * is pending or the route doesn't match. Always clears the pending entry on
 * match so a later page load starts fresh.
 */
export function consumePrefill(currentRoute: string): Record<string, unknown> | null {
  if (!pending) {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (raw) pending = JSON.parse(raw) as PendingPrefill;
    } catch { pending = null; }
  }
  if (!pending) return null;
  if (pending.route !== currentRoute) return null;
  const params = pending.params;
  pending = null;
  try { sessionStorage.removeItem(STORAGE_KEY); } catch { /* ignore */ }
  return params;
}
