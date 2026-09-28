import { api, type CallResult } from './api-client.js';
import { setSkReportingTopics, setSkReportingFiles } from './cache-store.js';
import { t } from '../i18n/index.js';

export const skReporting = {
  async topics(): Promise<CallResult> {
    const r = await api.get('api/sk/reporting/topics', undefined, { label: t('label.sk.reporting.topics') });
    if (r.ok) setSkReportingTopics(r.data);
    return r;
  },
  async list(topic: string): Promise<CallResult> {
    const r = await api.get('api/sk/reporting/list', { topic }, { label: `${t('label.sk.reporting.list')} "${topic}"`, params: { topic } });
    if (r.ok) setSkReportingFiles(topic, r.data);
    return r;
  },
  download(fileName: string): Promise<CallResult<{ filename: string; size: number } | null>> {
    return api.download('api/sk/reporting/download', { fileName }, {
      label: `${t('label.sk.reporting.download')} "${fileName}"`,
      params: { fileName },
    });
  },
};
