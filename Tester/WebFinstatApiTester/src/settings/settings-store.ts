import { Store } from 'birko-web-core/state';

/** In-memory representation of the settings persisted to localStorage. */
export interface Settings {
  apiKey: string;
  privateKey: string;
  stationId: string;
  stationName: string;
  /** Request timeout in milliseconds. */
  timeOut: number;
  finStatApiUrl: string;
  finStatApiUrlCZ: string;
  responseType: 'xml' | 'json';
}

export const DEFAULT_SETTINGS: Settings = {
  apiKey: '',
  privateKey: '',
  stationId: 'api-tester',
  stationName: 'Api Tester Web',
  timeOut: 30000,
  finStatApiUrl: 'https://www.finstat.sk/api',
  finStatApiUrlCZ: 'https://cz.finstat.sk/api',
  responseType: 'xml',
};

const STORAGE_KEY = 'finstat_tester_settings';

function loadFromStorage(): Settings {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return { ...DEFAULT_SETTINGS };
    const parsed = JSON.parse(raw) as Partial<Settings>;
    return { ...DEFAULT_SETTINGS, ...parsed };
  } catch {
    return { ...DEFAULT_SETTINGS };
  }
}

/**
 * Signals-backed settings store. Read with `settingsStore.get('apiKey')`,
 * subscribe via `.onChange()`. Use the helpers below for writes — they
 * distinguish in-memory ("Use") from persisted ("Use and Save").
 */
export const settingsStore = new Store<Settings>(loadFromStorage());

/** Update in-memory settings (desktop tester's "Use" button). */
export function useSettings(updates: Partial<Settings>): void {
  for (const [k, v] of Object.entries(updates)) {
    settingsStore.set(k as keyof Settings, v as never);
  }
}

/** Update in-memory + persist to localStorage (desktop tester's "Use and Save" button). */
export function useAndSaveSettings(updates: Partial<Settings>): void {
  useSettings(updates);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settingsStore.snapshot()));
}

/** True when ApiKey + PrivateKey are both filled. */
export function hasCredentials(): boolean {
  return Boolean(settingsStore.get('apiKey')) && Boolean(settingsStore.get('privateKey'));
}

/** Build the X-Finstat-* header bag for the API client wrapper. */
export function settingsHeaders(): Record<string, string> {
  const s = settingsStore.snapshot();
  return {
    'X-Finstat-ApiKey':       s.apiKey,
    'X-Finstat-PrivateKey':   s.privateKey,
    'X-Finstat-StationID':    s.stationId,
    'X-Finstat-StationName':  s.stationName,
    'X-Finstat-TimeOut':      String(s.timeOut),
    'X-Finstat-Url':          s.finStatApiUrl,
    'X-Finstat-UrlCZ':        s.finStatApiUrlCZ,
    'X-Finstat-ResponseType': s.responseType,
  };
}
