import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

export const czAuto = {
  autocomplete(query: string, lang?: string): Promise<CallResult> {
    return api.post('api/cz/autocomplete', { query }, {
      label: `${t('label.cz.autoComplete')} "${query}"`, params: { query }, lang,
    });
  },
  autoLogin(url: string, email?: string, lang?: string): Promise<CallResult> {
    return api.post('api/cz/autologin', { url, email }, {
      label: `${t('label.cz.autoLogin')} ${url}`, params: { url, email }, lang,
    });
  },
};
