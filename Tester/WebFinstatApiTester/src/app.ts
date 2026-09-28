// Register all Birko web components (<b-input>, <b-table>, <b-modal>, …).
import 'birko-web-components';

// Push localised month / day / button labels into Birko's date + time pickers so
// their built-in Today / Clear / Now buttons + calendar headers match the current UI locale.
import './i18n/date-locales.js';

// Settings + About + Curl modals — self-mount; listen for their respective open events.
import './settings/settings-modal.js';
import './shell/about-modal.js';
import './shell/loading-overlay.js';
import './history/curl-modal.js';

// Batch runner — floating FAB bottom-right opens a drawer with context-aware pre-fill.
import './pages/batch/batch-drawer.js';
import './pages/batch/batch-fab.js';

// Mount the shell + router. The shell wrapper is a singleton — only the inner page
// is swapped on navigation.
import './shell/router.js';
