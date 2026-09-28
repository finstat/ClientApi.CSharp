import { I18n, useI18n } from 'birko-web-core/i18n';

import { en } from './messages-en.js';
import { sk } from './messages-sk.js';

/** Supported locales — extend this union if we add more bundles. */
export type Locale = 'en' | 'sk';
export const LOCALES: readonly Locale[] = ['en', 'sk'];
export const LOCALE_LABELS: Record<Locale, string> = {
  en: 'English',
  sk: 'Slovenčina',
};

/**
 * Country-flag icons for each locale, as HTML entity pairs. Render as real flag
 * glyphs on macOS / iOS / Android / modern Linux; fall back to letter pairs
 * (e.g. "GB", "SK") on Windows where the emoji font lacks flag composition.
 */
export const LOCALE_ICONS: Record<Locale, string> = {
  en: '&#127468;&#127463;', // 🇬🇧
  sk: '&#127480;&#127472;', // 🇸🇰
};

const STORAGE_KEY = 'finstat_tester_locale';

function readPersistedLocale(): Locale {
  try {
    const v = localStorage.getItem(STORAGE_KEY);
    return v === 'sk' || v === 'en' ? v : 'en';
  } catch { return 'en'; }
}

/**
 * Shared I18n singleton. Bundles for en + sk are injected up-front via
 * `addMessages()` — no async JSON fetch needed. Persists locale choice to
 * localStorage under `finstat_tester_locale`.
 */
export const i18n = new I18n({
  defaultLocale: readPersistedLocale(),
  fallbackLocale: 'en',
  storageKey: STORAGE_KEY,
});

i18n.addMessages('en', en);
i18n.addMessages('sk', sk);

// Register this instance as the Birko.Web global singleton so components
// (b-command-palette, b-ribbon, pagination, etc.) read from our bundle via
// `bwc.*` keys without per-instance label-* attribute wiring.
useI18n(i18n);

document.documentElement.setAttribute('lang', i18n.locale);

/** Short translator — prefer this over calling `i18n.t` directly. */
export function t(key: string, params?: Record<string, string | number>): string {
  return i18n.t(key, params);
}

/** Persist + switch locale and notify subscribers. `setLocale` is fire-and-forget
 *  (the bundle is already in memory; the async JSON fetch inside is a silent no-op). */
export function setLocale(locale: Locale): void {
  void i18n.setLocale(locale);
}

export function getLocale(): Locale {
  return i18n.locale as Locale;
}

/** Subscribe to locale changes — returns unsubscribe. */
export function onLocaleChange(fn: (locale: Locale) => void): () => void {
  return i18n.onLocaleChange(l => fn(l as Locale));
}
