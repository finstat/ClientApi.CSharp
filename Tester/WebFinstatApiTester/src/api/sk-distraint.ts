import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

export interface DistraintQuery {
  ico?: string;
  surname?: string;
  dateOfBirth?: string;
  city?: string;
  companyName?: string;
  fileReference?: string;
}

const nonEmpty = <T extends object>(o: T): Record<string, unknown> => {
  const out: Record<string, unknown> = {};
  for (const [k, v] of Object.entries(o)) if (v !== undefined && v !== '') out[k] = v;
  return out;
};

export const skDistraint = {
  search:         (q: DistraintQuery): Promise<CallResult> =>
    api.post('api/sk/distraint/search', q, { label: t('label.sk.distraint.search'),  params: nonEmpty(q) }),
  results:        (q: DistraintQuery): Promise<CallResult> =>
    api.post('api/sk/distraint/results', q, { label: t('label.sk.distraint.results'), params: nonEmpty(q) }),
  detail:         (token: string, ids: number[]): Promise<CallResult> =>
    api.post('api/sk/distraint/detail', { token, ids }, {
      label: `${t('label.sk.distraint.detail')} (${ids.length} id${ids.length === 1 ? '' : 's'})`,
      params: { token, ids },
    }),
  resultsByToken: (token: string): Promise<CallResult> =>
    api.post('api/sk/distraint/results-by-token', { token }, {
      label: t('label.sk.distraint.resultsByToken'), params: { token },
    }),
  storedDetail:   (id: string): Promise<CallResult> =>
    api.post('api/sk/distraint/stored-detail', { id }, {
      label: `${t('label.sk.distraint.storedDetail')} ${id}`, params: { id },
    }),
};
