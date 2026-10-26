import { Store } from 'birko-web-core/state';

/** Marks a profile wherever the active one is shown, so production is not called by mistake; '' = none. */
export type ProfileColor = '' | 'green' | 'amber' | 'red';

export const PROFILE_COLORS: readonly ProfileColor[] = ['', 'green', 'amber', 'red'];

/** The theme colour of a profile colour; '' for none. */
export function profileColorCss(color: ProfileColor | undefined): string {
  switch (color) {
    case 'green': return 'var(--b-color-success)';
    case 'amber': return 'var(--b-color-warning)';
    case 'red':   return 'var(--b-color-danger)';
    default:      return '';
  }
}

/**
 * A named FinStat environment: the SK and the CZ API host the tester calls, one port for both,
 * and optional API keys that replace the global ones.
 */
export interface EnvProfile {
  id: string;
  name: string;
  /** Built-in profiles keep their name and hosts; only the port, the keys and the colour can change. */
  builtIn: boolean;
  skHost: string;
  czHost: string;
  /** null = the default HTTPS port. */
  port: number | null;
  /** '' = use the global key. */
  apiKey: string;
  privateKey: string;
  color: ProfileColor;
}

export const BUILT_IN_PROFILES: readonly EnvProfile[] = [
  builtIn('finstat-sk',   'finstat.sk',   'www.finstat.sk',  'cz.finstat.sk', 'red'),
  builtIn('finstat-cz',   'finstat.cz',   'sk.finstat.cz',   'www.finstat.cz', 'red'),
  builtIn('zdrojak-eu',   'zdrojak.eu',   'zdrojak.eu',      'cz.zdrojak.eu'),
  builtIn('zdrojakcz-eu', 'zdrojakcz.eu', 'sk.zdrojakcz.eu', 'zdrojakcz.eu'),
];

function builtIn(id: string, name: string, skHost: string, czHost: string, color: ProfileColor = ''): EnvProfile {
  return { id, name, builtIn: true, skHost, czHost, port: null, apiKey: '', privateKey: '', color };
}

export function profileUrl(host: string, port: number | null): string {
  return `https://${host}${port ? `:${port}` : ''}/api`;
}

/** In-memory representation of the settings persisted to localStorage. */
export interface Settings {
  /** Global keys, used by every profile that does not set its own. */
  apiKey: string;
  privateKey: string;
  stationId: string;
  stationName: string;
  /** Request timeout in milliseconds. */
  timeOut: number;
  profiles: EnvProfile[];
  activeProfileId: string;
  /** Derived from the active profile; kept in the store so URL subscribers see profile switches. */
  finStatApiUrl: string;
  finStatApiUrlCZ: string;
  responseType: 'xml' | 'json';
  /** FinStat response text language; '' = server default. Honoured on finstat.cz only. */
  lang: '' | 'sk' | 'cs';
}

export const DEFAULT_SETTINGS: Settings = withUrls({
  apiKey: '',
  privateKey: '',
  stationId: 'api-tester',
  stationName: 'Api Tester Web',
  timeOut: 30000,
  profiles: BUILT_IN_PROFILES.map(p => ({ ...p })),
  activeProfileId: BUILT_IN_PROFILES[0].id,
  finStatApiUrl: '',
  finStatApiUrlCZ: '',
  responseType: 'xml',
  lang: '',
});

const STORAGE_KEY = 'finstat_tester_settings';

export function findProfile(profiles: readonly EnvProfile[], id: string): EnvProfile {
  return profiles.find(p => p.id === id) ?? profiles[0];
}

function withUrls(s: Settings): Settings {
  const p = findProfile(s.profiles, s.activeProfileId);
  return { ...s, activeProfileId: p.id, finStatApiUrl: profileUrl(p.skHost, p.port), finStatApiUrlCZ: profileUrl(p.czHost, p.port) };
}

/**
 * Built-ins always come from code (a stored copy only keeps its port, keys and colour), custom profiles
 * as stored. Profiles saved before colours existed get the built-in's colour, a custom one none.
 */
function mergeProfiles(stored: EnvProfile[] | undefined): EnvProfile[] {
  const builtIns = BUILT_IN_PROFILES.map(p => {
    const s = stored?.find(x => x.id === p.id);
    return { ...p, port: s?.port ?? null, apiKey: s?.apiKey ?? '', privateKey: s?.privateKey ?? '', color: validColor(s?.color) ?? p.color };
  });
  const custom = (stored ?? []).filter(p => !BUILT_IN_PROFILES.some(b => b.id === p.id)).map(p => ({ ...p, builtIn: false, color: validColor(p.color) ?? '' }));
  return [...builtIns, ...custom];
}

function validColor(color: unknown): ProfileColor | undefined {
  return PROFILE_COLORS.find(c => c === color);
}

/** Settings saved before profiles existed hold two URLs: pick the built-in they point at, or keep them as a custom profile. */
function migrateUrls(sk: string | undefined, cz: string | undefined): { profiles: EnvProfile[]; activeProfileId: string } {
  const profiles = mergeProfiles(undefined);
  const a = parseUrl(sk);
  const b = parseUrl(cz);
  if (!a || !b) return { profiles, activeProfileId: profiles[0].id };
  const port = a.port ?? b.port;
  const match = profiles.find(p => sameHost(p.skHost, a.host) && sameHost(p.czHost, b.host));
  if (match) {
    match.port = port;
    return { profiles, activeProfileId: match.id };
  }
  const custom: EnvProfile = { id: crypto.randomUUID(), name: a.host, builtIn: false, skHost: a.host, czHost: b.host, port, apiKey: '', privateKey: '', color: '' };
  return { profiles: [...profiles, custom], activeProfileId: custom.id };
}

// finstat.sk and www.finstat.sk are the same server
function sameHost(x: string, y: string): boolean {
  const bare = (h: string) => h.toLowerCase().replace(/^www\./, '');
  return bare(x) === bare(y);
}

function parseUrl(url: string | undefined): { host: string; port: number | null } | null {
  if (!url) return null;
  try {
    const u = new URL(url.includes('://') ? url : `https://${url}`);
    return { host: u.hostname, port: u.port ? Number(u.port) : null };
  } catch {
    return null;
  }
}

function loadFromStorage(): Settings {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return { ...DEFAULT_SETTINGS };
    const parsed = JSON.parse(raw) as Partial<Settings>;
    const profiles = parsed.profiles
      ? { profiles: mergeProfiles(parsed.profiles), activeProfileId: parsed.activeProfileId ?? DEFAULT_SETTINGS.activeProfileId }
      : migrateUrls(parsed.finStatApiUrl, parsed.finStatApiUrlCZ);
    return withUrls({ ...DEFAULT_SETTINGS, ...parsed, ...profiles });
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

/** Update in-memory settings (desktop tester's "Use" button). The URLs follow the active profile. */
export function useSettings(updates: Partial<Settings>): void {
  const merged = { ...settingsStore.snapshot(), ...updates };
  const next = withUrls(updates.profiles ? { ...merged, profiles: mergeProfiles(updates.profiles) } : merged);
  for (const [k, v] of Object.entries(next)) {
    settingsStore.set(k as keyof Settings, v as never);
  }
}

/** Update in-memory + persist to localStorage (desktop tester's "Use and Save" button). */
export function useAndSaveSettings(updates: Partial<Settings>): void {
  useSettings(updates);
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settingsStore.snapshot()));
}

export function activeProfile(): EnvProfile {
  return findProfile(settingsStore.get('profiles'), settingsStore.get('activeProfileId'));
}

/** True when the active profile has an API Key + Private Key, its own or the global one. */
export function hasCredentials(): boolean {
  const h = settingsHeaders();
  return Boolean(h['X-Finstat-ApiKey']) && Boolean(h['X-Finstat-PrivateKey']);
}

/** Build the X-Finstat-* header bag for the API client wrapper, for the active profile or the given one. */
export function settingsHeaders(profile: EnvProfile = activeProfile()): Record<string, string> {
  const s = settingsStore.snapshot();
  return {
    'X-Finstat-ApiKey':       profile.apiKey || s.apiKey,
    'X-Finstat-PrivateKey':   profile.privateKey || s.privateKey,
    'X-Finstat-StationID':    s.stationId,
    'X-Finstat-StationName':  s.stationName,
    'X-Finstat-TimeOut':      String(s.timeOut),
    'X-Finstat-Url':          profileUrl(profile.skHost, profile.port),
    'X-Finstat-UrlCZ':        profileUrl(profile.czHost, profile.port),
    'X-Finstat-ResponseType': s.responseType,
    'X-Finstat-Lang':         s.lang,
  };
}
