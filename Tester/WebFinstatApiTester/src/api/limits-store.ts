import { Store } from 'birko-web-core/state';

export interface Limits {
  daily:   { current: number; max: number } | null;
  monthly: { current: number; max: number } | null;
}

export const limitsStore = new Store<Limits>({ daily: null, monthly: null });

/**
 * Parse the `X-Finstat-{Daily,Monthly}-{Current,Max}` headers the backend attaches to
 * every FinStat-backed response and publish to the store. Missing or zero pairs are
 * treated as "not yet known" — we keep the previous value so the status bar doesn't
 * flicker during errors or endpoints that don't consume quota.
 */
export function readLimitsFromHeaders(headers: Headers): void {
  const dc = num(headers.get('X-Finstat-Daily-Current'));
  const dm = num(headers.get('X-Finstat-Daily-Max'));
  const mc = num(headers.get('X-Finstat-Monthly-Current'));
  const mm = num(headers.get('X-Finstat-Monthly-Max'));
  if (dm > 0) limitsStore.set('daily',   { current: dc, max: dm });
  if (mm > 0) limitsStore.set('monthly', { current: mc, max: mm });
}

function num(v: string | null): number {
  if (!v) return 0;
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
}
