import { Router, type Route } from 'birko-web-core/router';
import { createShellWrapper } from 'birko-web-shell/shell';

import './app-shell.js';
import './landing-page.js';
import './placeholder-page.js';
import '../pages/sk/detail-page.js';
import '../pages/sk/autocomplete-page.js';
import '../pages/sk/autologin-page.js';
import '../pages/sk/monitoring-categories-page.js';
import '../pages/sk/monitoring-ico-page.js';
import '../pages/sk/monitoring-ico-queries-page.js';
import '../pages/sk/monitoring-date-page.js';
import '../pages/sk/monitoring-date-queries-page.js';
import '../pages/sk/distraint-query-page.js';
import '../pages/sk/distraint-detail-page.js';
import '../pages/sk/distraint-results-by-token-page.js';
import '../pages/sk/distraint-stored-detail-page.js';
import '../pages/sk/diff-page.js';
import '../pages/sk/bankruptcy-persons-page.js';
import '../pages/sk/bankruptcy-company-page.js';
import '../pages/sk/statement-page.js';
import '../pages/sk/reporting-page.js';
import '../pages/cz/detail-page.js';
import '../pages/cz/autocomplete-page.js';
import '../pages/cz/autologin-page.js';
import '../pages/cz/monitoring-categories-page.js';
import '../pages/cz/monitoring-ico-page.js';
import '../pages/cz/monitoring-ico-queries-page.js';
import '../pages/cz/monitoring-date-page.js';
import '../pages/cz/monitoring-date-queries-page.js';
import '../history/response-viewer.js';

const wrap = createShellWrapper('app-shell');

/** Routes registered so far. Later tasks replace `placeholder-page` entries with real pages. */
const ROUTES: Route[] = [
  { path: '/',                    component: () => wrap('landing-page') },
  { path: '/sk/detail/basic',     component: () => wrap('sk-detail-page') },
  { path: '/sk/detail/detail',    component: () => wrap('sk-detail-page') },
  { path: '/sk/detail/extended',  component: () => wrap('sk-detail-page') },
  { path: '/sk/detail/ultimate',  component: () => wrap('sk-detail-page') },
  { path: '/sk/autocomplete',     component: () => wrap('sk-autocomplete-page') },
  { path: '/sk/autologin',        component: () => wrap('sk-autologin-page') },

  { path: '/sk/monitoring/categories',       component: () => wrap('sk-monitoring-categories-page') },
  { path: '/sk/monitoring/add',              component: () => wrap('sk-monitoring-ico-page') },
  { path: '/sk/monitoring/remove',           component: () => wrap('sk-monitoring-ico-page') },
  { path: '/sk/monitoring/list',             component: () => wrap('sk-monitoring-ico-queries-page') },
  { path: '/sk/monitoring/report',           component: () => wrap('sk-monitoring-ico-queries-page') },
  { path: '/sk/monitoring/proceedings',      component: () => wrap('sk-monitoring-ico-queries-page') },
  { path: '/sk/monitoring/date/add',         component: () => wrap('sk-monitoring-date-page') },
  { path: '/sk/monitoring/date/remove',      component: () => wrap('sk-monitoring-date-page') },
  { path: '/sk/monitoring/date/list',        component: () => wrap('sk-monitoring-date-queries-page') },
  { path: '/sk/monitoring/date/report',      component: () => wrap('sk-monitoring-date-queries-page') },
  { path: '/sk/monitoring/date/proceedings', component: () => wrap('sk-monitoring-date-queries-page') },

  { path: '/sk/distraint/search',            component: () => wrap('sk-distraint-query-page') },
  { path: '/sk/distraint/results',           component: () => wrap('sk-distraint-query-page') },
  { path: '/sk/distraint/detail',            component: () => wrap('sk-distraint-detail-page') },
  { path: '/sk/distraint/results-by-token',  component: () => wrap('sk-distraint-results-by-token-page') },
  { path: '/sk/distraint/stored-detail',     component: () => wrap('sk-distraint-stored-detail-page') },

  { path: '/sk/diff/daily/list',             component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/daily/download',         component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/ultimate/list',          component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/ultimate/download',      component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/statement/list',         component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/statement/download',     component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/statement/legend',       component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/statement2014/list',     component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/statement2014/download', component: () => wrap('sk-diff-page') },
  { path: '/sk/diff/statement2014/legend',   component: () => wrap('sk-diff-page') },

  { path: '/sk/bankruptcy/persons',          component: () => wrap('sk-bankruptcy-persons-page') },
  { path: '/sk/bankruptcy/company',          component: () => wrap('sk-bankruptcy-company-page') },

  { path: '/sk/statement/list',              component: () => wrap('sk-statement-page') },
  { path: '/sk/statement/detail',            component: () => wrap('sk-statement-page') },
  { path: '/sk/statement/legend',            component: () => wrap('sk-statement-page') },

  { path: '/sk/reporting/topics',            component: () => wrap('sk-reporting-page') },
  { path: '/sk/reporting/list',              component: () => wrap('sk-reporting-page') },
  { path: '/sk/reporting/download',          component: () => wrap('sk-reporting-page') },

  { path: '/cz/detail/basic',                component: () => wrap('cz-detail-page') },
  { path: '/cz/detail/detail',               component: () => wrap('cz-detail-page') },
  { path: '/cz/detail/premium',              component: () => wrap('cz-detail-page') },
  { path: '/cz/detail/elite',                component: () => wrap('cz-detail-page') },
  { path: '/cz/detail/ultimate',             component: () => wrap('cz-detail-page') },
  { path: '/cz/autocomplete',                component: () => wrap('cz-autocomplete-page') },
  { path: '/cz/autologin',                   component: () => wrap('cz-autologin-page') },

  { path: '/cz/monitoring/categories',       component: () => wrap('cz-monitoring-categories-page') },
  { path: '/cz/monitoring/add',              component: () => wrap('cz-monitoring-ico-page') },
  { path: '/cz/monitoring/remove',           component: () => wrap('cz-monitoring-ico-page') },
  { path: '/cz/monitoring/list',             component: () => wrap('cz-monitoring-ico-queries-page') },
  { path: '/cz/monitoring/report',           component: () => wrap('cz-monitoring-ico-queries-page') },
  { path: '/cz/monitoring/date/add',         component: () => wrap('cz-monitoring-date-page') },
  { path: '/cz/monitoring/date/remove',      component: () => wrap('cz-monitoring-date-page') },
  { path: '/cz/monitoring/date/list',        component: () => wrap('cz-monitoring-date-queries-page') },
  { path: '/cz/monitoring/date/report',      component: () => wrap('cz-monitoring-date-queries-page') },

  { path: '/history',             component: () => wrap('response-viewer') },
  { path: '/history/:id',         component: () => wrap('response-viewer') },
  { path: '*',                    component: () => wrap('placeholder-page') },
];

export const router = new Router(ROUTES, '#app');

// Kick the first resolution once the DOM is ready.
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => router.resolve());
} else {
  router.resolve();
}
