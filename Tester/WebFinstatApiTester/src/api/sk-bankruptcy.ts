import { api, type CallResult } from './api-client.js';
import { t } from '../i18n/index.js';

export const skBankruptcy = {
  person(name: string, surname: string, dateOfBirth: string): Promise<CallResult> {
    return api.post('api/sk/bankruptcy/persons', { name, surname, dateOfBirth }, {
      label: `${t('label.sk.bankruptcy.persons')} ${surname}, ${name} (${dateOfBirth})`,
      params: { name, surname, dateOfBirth },
    });
  },
  company(ico: string, name: string): Promise<CallResult> {
    return api.post('api/sk/bankruptcy/company', { ico, name }, {
      label: `${t('label.sk.bankruptcy.company')} ${ico}`,
      params: { ico, name },
    });
  },
};
