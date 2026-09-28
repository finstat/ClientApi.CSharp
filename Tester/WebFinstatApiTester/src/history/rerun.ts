import { api, type CallResult } from '../api/api-client.js';
import type { ResponseHistoryItem } from './response-history.js';

/**
 * Re-fires the exact call recorded in a history entry using the current settings.
 * Mirrors DesktopFinstatApiTester's "Repeat request" context menu.
 *
 * - Endpoints ending in `/download` go through `api.download()` so the file triggers
 *   a browser save instead of being parsed as text.
 * - Everything else uses `api.post`/`api.get` with the recorded params.
 */
export function rerunHistoryItem(item: ResponseHistoryItem): Promise<CallResult> {
  const isDownload = item.endpoint.endsWith('/download') || item.endpoint.includes('/download?');
  const pathOnly   = item.endpoint.split('?')[0];

  if (isDownload) {
    const p = item.params as Record<string, string> | null;
    const fileName = (p && typeof p.fileName === 'string') ? p.fileName : '';
    return api.download(pathOnly, fileName ? { fileName } : undefined, {
      label: `Rerun: ${item.label}`,
      params: item.params,
    });
  }

  if (item.method === 'POST') {
    return api.post(pathOnly, item.params, { label: `Rerun: ${item.label}`, params: item.params });
  }

  // GET — the params are in the query string of `endpoint`; pull them back into a dict.
  const url = new URL(item.endpoint, window.location.origin);
  const queryParams: Record<string, string> = {};
  url.searchParams.forEach((v, k) => (queryParams[k] = v));
  return api.get(url.pathname.replace(/^\//, ''), queryParams, {
    label: `Rerun: ${item.label}`,
    params: item.params,
  });
}
