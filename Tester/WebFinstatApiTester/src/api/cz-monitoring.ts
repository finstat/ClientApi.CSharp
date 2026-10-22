import { api, type CallResult } from './api-client.js';
import { setCzMonitoringCategories } from './cache-store.js';
import { t } from '../i18n/index.js';

/**
 * CZ Monitoring — same ICO surface as SK plus Date (CZ has no Proceedings).
 */
export const czMonitoring = {
  categories: async (lang?: string): Promise<CallResult> => {
    const r = await api.get('api/cz/monitoring/categories', undefined, { label: t('label.cz.monitoring.categories'), lang });
    if (r.ok) setCzMonitoringCategories(r.data);
    return r;
  },
  list: (category?: string, lang?: string): Promise<CallResult> =>
    api.get('api/cz/monitoring/list', category ? { category } : undefined,
      { label: category ? `${t('label.cz.monitoring.list')} [${category}]` : t('label.cz.monitoring.list'), params: { category: category ?? null }, lang }),
  add: (ico: string, category?: string, lang?: string): Promise<CallResult> =>
    api.post('api/cz/monitoring/add', { ico, category }, {
      label: category ? `${t('label.cz.monitoring.add')} ${ico} [${category}]` : `${t('label.cz.monitoring.add')} ${ico}`,
      params: { ico, category: category ?? null }, lang,
    }),
  remove: (ico: string, category?: string, lang?: string): Promise<CallResult> =>
    api.post('api/cz/monitoring/remove', { ico, category }, {
      label: category ? `${t('label.cz.monitoring.remove')} ${ico} [${category}]` : `${t('label.cz.monitoring.remove')} ${ico}`,
      params: { ico, category: category ?? null }, lang,
    }),
  report: (category?: string, lang?: string): Promise<CallResult> =>
    api.get('api/cz/monitoring/report', category ? { category } : undefined,
      { label: category ? `${t('label.cz.monitoring.report')} [${category}]` : t('label.cz.monitoring.report'), params: { category: category ?? null }, lang }),

  // Date (no Proceedings — CZ doesn't expose it)
  date: {
    list: (category?: string, lang?: string): Promise<CallResult> =>
      api.get('api/cz/monitoring/date/list', category ? { category } : undefined,
        { label: category ? `${t('label.cz.monitoring.dateList')} [${category}]` : t('label.cz.monitoring.dateList'),
          params: { category: category ?? null }, lang }),
    add: (date: string, category?: string, lang?: string): Promise<CallResult> =>
      api.post('api/cz/monitoring/date/add', { date, category }, {
        label: category ? `${t('label.cz.monitoring.dateAdd')} ${date} [${category}]` : `${t('label.cz.monitoring.dateAdd')} ${date}`,
        params: { date, category: category ?? null }, lang,
      }),
    remove: (date: string, category?: string, lang?: string): Promise<CallResult> =>
      api.post('api/cz/monitoring/date/remove', { date, category }, {
        label: category ? `${t('label.cz.monitoring.dateRemove')} ${date} [${category}]` : `${t('label.cz.monitoring.dateRemove')} ${date}`,
        params: { date, category: category ?? null }, lang,
      }),
    report: (category?: string, lang?: string): Promise<CallResult> =>
      api.get('api/cz/monitoring/date/report', category ? { category } : undefined,
        { label: category ? `${t('label.cz.monitoring.dateReport')} [${category}]` : t('label.cz.monitoring.dateReport'),
          params: { category: category ?? null }, lang }),
  },
};
