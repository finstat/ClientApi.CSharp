import { settingsHeaders } from '../settings/settings-store.js';

export interface CurlResult { commands: string[]; }

/**
 * POSTs the original endpoint + params to `/api/_curl`; backend maps it to the FinStat
 * endpoint, computes the signed hash, and returns three curl commands (GET query string,
 * POST form, POST JSON). Does not record into response history — this is a meta call.
 *
 * `headers` overrides the current settings snapshot so the history viewer can
 * reconstruct curl commands against the credentials/URLs in effect at request time,
 * not whatever the Settings modal holds now.
 */
export async function buildCurl(
  endpoint: string,
  params: unknown,
  headers?: Record<string, string>,
): Promise<CurlResult | null> {
  const resp = await fetch('api/_curl', {
    method: 'POST',
    headers: { ...(headers ?? settingsHeaders()), 'Content-Type': 'application/json' },
    body: JSON.stringify({ endpoint, params }),
  });
  if (!resp.ok) return null;
  return resp.json();
}
