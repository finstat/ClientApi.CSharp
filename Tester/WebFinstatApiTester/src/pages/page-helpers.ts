import { toast } from 'birko-web-components';
import type { CallResult } from '../api/api-client.js';
import { hasCredentials } from '../settings/settings-store.js';
import { OPEN_SETTINGS_EVENT } from '../shell/app-shell.js';
import { LOADING_END_EVENT, LOADING_START_EVENT } from '../shell/loading-overlay.js';
import { consumePrefill } from './prefill.js';
import { t } from '../i18n/index.js';

/**
 * Resolve a FinstatApiException.FailType enum name to a localized label.
 * Unknown enum names (forward-compat with new server values) fall through to
 * the raw enum name so the user still sees *something* meaningful.
 */
export function failTypeLabel(failType: string | undefined): string | undefined {
  if (!failType) return undefined;
  const key = `common.failType.${failType}`;
  const translated = t(key);
  return translated === key ? failType : translated;
}

/**
 * Runs an endpoint call with the standard UX chrome:
 *  - Prompts for credentials (opens Settings) if the ApiKey/PrivateKey are empty.
 *  - Toasts success / failure (label is expected to be already-translated).
 *  - Navigates to `#/history/<id>` so the response viewer opens on the fresh entry.
 *
 * `silent: true` is for background prefills (e.g. cache-hint loads): keeps the
 * loading overlay and failure toast but suppresses the success toast and the
 * history-hash redirect, so the user stays on the page they triggered it from.
 */
export async function runApiCall(
  label: string,
  fn: () => Promise<CallResult>,
  opts: { silent?: boolean } = {},
): Promise<CallResult | null> {
  if (!hasCredentials()) {
    toast.warning(t('settings.toast.missingCreds'));
    window.dispatchEvent(new CustomEvent(OPEN_SETTINGS_EVENT));
    return null;
  }

  const loadingId = crypto.randomUUID();
  window.dispatchEvent(new CustomEvent(LOADING_START_EVENT, { detail: { id: loadingId, label } }));
  let res: CallResult;
  try { res = await fn(); }
  finally { window.dispatchEvent(new CustomEvent(LOADING_END_EVENT, { detail: { id: loadingId } })); }

  if (res.ok) { if (!opts.silent) toast.success(t('common.callOk', { label })); }
  else {
    const error    = res.error ?? t('common.httpStatus', { status: res.status });
    const failType = failTypeLabel(res.failType);
    toast.error(failType
      ? t('common.callFailedTyped', { label, failType, error })
      : t('common.callFailed', { label, error }));
  }
  if (!opts.silent) window.location.hash = `#/history/${res.historyId}`;
  return res;
}

/**
 * Split the path after `prefix` from the current (or given) hash into segments.
 * Example: `parseRouteSegments('#/sk/diff/', '#/sk/diff/daily/list')` → `['daily', 'list']`.
 * Pages use this to discover the ribbon-selected action from the URL.
 */
export function parseRouteSegments(prefix: string, hash: string = window.location.hash): string[] {
  if (!hash.startsWith(prefix)) return [];
  return hash.slice(prefix.length).split('/').filter(Boolean);
}

/**
 * Mark the button matching `action` as emphasised (cleared from any prior target) and
 * optionally focus a nearby input. Matches `[data-action]` and `[data-kind]` so it works
 * on pages using either convention. Pair with CSS like `b-button[data-emphasis] { ... }`.
 */
export function emphasizeAction(
  root: ShadowRoot | null | undefined,
  action: string | undefined,
  opts: { focus?: string } = {},
): void {
  if (!root) return;
  root.querySelectorAll('[data-emphasis]').forEach(el => el.removeAttribute('data-emphasis'));
  if (!action) return;
  const btn = root.querySelector(
    `[data-action="${CSS.escape(action)}"], [data-kind="${CSS.escape(action)}"]`,
  ) as HTMLElement | null;
  if (!btn) return;
  btn.setAttribute('data-emphasis', '');
  btn.scrollIntoView({ block: 'nearest' });
  if (opts.focus) {
    const input = root.querySelector<HTMLElement>(opts.focus);
    input?.focus();
  }
}

/**
 * Inline affordance shown next to a cache-backed select when the cache is empty —
 * a single ghost button that fires the producer endpoint silently (no toast/redirect).
 * Drop the returned HTML into render() right below the .row, and call `toggleCacheHint`
 * from the same place you already populate the select from the cache.
 */
export function cacheHintMarkup(opts: { id: string; action: string; label: string }): string {
  return `
      <div class="cache-hint" id="${opts.id}" hidden>
        <b-button variant="ghost" data-action="${opts.action}">${opts.label}</b-button>
      </div>`;
}

/** Show the hint (remove `hidden`) when the cache is empty; hide it when populated. */
export function toggleCacheHint(root: ShadowRoot | null | undefined, id: string, hasItems: boolean): void {
  const el = root?.getElementById(id);
  if (el) el.toggleAttribute('hidden', hasItems);
}

/**
 * If a history "Open action" click queued a prefill for the current route,
 * pop it and apply each param to an input whose element id matches the param
 * name (e.g. `{ ico: "35757442" }` fills `#ico`). Silently ignores:
 *  - null / undefined / empty-string values,
 *  - params for which no input with a matching id exists,
 *  - non-matching routes (the prefill stays queued for its target).
 *
 * Pages opt in with a single call from `onMount`. Works against both b-input
 * and b-select — both expose a writable `value` property.
 */
export function applyPendingPrefill(root: ShadowRoot | null | undefined): void {
  if (!root) return;
  const hash = window.location.hash.split('?')[0];
  const params = consumePrefill(hash);
  if (!params) return;
  for (const [key, raw] of Object.entries(params)) {
    if (raw === null || raw === undefined || raw === '') continue;
    const el = root.querySelector<HTMLElement & { value?: unknown }>(`#${CSS.escape(key)}`);
    if (!el) continue;
    const value = String(raw);
    if ('value' in el) {
      (el as unknown as { value: string }).value = value;
      // Also reflect as attribute — b-select reads `value` attribute during render().
      el.setAttribute('value', value);
    }
  }
}
