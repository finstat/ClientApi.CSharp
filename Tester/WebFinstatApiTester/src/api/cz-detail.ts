import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

export const czDetail = {
  basic:   (ico: string) => call('basic',   t('label.cz.detail.basic'),   ico),
  detail:  (ico: string) => call('detail',  t('label.cz.detail.detail'),  ico),
  premium: (ico: string) => call('premium', t('label.cz.detail.premium'), ico),
  elite:   (ico: string) => call('elite',   t('label.cz.detail.elite'),   ico),
  ultimate: (ico: string) => call('ultimate', t('label.cz.detail.ultimate'), ico),
};

function call(kind: string, label: string, ico: string): Promise<CallResult> {
  return api.post(`api/cz/detail/${kind}`, { ico }, { label, params: { ico } });
}
