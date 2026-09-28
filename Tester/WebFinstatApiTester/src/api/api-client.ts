import { settingsHeaders } from '../settings/settings-store.js';
import { addHistory, type ResponseHistoryItem } from '../history/response-history.js';
import { readLimitsFromHeaders } from './limits-store.js';

/**
 * Decode `X-Finstat-Request-Params` — the JSON-encoded form-POST params the C# client
 * actually sent to the real FinStat API (apiKey, StationId, Hash, ico, …). When present
 * we prefer this over the browser-originated body so the Request tab shows the true
 * outbound payload, mirroring the desktop tester's output window.
 */
function readFinstatRequestParams(headers: Headers): Record<string, string> | null {
  const raw = headers.get('X-Finstat-Request-Params');
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw);
    return parsed && typeof parsed === 'object' ? (parsed as Record<string, string>) : null;
  } catch { return null; }
}

export interface CallResult<T = unknown> {
  ok: boolean;
  status: number;
  data: T;
  rawText: string;
  contentType: string;
  /** Id of the response-history entry just added — pages can use this to select it in the viewer. */
  historyId: string;
  /** Human error summary, set when `ok === false`. */
  error?: string;
  /** FinstatApiException.FailType enum name (e.g. "LicenseExpired", "GdprRestriction"),
   *  pulled from the ProblemDetails `failType` extension. Only set on FinStat-translated
   *  failures — generic 4xx/5xx without our handler leave this undefined. */
  failType?: string;
}

export interface CallOptions {
  /** Human-readable label recorded in history (defaults to `${method} ${path}`). */
  label?: string;
  /** Parameters to record under the "Request" tab (defaults to the request body or query). */
  params?: unknown;
}

function parseBody(contentType: string, text: string): { body: unknown; isJson: boolean } {
  if (!text) return { body: null, isJson: false };
  const isJson = contentType.includes('application/json')
    || contentType.includes('application/problem+json');
  if (isJson) {
    try { return { body: JSON.parse(text), isJson: true }; }
    catch { return { body: text, isJson: false }; }
  }
  return { body: text, isJson: false };
}

/**
 * Successful FinStat endpoints wrap their result in an envelope so the browser can
 * see the raw XML / JSON body alongside the parsed object. See `FinstatEnvelope.cs`.
 */
interface Envelope {
  result: unknown;
  rawBody: string | null;
  rawContentType: string;
}

function asEnvelope(data: unknown): Envelope | null {
  if (!data || typeof data !== 'object') return null;
  const d = data as Record<string, unknown>;
  if ('result' in d && 'rawBody' in d && 'rawContentType' in d
      && typeof d.rawContentType === 'string') {
    return { result: d.result, rawBody: (d.rawBody as string | null) ?? null, rawContentType: d.rawContentType };
  }
  return null;
}

function problemMessage(data: unknown): string | undefined {
  if (!data || typeof data !== 'object') return undefined;
  const d = data as { detail?: unknown; title?: unknown };
  if (typeof d.detail === 'string') return d.detail;
  if (typeof d.title  === 'string') return d.title;
  return undefined;
}

/** ProblemDetails carries `failType` as a top-level extension (see FinstatExceptionHandler). */
function problemFailType(data: unknown): string | undefined {
  if (!data || typeof data !== 'object') return undefined;
  const d = data as { failType?: unknown };
  return typeof d.failType === 'string' ? d.failType : undefined;
}

async function request<T>(
  method: 'GET' | 'POST',
  path: string,
  body: unknown,
  opts: CallOptions,
): Promise<CallResult<T>> {
  const start = performance.now();
  const snapshot = settingsHeaders();
  const headers: Record<string, string> = { ...snapshot };
  if (method === 'POST') headers['Content-Type'] = 'application/json';

  let status = 0;
  let ok = false;
  let contentType = '';
  let rawText = '';
  let data: unknown = null;
  let error: string | undefined;
  let failType: string | undefined;
  let finstatParams: Record<string, string> | null = null;

  try {
    const resp = await fetch(path, {
      method,
      headers,
      body: method === 'POST' && body !== undefined ? JSON.stringify(body) : undefined,
    });
    status = resp.status;
    ok = resp.ok;
    contentType = resp.headers.get('content-type') ?? '';
    readLimitsFromHeaders(resp.headers);
    finstatParams = readFinstatRequestParams(resp.headers);
    rawText = await resp.text();
    data = parseBody(contentType, rawText).body;
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
  }

  if (!ok) {
    failType = problemFailType(data);
    if (!error) error = problemMessage(data) ?? (status ? `HTTP ${status}` : 'Network error');
  }

  // On success, unwrap the FinStatEnvelope so the history shows the raw XML/JSON body
  // and the true content-type (not our backend's application/json transport wrapper).
  if (ok) {
    const env = asEnvelope(data);
    if (env) {
      data = env.result;
      if (env.rawBody !== null) rawText = env.rawBody;
      contentType = env.rawContentType;
    }
  }

  const item: ResponseHistoryItem = {
    id: crypto.randomUUID(),
    timestamp: Date.now(),
    durationMs: Math.round(performance.now() - start),
    method,
    endpoint: path,
    label: opts.label ?? `${method} ${path}`,
    params: finstatParams ?? opts.params ?? (method === 'POST' ? body : null),
    status,
    ok,
    contentType,
    responseBody: data,
    responseText: rawText,
    error,
    failType,
    headers: snapshot,
  };
  addHistory(item);

  return { ok, status, data: data as T, rawText, contentType, historyId: item.id, error, failType };
}

/**
 * Typed client for the WebFinstatApiTester backend. Every call injects the
 * `X-Finstat-*` headers from the settings store and records an entry into the
 * response-history store so the sidebar / viewer can surface it.
 *
 * Pages call typed wrappers via this object — see `api/sk/*.ts` etc. (added
 * alongside each page in later tasks). A generic escape hatch is exposed for
 * ad-hoc endpoints not yet wrapped.
 */
export const api = {
  get<T = unknown>(path: string, params?: Record<string, string>, opts: CallOptions = {}) {
    let url = path;
    if (params) {
      const qs = new URLSearchParams(params).toString();
      if (qs) url += '?' + qs;
    }
    return request<T>('GET', url, undefined, opts);
  },
  post<T = unknown>(path: string, body: unknown, opts: CallOptions = {}) {
    return request<T>('POST', path, body, opts);
  },
  download,
};

/**
 * GET that streams binary bytes and triggers a browser download. Records a
 * history entry with metadata only (no binary payload in the log). Errors
 * still arrive as ProblemDetails JSON — the same pipeline as every other call.
 */
async function download(
  path: string,
  params?: Record<string, string>,
  opts: CallOptions = {},
): Promise<CallResult<{ filename: string; size: number } | null>> {
  let url = path;
  if (params) {
    const qs = new URLSearchParams(params).toString();
    if (qs) url += '?' + qs;
  }
  const start = performance.now();
  const snapshot = settingsHeaders();
  const headers: Record<string, string> = { ...snapshot };

  let status = 0;
  let ok = false;
  let contentType = '';
  let rawText = '';
  let data: unknown = null;
  let error: string | undefined;
  let failType: string | undefined;
  let filename = params?.fileName ?? path.split('/').pop() ?? 'download';
  let size = 0;
  let finstatParams: Record<string, string> | null = null;

  try {
    const resp = await fetch(url, { method: 'GET', headers });
    status = resp.status;
    ok = resp.ok;
    contentType = resp.headers.get('content-type') ?? '';
    readLimitsFromHeaders(resp.headers);
    finstatParams = readFinstatRequestParams(resp.headers);

    const disp = resp.headers.get('content-disposition') ?? '';
    const m = /filename\*?=(?:UTF-8''|["']?)([^"';\s]+)/i.exec(disp);
    if (m) filename = decodeURIComponent(m[1]);

    if (ok && !contentType.includes('json')) {
      const blob = await resp.blob();
      size = blob.size;
      const href = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = href;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(href);
      data = { filename, size };
      rawText = `<binary ${size.toLocaleString()} bytes saved as "${filename}">`;
    } else {
      rawText = await resp.text();
      data = parseBody(contentType, rawText).body;
    }
  } catch (e) {
    error = e instanceof Error ? e.message : String(e);
  }

  if (!ok) {
    failType = problemFailType(data);
    if (!error) error = problemMessage(data) ?? (status ? `HTTP ${status}` : 'Network error');
  }

  const item: ResponseHistoryItem = {
    id: crypto.randomUUID(),
    timestamp: Date.now(),
    durationMs: Math.round(performance.now() - start),
    method: 'GET',
    endpoint: url,
    label: opts.label ?? `Download ${filename}`,
    params: finstatParams ?? opts.params ?? params ?? null,
    status,
    ok,
    contentType,
    responseBody: data,
    responseText: rawText,
    error,
    failType,
    headers: snapshot,
  };
  addHistory(item);

  return { ok, status, data: data as { filename: string; size: number } | null, rawText, contentType, historyId: item.id, error, failType };
}
