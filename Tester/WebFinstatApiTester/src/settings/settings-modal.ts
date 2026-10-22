import { BaseComponent, define } from 'birko-web-core';
import type { BModal, BForm, BTabs, FormSchema } from 'birko-web-components';
import { toast } from 'birko-web-components';

// Backend validator property names → nested form-field paths. Keys come from the
// ValidationProblemDetails `errors` map. System.Text.Json's default DictionaryKeyPolicy
// is null, so dictionary keys survive as the raw PascalCase property names used by
// FinstatCredentialsValidator (ApiKey, UrlSk, …) — not camelCased.
const SERVER_FIELD_MAP: Record<string, string> = {
  ApiKey:      'credentials.apiKey',
  PrivateKey:  'credentials.privateKey',
  StationID:   'station.stationId',
  StationName: 'station.stationName',
  TimeOut:     'station.timeOut',
  UrlSk:       'env.skHost',
  UrlCz:       'env.czHost',
  Lang:        'station.lang',
};

function candidateHeaders(flat: Record<string, unknown>, profile: EnvProfile): Record<string, string> {
  return {
    'X-Finstat-ApiKey':       profile.apiKey || String(flat.apiKey ?? ''),
    'X-Finstat-PrivateKey':   profile.privateKey || String(flat.privateKey ?? ''),
    'X-Finstat-StationID':    String(flat.stationId ?? ''),
    'X-Finstat-StationName':  String(flat.stationName ?? ''),
    'X-Finstat-TimeOut':      String(flat.timeOut ?? ''),
    'X-Finstat-Url':          profileUrl(profile.skHost, profile.port),
    'X-Finstat-UrlCZ':        profileUrl(profile.czHost, profile.port),
    'X-Finstat-ResponseType': String(flat.responseType ?? ''),
    'X-Finstat-Lang':         String(flat.lang ?? ''),
  };
}

/** Returns true when the backend accepted the settings. On 400, paints errors onto the form fields. */
async function validateOnServer(
  setFieldError: (path: string, message: string) => void,
  flat: Record<string, unknown>,
  profile: EnvProfile,
): Promise<boolean> {
  try {
    const resp = await fetch('api/settings/validate', {
      method: 'POST',
      headers: candidateHeaders(flat, profile),
    });
    if (resp.ok) return true;
    // Older deployments don't expose /api/settings/validate — don't block the user.
    if (resp.status === 404) return true;

    const data = await resp.json().catch(() => null) as { errors?: Record<string, string[]> } | null;
    const errors = data?.errors ?? {};
    let painted = false;
    for (const [key, messages] of Object.entries(errors)) {
      const path = SERVER_FIELD_MAP[key] ?? key;
      const msg = Array.isArray(messages) ? String(messages[0] ?? '') : String(messages ?? '');
      if (msg) { setFieldError(path, msg); painted = true; }
    }
    if (!painted) toast.error(t('settings.toast.invalid'));
    return false;
  } catch {
    // Network failure — don't block the user, local validation already passed.
    return true;
  }
}

import { OPEN_SETTINGS_EVENT } from '../shell/app-shell.js';
import { t, onLocaleChange } from '../i18n/index.js';
import {
  settingsStore,
  useSettings,
  useAndSaveSettings,
  DEFAULT_SETTINGS,
  hasCredentials,
  findProfile,
  profileUrl,
  type EnvProfile,
} from './settings-store.js';

/** Profile fields a built-in profile keeps fixed; its port and keys stay editable. */
const BUILT_IN_LOCKED = ['env.name', 'env.skHost', 'env.czHost'];

function profileOptions(profiles: readonly EnvProfile[]) {
  return profiles.map(p => ({ value: p.id, label: p.name }));
}

/** General tab: global API keys and station. */
function buildGeneralSchema(): FormSchema {
  return {
    name: 'root',
    children: [
      {
        name: 'credentials', label: t('settings.section.apiKeys'), layout: 'grid',
        children: [
          { name: 'apiKey',     type: 'text',     label: t('settings.field.apiKey'),     required: true, fullWidth: true },
          { name: 'privateKey', type: 'password', label: t('settings.field.privateKey'), required: true, fullWidth: true },
        ],
      },
      {
        name: 'station', label: t('settings.section.station'), layout: 'grid',
        children: [
          { name: 'stationId',   type: 'text',   label: t('settings.field.stationId'),   required: true },
          { name: 'stationName', type: 'text',   label: t('settings.field.stationName'), required: true },
          {
            name: 'timeOut', type: 'number', label: t('settings.field.timeOut'), required: true,
            rules: [{ type: 'min', value: 0, message: t('common.mustBeNonNegative') }],
          },
          {
            name: 'responseType', type: 'select', label: t('settings.field.responseType'), required: true,
            hint: t('settings.field.responseTypeHint'),
            options: [{ value: 'xml', label: 'XML' }, { value: 'json', label: 'JSON' }],
          },
          {
            name: 'lang', type: 'select', label: t('settings.field.lang'),
            hint: t('settings.field.langHint'),
            options: [
              { value: '',   label: t('settings.field.langDefault') },
              { value: 'cs', label: 'CS' },
              { value: 'sk', label: 'SK' },
            ],
          },
        ],
      },
    ],
  };
}

/** Environment tab: the profile picker and the fields of the shown profile. */
function buildEnvSchema(profiles: readonly EnvProfile[]): FormSchema {
  return {
    name: 'root',
    children: [
      {
        name: 'env', layout: 'grid',
        children: [
          {
            name: 'profileId', type: 'select', label: t('settings.field.profile'), required: true, fullWidth: true,
            hint: t('settings.field.profileHint'), options: profileOptions(profiles),
          },
          { name: 'name',   type: 'text', label: t('settings.field.profileName'), required: true },
          {
            name: 'port', type: 'number', label: t('settings.field.port'), hint: t('settings.field.portHint'),
            rules: [{ type: 'min', value: 1, message: t('settings.field.portInvalid') }, { type: 'max', value: 65535, message: t('settings.field.portInvalid') }],
          },
          { name: 'skHost', type: 'text', label: t('settings.field.skHost'), required: true },
          { name: 'czHost', type: 'text', label: t('settings.field.czHost'), required: true },
          { name: 'apiKey',     type: 'text',     label: t('settings.field.profileApiKey'),     hint: t('settings.field.profileKeyHint') },
          { name: 'privateKey', type: 'password', label: t('settings.field.profilePrivateKey'), hint: t('settings.field.profileKeyHint') },
        ],
      },
    ],
  };
}

function profileFormValues(p: EnvProfile) {
  return { profileId: p.id, name: p.name, port: p.port ?? '', skHost: p.skHost, czHost: p.czHost, apiKey: p.apiKey, privateKey: p.privateKey };
}

/** Flatten current settings into the nested shape the General form expects. */
function toGeneralValues() {
  const s = settingsStore.snapshot();
  return {
    credentials: { apiKey: s.apiKey, privateKey: s.privateKey },
    station: {
      stationId: s.stationId,
      stationName: s.stationName,
      timeOut: s.timeOut,
      responseType: s.responseType,
      lang: s.lang,
    },
  };
}

/** Flatten the form result back to the flat Settings fields outside the profiles. */
function fromFormData(data: Record<string, unknown>): Record<string, unknown> {
  const c = (data.credentials ?? {}) as Record<string, unknown>;
  const st = (data.station ?? {}) as Record<string, unknown>;
  return {
    apiKey:          String(c.apiKey ?? ''),
    privateKey:      String(c.privateKey ?? ''),
    stationId:       String(st.stationId ?? DEFAULT_SETTINGS.stationId),
    stationName:     String(st.stationName ?? DEFAULT_SETTINGS.stationName),
    timeOut:         Number(st.timeOut ?? DEFAULT_SETTINGS.timeOut),
    responseType:    (st.responseType === 'json' ? 'json' : 'xml') as 'xml' | 'json',
    lang:            (st.lang === 'sk' || st.lang === 'cs' ? st.lang : '') as '' | 'sk' | 'cs',
  };
}

/** The env group as a profile; a built-in profile keeps its name and hosts whatever the form says. */
function readProfile(base: EnvProfile, env: Record<string, unknown>): EnvProfile {
  const port = Number(env.port);
  return {
    ...base,
    name:       base.builtIn ? base.name   : String(env.name ?? '').trim(),
    skHost:     base.builtIn ? base.skHost : String(env.skHost ?? '').trim(),
    czHost:     base.builtIn ? base.czHost : String(env.czHost ?? '').trim(),
    port:       env.port === '' || env.port == null || !Number.isFinite(port) || port <= 0 ? null : port,
    apiKey:     String(env.apiKey ?? '').trim(),
    privateKey: String(env.privateKey ?? '').trim(),
  };
}

export class SettingsModal extends BaseComponent {
  private _windowListener?: () => void;
  /** Working copy of the profiles while the modal is open; written to the store on Use / Save. */
  private _profiles: EnvProfile[] = [];
  /** Profile currently shown in the env group. */
  private _profileId = '';

  static get styles() {
    return `
      :host { display: contents; }
      /* the General tab's height on both tabs, so switching tabs does not resize the window; no scrolling of
         its own: on a short screen the modal body scrolls, and a second scrollbar inside it would be one too many */
      .tab-body { min-height: 27.5rem; }
      .profile-actions { display: flex; gap: var(--b-space-sm, 0.5rem); justify-content: flex-end; margin-top: var(--b-space-sm, 0.5rem); }
    `;
  }

  render() {
    return `
      <b-modal id="modal" title="${t('settings.title')}" size="lg">
        <b-tabs id="tabs">
          <div slot="general" class="tab-body"><b-form id="form-general"></b-form></div>
          <div slot="env" class="tab-body">
            <b-form id="form-env"></b-form>
            <div class="profile-actions">
              <b-button variant="ghost" size="sm" id="btn-duplicate">${t('settings.button.duplicateProfile')}</b-button>
              <b-button variant="ghost" size="sm" id="btn-delete">${t('settings.button.deleteProfile')}</b-button>
            </div>
          </div>
        </b-tabs>
        <footer slot="footer">
          <b-button variant="secondary" id="btn-cancel">${t('settings.button.cancel')}</b-button>
          <b-button variant="ghost"     id="btn-use">${t('settings.button.use')}</b-button>
          <b-button variant="primary"   id="btn-save">${t('settings.button.save')}</b-button>
        </footer>
      </b-modal>
    `;
  }

  private _unsubLocale?: () => void;

  private get _general() { return this.$<BForm>('#form-general'); }
  private get _env() { return this.$<BForm>('#form-env'); }

  private _setTabs(active?: string) {
    this.$<BTabs>('#tabs')?.setTabs(
      [
        { id: 'general', label: t('settings.tab.general') },
        { id: 'env',     label: t('settings.section.environment') },
      ],
      active,
    );
  }

  protected onMount() {
    this._setTabs('general');
    this._general?.setSchema(buildGeneralSchema());
    this._env?.setSchema(buildEnvSchema(settingsStore.get('profiles')));
    this._env?.addEventListener('change', (e: Event) => {
      const { path, value } = (e as CustomEvent<{ path: string; value: unknown }>).detail;
      if (path === 'env.profileId') this._switchProfile(String(value));
    });

    this.$('#btn-duplicate')?.addEventListener('click', () => this._duplicate());
    this.$('#btn-delete')?.addEventListener('click', () => this._delete());
    this.$('#btn-cancel')?.addEventListener('click', () => this.close());
    this.$('#btn-use')?.addEventListener('click', () => this._apply(false));
    this.$('#btn-save')?.addEventListener('click', () => this._apply(true));

    this._windowListener = () => this.open();
    window.addEventListener(OPEN_SETTINGS_EVENT, this._windowListener);
    this._unsubLocale = onLocaleChange(() => {
      const general = this._general?.getValues();
      const env = this._env?.getValues();
      this.update();
      this._setTabs(this.$<BTabs>('#tabs')?.getAttribute('active') ?? undefined);
      this._general?.setSchema(buildGeneralSchema());
      this._env?.setSchema(buildEnvSchema(this._profiles.length ? this._profiles : settingsStore.get('profiles')));
      if (general) this._general?.setValues(general);
      if (env) this._env?.setValues(env);
      this._lockBuiltIn();
    });

    // Auto-open on first load when credentials are missing (matches desktop Backstage behaviour).
    if (!hasCredentials()) queueMicrotask(() => this.open());
  }

  protected onUnmount() {
    if (this._windowListener) window.removeEventListener(OPEN_SETTINGS_EVENT, this._windowListener);
    this._unsubLocale?.();
  }

  open() {
    this._profiles = settingsStore.get('profiles').map(p => ({ ...p }));
    this._profileId = settingsStore.get('activeProfileId');
    this._general?.setValues(toGeneralValues());
    this._showProfile(this._profileId);
    this._setTabs('general');
    this.$<BModal>('#modal')?.open();
  }

  close() { this.$<BModal>('#modal')?.close(); }

  /** Keep what was typed into the shown profile, then show another one. */
  private _switchProfile(id: string) {
    if (id === this._profileId) return;
    this._storeShownProfile();
    this._showProfile(id);
  }

  private _storeShownProfile() {
    const env = (this._env?.getValues().env ?? {}) as Record<string, unknown>;
    this._profiles = this._profiles.map(p => p.id === this._profileId ? readProfile(p, env) : p);
  }

  private _showProfile(id: string) {
    this._profileId = id;
    this._env?.setFieldOptions('env.profileId', profileOptions(this._profiles));
    this._env?.setValues({ env: profileFormValues(findProfile(this._profiles, id)) });
    this._lockBuiltIn();
  }

  private _lockBuiltIn() {
    const builtIn = findProfile(this._profiles, this._profileId)?.builtIn ?? true;
    for (const path of BUILT_IN_LOCKED) this._env?.setFieldDisabled(path, builtIn);
    this.$<HTMLElement>('#btn-delete')?.toggleAttribute('disabled', builtIn);
  }

  private _duplicate() {
    this._storeShownProfile();
    const source = findProfile(this._profiles, this._profileId);
    const copy: EnvProfile = { ...source, id: crypto.randomUUID(), builtIn: false, name: t('settings.field.profileCopy', { name: source.name }) };
    this._profiles = [...this._profiles, copy];
    this._showProfile(copy.id);
  }

  private _delete() {
    const shown = findProfile(this._profiles, this._profileId);
    if (shown.builtIn) return;
    this._profiles = this._profiles.filter(p => p.id !== shown.id);
    this._showProfile(this._profiles[0].id);
  }

  private async _apply(persist: boolean) {
    const general = this._general;
    const env = this._env;
    if (!general || !env) return;
    const g = general.validate();
    const e = env.validate();
    // show the tab that holds the first error; the other one may be hidden
    if (!g.valid || !e.valid) {
      this._setTabs(g.valid ? 'env' : 'general');
      return;
    }

    this._storeShownProfile();
    const profile = findProfile(this._profiles, this._profileId);
    const flat = fromFormData(g.data);
    const paint = (path: string, message: string) => {
      const tab = path.startsWith('env.') ? 'env' : 'general';
      (tab === 'env' ? env : general).setFieldError(path, message);
      this._setTabs(tab);
    };
    if (!(await validateOnServer(paint, flat, profile))) return;

    const updates = { ...flat, profiles: this._profiles, activeProfileId: profile.id };
    if (persist) {
      useAndSaveSettings(updates);
      toast.success(t('settings.toast.saved'));
    } else {
      useSettings(updates);
      toast.info(t('settings.toast.applied'));
    }
    this.close();
  }
}

define('settings-modal', SettingsModal);

// Mount a single instance outside the shell so the overlay covers the full viewport.
if (typeof document !== 'undefined' && !document.querySelector('settings-modal')) {
  const mount = () => document.body.appendChild(document.createElement('settings-modal'));
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', mount, { once: true });
  } else {
    mount();
  }
}
