import { api, type CallResult } from './api-client.js';
import { setSkStatementItems } from './cache-store.js';
import { t } from '../i18n/index.js';

export const TEMPLATES = [
  'Template2011v2', 'Template2014', 'Template2014micro',
  'TemplateFinancial', 'TemplateROPO', 'TemplateNujPU',
] as const;
export type Template = typeof TEMPLATES[number];

export const skStatement = {
  async list(ico: string): Promise<CallResult> {
    const r = await api.get('api/sk/statement/list', { ico }, { label: `${t('label.sk.statement.list')} ${ico}`, params: { ico } });
    if (r.ok) setSkStatementItems(ico, r.data);
    return r;
  },
  detail(ico: string, year: number, template: Template): Promise<CallResult> {
    return api.post('api/sk/statement/detail', { ico, year, template }, {
      label: `${t('label.sk.statement.detail')} ${ico}/${year}/${template}`,
      params: { ico, year, template },
    });
  },
  legend(template: Template, lang = 'sk'): Promise<CallResult> {
    return api.get('api/sk/statement/legend', { template, lang }, {
      label: `${t('label.sk.statement.legend')} ${template} [${lang}]`,
      params: { template, lang },
    });
  },
};
