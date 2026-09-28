import { api, type CallResult } from './api-client.js';
import { setSkMonitoringCategories } from './cache-store.js';
import { t } from '../i18n/index.js';

export const skMonitoring = {
  // ICO
  categories:  async (): Promise<CallResult> => {
    const r = await api.get('api/sk/monitoring/categories', undefined, { label: t('label.sk.monitoring.categories') });
    if (r.ok) setSkMonitoringCategories(r.data);
    return r;
  },
  list:        (category?: string): Promise<CallResult> =>
    api.get('api/sk/monitoring/list', category ? { category } : undefined,
      { label: category ? `${t('label.sk.monitoring.list')} [${category}]` : t('label.sk.monitoring.list'), params: { category: category ?? null } }),
  add:         (ico: string, category?: string): Promise<CallResult> =>
    api.post('api/sk/monitoring/add', { ico, category }, {
      label: category ? `${t('label.sk.monitoring.add')} ${ico} [${category}]` : `${t('label.sk.monitoring.add')} ${ico}`,
      params: { ico, category: category ?? null },
    }),
  remove:      (ico: string, category?: string): Promise<CallResult> =>
    api.post('api/sk/monitoring/remove', { ico, category }, {
      label: category ? `${t('label.sk.monitoring.remove')} ${ico} [${category}]` : `${t('label.sk.monitoring.remove')} ${ico}`,
      params: { ico, category: category ?? null },
    }),
  report:      (category?: string): Promise<CallResult> =>
    api.get('api/sk/monitoring/report', category ? { category } : undefined,
      { label: category ? `${t('label.sk.monitoring.report')} [${category}]` : t('label.sk.monitoring.report'), params: { category: category ?? null } }),
  proceedings: (): Promise<CallResult> =>
    api.get('api/sk/monitoring/proceedings', undefined, { label: t('label.sk.monitoring.proceedings') }),

  // Date
  date: {
    list:        (category?: string): Promise<CallResult> =>
      api.get('api/sk/monitoring/date/list', category ? { category } : undefined,
        { label: category ? `${t('label.sk.monitoring.dateList')} [${category}]` : t('label.sk.monitoring.dateList'),
          params: { category: category ?? null } }),
    add:         (date: string, category?: string): Promise<CallResult> =>
      api.post('api/sk/monitoring/date/add', { date, category }, {
        label: category ? `${t('label.sk.monitoring.dateAdd')} ${date} [${category}]` : `${t('label.sk.monitoring.dateAdd')} ${date}`,
        params: { date, category: category ?? null },
      }),
    remove:      (date: string, category?: string): Promise<CallResult> =>
      api.post('api/sk/monitoring/date/remove', { date, category }, {
        label: category ? `${t('label.sk.monitoring.dateRemove')} ${date} [${category}]` : `${t('label.sk.monitoring.dateRemove')} ${date}`,
        params: { date, category: category ?? null },
      }),
    report:      (category?: string): Promise<CallResult> =>
      api.get('api/sk/monitoring/date/report', category ? { category } : undefined,
        { label: category ? `${t('label.sk.monitoring.dateReport')} [${category}]` : t('label.sk.monitoring.dateReport'),
          params: { category: category ?? null } }),
    proceedings: (): Promise<CallResult> =>
      api.get('api/sk/monitoring/date/proceedings', undefined, { label: t('label.sk.monitoring.dateProceedings') }),
  },
};
