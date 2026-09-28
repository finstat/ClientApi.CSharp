import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

/**
 * SK Detail endpoint group — thin typed wrappers over the backend routes
 * that invoke `FinstatApi.ApiClient.Request{Basic,Detail,ExtendedDetail,UltimateDetail}`.
 *
 * Actual response types come from `FinstatApi.ViewModel.*` on the server; we
 * keep them as `unknown` here since the viewer renders the object graph
 * client-side. Pages can supply their own typings where convenient.
 */
export const skDetail = {
  basic:    (ico: string) => call('basic',    t('label.sk.detail.basic'),    ico),
  detail:   (ico: string) => call('detail',   t('label.sk.detail.detail'),   ico),
  extended: (ico: string) => call('extended', t('label.sk.detail.extended'), ico),
  ultimate: (ico: string) => call('ultimate', t('label.sk.detail.ultimate'), ico),
};

function call(kind: string, label: string, ico: string): Promise<CallResult> {
  return api.post(`api/sk/detail/${kind}`, { ico }, { label, params: { ico } });
}
