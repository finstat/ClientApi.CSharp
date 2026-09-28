import { BaseComponent, define } from 'birko-web-core';

/** Stub page used for ribbon routes whose real page hasn't been built yet. */
export class PlaceholderPage extends BaseComponent {
  static get styles() {
    return `
      :host { display: block; padding: var(--b-space-xl, 2rem); color: var(--b-text); }
      code { background: var(--b-bg-tertiary); padding: 0.1rem 0.3rem; border-radius: var(--b-radius-sm, 4px); }
    `;
  }

  render() {
    const hash = window.location.hash || '#/';
    return `
      <b-empty icon="&#128230;" message="Route ${hash} — page not implemented yet."></b-empty>
    `;
  }
}

define('placeholder-page', PlaceholderPage);
