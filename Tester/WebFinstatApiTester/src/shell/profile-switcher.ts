import { BaseComponent, define } from 'birko-web-core';
import type { BSelect } from 'birko-web-components';
import { t, onLocaleChange } from '../i18n/index.js';
import { settingsStore, useAndSaveSettings, activeProfile, profileUrl, profileColorCss } from '../settings/settings-store.js';

/**
 * Header control: the active environment profile, switchable without opening Settings and saved
 * as "Use and Save" in Settings would. Its port is set in the ribbon's Environment tab.
 */
export class ProfileSwitcher extends BaseComponent {
  private _unsubs: Array<() => void> = [];

  static get styles() {
    return `
      :host { display: inline-flex; align-items: center; gap: var(--b-space-xs, 0.25rem); }
      #profile { width: 10rem; }
      #dot { width: 0.75rem; height: 0.75rem; border-radius: 50%; flex: 0 0 auto; }
    `;
  }

  render() {
    return `
      <span id="dot" aria-hidden="true"></span>
      <b-select id="profile" bare label="${t('settings.field.profile')}"></b-select>
    `;
  }

  protected onMount() {
    this.$<BSelect>('#profile')?.addEventListener('change', (e: Event) => {
      const id = String((e as CustomEvent<{ value: unknown }>).detail.value ?? '');
      if (id && id !== settingsStore.get('activeProfileId')) useAndSaveSettings({ activeProfileId: id });
    });

    this._unsubs.push(
      settingsStore.onChange('activeProfileId', () => this._refresh()),
      settingsStore.onChange('profiles',        () => this._refresh()),
      onLocaleChange(() => { this.update(); this._refresh(); }),
    );
    this._refresh();
  }

  protected onUnmount() {
    this._unsubs.forEach(u => u());
    this._unsubs = [];
  }

  private _refresh() {
    const active = activeProfile();
    const select = this.$<BSelect>('#profile');
    select?.setOptions(settingsStore.get('profiles').map(p => ({ value: p.id, label: p.name })));
    if (select) select.value = active.id;
    const dot = this.$<HTMLElement>('#dot');
    const color = profileColorCss(active.color);
    if (dot) { dot.style.background = color; dot.style.display = color ? '' : 'none'; }
    this.title = `SK: ${profileUrl(active.skHost, active.port)}\nCZ: ${profileUrl(active.czHost, active.port)}`;
  }
}

define('profile-switcher', ProfileSwitcher);
