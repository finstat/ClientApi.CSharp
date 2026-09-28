import { confirm } from 'birko-web-components/dialogs';

import { t } from '../i18n/index.js';

/**
 * Confirmation gate for endpoints that draw down FinStat credit — the web counterpart of the
 * desktop tester's prompt in front of Distraint Search / Detail. Resolves `true` when the user
 * accepts the charge.
 *
 * Imported from the lean `birko-web-components/dialogs` subpath, which pulls only
 * b-confirm-dialog + b-button rather than the whole component barrel.
 */
export function confirmCharge(): Promise<boolean> {
  return confirm(t('page.sk.distraint.chargeWarning'), {
    title: t('page.sk.distraint.chargeTitle'),
    confirmText: t('page.sk.distraint.chargeConfirm'),
    cancelText: t('common.cancel'),
  });
}
