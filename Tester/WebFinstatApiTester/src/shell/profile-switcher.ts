import { BaseComponent, define } from 'birko-web-core';
import type { BSelect, BInput } from 'birko-web-components';
import { t, onLocaleChange } from '../i18n/index.js';
import { settingsStore, useAndSaveSettings, activeProfile, profileUrl } from '../settings/settings-store.js';

/** Typing in the port field saves once the user pauses, not on every keystroke. */
const PORT_SAVE_DELAY_MS = 600;

/**
 * Header control: the active environment profile and its port, switchable without opening
 * Settings. Both changes are saved, as "Use and Save" in Settings would.
 */
export class ProfileSwitcher extends BaseComponent {
  private _unsubs: Array<() => void> = [];
  private _portTimer?: ReturnType<typeof setTimeout>;

  static get styles() {
    return `
      :host { display: inline-flex; align-items: center; gap: var(--b-space-xs, 0.25rem); }
      #profile { width: 10rem; }
      #port { width: 5.5rem; }
    `;
  }

  render() {
    return `
      <b-select id="profile" bare label="${t('settings.field.profile')}"></b-select>
      <b-input id="port" bare type="number" min="1" max="65535" placeholder="443" label="${t('settings.field.port')}"></b-input>
    `;
  }

  protected onMount() {
    this.$<BSelect>('#profile')?.addEventListener('change', (e: Event) => {
      const id = String((e as CustomEvent<{ value: unknown }>).detail.value ?? '');
      if (id && id !== settingsStore.get('activeProfileId')) useAndSaveSettings({ activeProfileId: id });
    });
    this.$<BInput>('#port')?.addEventListener('change', (e: Event) => {
      const value = String((e as CustomEvent<{ value: unknown }>).detail.value ?? '');
      clearTimeout(this._portTimer);
      this._portTimer = setTimeout(() => this._savePort(value), PORT_SAVE_DELAY_MS);
    });

    this._unsubs.push(
      settingsStore.onChange('activeProfileId', () => this._refresh()),
      settingsStore.onChange('profiles',        () => this._refresh()),
      onLocaleChange(() => { this.update(); this._refresh(); }),
    );
    this._refresh();
  }

  protected onUnmount() {
    clearTimeout(this._portTimer);
    this._unsubs.forEach(u => u());
    this._unsubs = [];
  }

  private _refresh() {
    const active = activeProfile();
    const select = this.$<BSelect>('#profile');
    select?.setOptions(settingsStore.get('profiles').map(p => ({ value: p.id, label: p.name })));
    if (select) select.value = active.id;
    const port = this.$<BInput>('#port');
    const portText = active.port ? String(active.port) : '';
    // leave the field alone while it already shows this value, so typing is not disturbed
    if (port && port.value !== portText) port.value = portText;
    this.title = `SK: ${profileUrl(active.skHost, active.port)}\nCZ: ${profileUrl(active.czHost, active.port)}`;
  }

  /** Empty = default port; anything outside 1–65535 is ignored until it becomes valid. */
  private _savePort(value: string) {
    const trimmed = value.trim();
    const port = trimmed === '' ? null : Number(trimmed);
    if (port !== null && !(Number.isInteger(port) && port >= 1 && port <= 65535)) return;
    const active = activeProfile();
    if (port === active.port) return;
    useAndSaveSettings({ profiles: settingsStore.get('profiles').map(p => p.id === active.id ? { ...p, port } : p) });
  }
}

define('profile-switcher', ProfileSwitcher);
