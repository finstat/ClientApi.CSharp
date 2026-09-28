import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

export const skAuto = {
  autocomplete(query: string): Promise<CallResult> {
    return api.post('api/sk/autocomplete', { query }, {
      label: `${t('label.sk.autoComplete')} "${query}"`,
      params: { query },
    });
  },
  autoLogin(url: string, email?: string): Promise<CallResult> {
    return api.post('api/sk/autologin', { url, email }, {
      label: `${t('label.sk.autoLogin')} ${url}`,
      params: { url, email },
    });
  },
};
