import type { CallResult } from '../../api/api-client.js';

export type RowState = 'pending' | 'running' | 'ok' | 'fail';

export interface RowStatus {
  state: RowState;
  value: string;
  historyId?: string;
  error?: string;
  /** FinstatApiException.FailType enum name on failure — surfaced as a badge in the error column. */
  failType?: string;
  durationMs?: number;
  status?: number;
}

export interface BatchHandle {
  /** Resolves when every worker has drained. */
  readonly done: Promise<void>;
  cancel(): void;
  readonly cancelled: boolean;
}

/**
 * Runs `fn` over `values` with up to `concurrency` in flight at once. Calls `onUpdate`
 * whenever a row transitions state (pending → running → ok/fail). `cancel()` stops
 * dispatching new work; in-flight requests complete naturally.
 */
export function runBatch(
  values: string[],
  concurrency: number,
  fn: (value: string, index: number) => Promise<CallResult>,
  onUpdate: (index: number, status: RowStatus) => void,
): BatchHandle {
  const rows: RowStatus[] = values.map(v => ({ state: 'pending', value: v }));
  let cursor = 0;
  let cancelled = false;

  const worker = async () => {
    while (true) {
      if (cancelled) return;
      const i = cursor++;
      if (i >= values.length) return;

      rows[i] = { ...rows[i], state: 'running' };
      onUpdate(i, rows[i]);

      const start = performance.now();
      try {
        const res = await fn(values[i], i);
        const durationMs = Math.round(performance.now() - start);
        rows[i] = {
          ...rows[i],
          state: res.ok ? 'ok' : 'fail',
          historyId: res.historyId,
          error: res.ok ? undefined : (res.error ?? `HTTP ${res.status}`),
          failType: res.ok ? undefined : res.failType,
          durationMs,
          status: res.status,
        };
      } catch (e) {
        rows[i] = {
          ...rows[i],
          state: 'fail',
          error: e instanceof Error ? e.message : String(e),
          durationMs: Math.round(performance.now() - start),
        };
      }
      onUpdate(i, rows[i]);
    }
  };

  const workers = Array.from({ length: Math.max(1, Math.min(concurrency, values.length)) }, worker);
  const done = Promise.all(workers).then(() => undefined);

  return {
    done,
    cancel: () => { cancelled = true; },
    get cancelled() { return cancelled; },
  };
}
