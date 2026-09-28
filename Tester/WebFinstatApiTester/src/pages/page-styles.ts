/**
 * Shared CSS for tester pages — composed into every page's `static get styles()`.
 * Covers the recurring layout primitives (`:host`, `h2`, `.hint`, `.row`, `.grid`,
 * `.buttons`) so individual pages only need page-specific tweaks.
 */
export const PAGE_STYLES = `
  :host { display: block; padding: var(--b-space-xl, 2rem); color: var(--b-text); }
  h2 { margin: 0 0 var(--b-space-md, 1rem); }
  p.hint { color: var(--b-text-secondary); margin: 0 0 var(--b-space-lg, 1.5rem); }
  b-card { margin-bottom: var(--b-space-md, 1rem); }
  .row { display: flex; gap: var(--b-space-sm, 0.5rem); align-items: flex-end; flex-wrap: wrap; }
  .row b-input, .row b-select { flex: 1 1 16rem; min-width: 12rem; }
  .grid { display: grid; gap: var(--b-space-md, 0.75rem); }
  .grid-2 { grid-template-columns: 1fr 1fr; max-width: 50rem; }
  .grid-3 { grid-template-columns: repeat(3, minmax(14rem, 1fr)); max-width: 60rem; }
  .grid-4 { grid-template-columns: repeat(4, minmax(12rem, 1fr)); }
  .buttons { display: flex; gap: var(--b-space-sm, 0.5rem); flex-wrap: wrap; margin-top: var(--b-space-md, 1rem); }
  .actions { margin-top: var(--b-space-md, 1rem); }
  /* Inline affordance shown next to a cache-backed select when the cache is empty —
     a single ghost button that fires the producer endpoint silently. */
  .cache-hint { margin-top: var(--b-space-xs, 0.25rem); display: flex; }
  /* URL-picked action — a 2px accent bar under the button (same idiom as the ribbon's
     active tab underline), plus a gentle one-shot fade-in when the route changes. No
     size change, variant-agnostic. */
  b-button[data-emphasis] { position: relative; }
  b-button[data-emphasis]::after {
    content: '';
    position: absolute;
    left: 10%; right: 10%;
    bottom: -0.375rem;
    height: 2px;
    background: var(--b-color-primary);
    border-radius: 1px;
    pointer-events: none;
    animation: emphasis-land 250ms ease-out;
  }
  @keyframes emphasis-land {
    from { opacity: 0; transform: translateY(-2px); }
    to   { opacity: 1; transform: translateY(0); }
  }
`;
