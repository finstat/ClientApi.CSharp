import { BaseComponent, define } from 'birko-web-core';
import type { BModal, BForm, FormSchema } from 'birko-web-components';
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
  UrlSk:       'urls.finStatApiUrl',
  UrlCz:       'urls.finStatApiUrlCZ',
  Lang:        'station.lang',
};

function candidateHeaders(flat: Record<string, unknown>): Record<string, string> {
  return {
    'X-Finstat-ApiKey':       String(flat.apiKey ?? ''),
    'X-Finstat-PrivateKey':   String(flat.privateKey ?? ''),
    'X-Finstat-StationID':    String(flat.stationId ?? ''),
    'X-Finstat-StationName':  String(flat.stationName ?? ''),
    'X-Finstat-TimeOut':      String(flat.timeOut ?? ''),
    'X-Finstat-Url':          String(flat.finStatApiUrl ?? ''),
    'X-Finstat-UrlCZ':        String(flat.finStatApiUrlCZ ?? ''),
    'X-Finstat-ResponseType': String(flat.responseType ?? ''),
    'X-Finstat-Lang':         String(flat.lang ?? ''),
  };
}

/** Returns true when the backend accepted the settings. On 400, paints errors onto the form. */
async function validateOnServer(form: BForm, flat: Record<string, unknown>): Promise<boolean> {
  try {
    const resp = await fetch('api/settings/validate', {
      method: 'POST',
      headers: candidateHeaders(flat),
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
      if (msg) { form.setFieldError(path, msg); painted = true; }
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
} from './settings-store.js';

function buildSchema(): FormSchema {
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
      {
        name: 'urls', label: t('settings.section.urls'), layout: 'stack',
        children: [
          { name: 'finStatApiUrl',   type: 'text', label: t('settings.field.finStatApiUrl'),   required: true, fullWidth: true },
          { name: 'finStatApiUrlCZ', type: 'text', label: t('settings.field.finStatApiUrlCZ'), required: true, fullWidth: true },
        ],
      },
    ],
  };
}

/** Flatten current settings into the nested shape the form expects. */
function toFormValues() {
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
    urls: {
      finStatApiUrl: s.finStatApiUrl,
      finStatApiUrlCZ: s.finStatApiUrlCZ,
    },
  };
}

/** Flatten the form result back to a flat Settings object. */
function fromFormData(data: Record<string, unknown>): Record<string, unknown> {
  const c = (data.credentials ?? {}) as Record<string, unknown>;
  const st = (data.station ?? {}) as Record<string, unknown>;
  const u = (data.urls ?? {}) as Record<string, unknown>;
  return {
    apiKey:          String(c.apiKey ?? ''),
    privateKey:      String(c.privateKey ?? ''),
    stationId:       String(st.stationId ?? DEFAULT_SETTINGS.stationId),
    stationName:     String(st.stationName ?? DEFAULT_SETTINGS.stationName),
    timeOut:         Number(st.timeOut ?? DEFAULT_SETTINGS.timeOut),
    responseType:    (st.responseType === 'json' ? 'json' : 'xml') as 'xml' | 'json',
    lang:            (st.lang === 'sk' || st.lang === 'cs' ? st.lang : '') as '' | 'sk' | 'cs',
    finStatApiUrl:   String(u.finStatApiUrl   ?? DEFAULT_SETTINGS.finStatApiUrl),
    finStatApiUrlCZ: String(u.finStatApiUrlCZ ?? DEFAULT_SETTINGS.finStatApiUrlCZ),
  };
}

export class SettingsModal extends BaseComponent {
  private _windowListener?: () => void;

  render() {
    return `
      <b-modal id="modal" title="${t('settings.title')}" size="lg">
        <b-form id="form"></b-form>
        <footer slot="footer">
          <b-button variant="secondary" id="btn-cancel">${t('settings.button.cancel')}</b-button>
          <b-button variant="ghost"     id="btn-use">${t('settings.button.use')}</b-button>
          <b-button variant="primary"   id="btn-save">${t('settings.button.save')}</b-button>
        </footer>
      </b-modal>
    `;
  }

  private _unsubLocale?: () => void;

  protected onMount() {
    const form = this.$<BForm>('#form');
    form?.setSchema(buildSchema());

    this.$('#btn-cancel')?.addEventListener('click', () => this.close());
    this.$('#btn-use')?.addEventListener('click', () => this._apply(false));
    this.$('#btn-save')?.addEventListener('click', () => this._apply(true));

    this._windowListener = () => this.open();
    window.addEventListener(OPEN_SETTINGS_EVENT, this._windowListener);
    this._unsubLocale = onLocaleChange(() => { this.update(); this.$<BForm>('#form')?.setSchema(buildSchema()); });

    // Auto-open on first load when credentials are missing (matches desktop Backstage behaviour).
    if (!hasCredentials()) queueMicrotask(() => this.open());
  }

  protected onUnmount() {
    if (this._windowListener) window.removeEventListener(OPEN_SETTINGS_EVENT, this._windowListener);
    this._unsubLocale?.();
  }

  open() {
    this.$<BForm>('#form')?.setValues(toFormValues());
    this.$<BModal>('#modal')?.open();
  }

  close() { this.$<BModal>('#modal')?.close(); }

  private async _apply(persist: boolean) {
    const form = this.$<BForm>('#form');
    if (!form) return;
    const { valid, data } = form.validate();
    if (!valid) return;

    const flat = fromFormData(data);
    if (!(await validateOnServer(form, flat))) return;

    if (persist) {
      useAndSaveSettings(flat);
      toast.success(t('settings.toast.saved'));
    } else {
      useSettings(flat);
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
