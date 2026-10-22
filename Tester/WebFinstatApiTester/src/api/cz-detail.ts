import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

export const czDetail = {
  basic:   (ico: string, lang?: string) => call('basic',   t('label.cz.detail.basic'),   ico, lang),
  detail:  (ico: string, lang?: string) => call('detail',  t('label.cz.detail.detail'),  ico, lang),
  premium: (ico: string, lang?: string) => call('premium', t('label.cz.detail.premium'), ico, lang),
  elite:   (ico: string, lang?: string) => call('elite',   t('label.cz.detail.elite'),   ico, lang),
  ultimate: (ico: string, lang?: string) => call('ultimate', t('label.cz.detail.ultimate'), ico, lang),
};

function call(kind: string, label: string, ico: string, lang?: string): Promise<CallResult> {
  return api.post(`api/cz/detail/${kind}`, { ico }, { label, params: { ico }, lang });
}
