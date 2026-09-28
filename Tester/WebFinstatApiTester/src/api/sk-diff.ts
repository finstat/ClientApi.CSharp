import { api, type CallResult } from './api-client.js';
import { setSkDiffFiles } from './cache-store.js';
import { t } from '../i18n/index.js';

type Kind = 'daily' | 'ultimate' | 'statement' | 'statement2014';
const NAME_KEYS: Record<Kind, string> = {
  daily: 'page.sk.diff.daily',
  ultimate: 'page.sk.diff.ultimate',
  statement: 'page.sk.diff.statement',
  statement2014: 'page.sk.diff.statement2014',
};

export const skDiff = {
  async list(kind: Kind): Promise<CallResult> {
    const r = await api.get(`api/sk/diff/${kind}/list`, undefined, { label: `SK Diff ${t(NAME_KEYS[kind])} ${t('ribbon.item.list')}` });
    if (r.ok) setSkDiffFiles(kind, r.data);
    return r;
  },
  download(kind: Kind, fileName: string): Promise<CallResult<{ filename: string; size: number } | null>> {
    return api.download(`api/sk/diff/${kind}/download`, { fileName }, {
      label: `SK Diff ${t(NAME_KEYS[kind])} ${t('ribbon.item.download')} "${fileName}"`,
      params: { fileName },
    });
  },
  legend(kind: 'statement' | 'statement2014', lang = 'sk'): Promise<CallResult> {
    return api.get(`api/sk/diff/${kind}/legend`, { lang }, {
      label: `SK Diff ${t(NAME_KEYS[kind])} ${t('ribbon.item.legend')} [${lang}]`,
      params: { lang },
    });
  },
};
