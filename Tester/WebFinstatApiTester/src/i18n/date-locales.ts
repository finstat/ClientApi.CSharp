import { BDatePicker, BDatetimePicker, BTime } from 'birko-web-components';

import { getLocale, onLocaleChange, type Locale } from './index.js';

/**
 * Localised month/day names + button labels fed to Birko's date-picker family. Each
 * component exposes a static `setLocale()` that overrides the English defaults. We
 * subscribe to our own i18n locale changes and push the matching entry — keeps the
 * Today / Clear / Now buttons and calendar headers in sync with the rest of the UI.
 */
interface DateLocale {
  months: string[];
  days: string[];
  today: string;
  clear: string;
  now: string;
}

const LOCALES: Record<Locale, DateLocale> = {
  en: {
    months: ['January', 'February', 'March', 'April', 'May', 'June',
             'July', 'August', 'September', 'October', 'November', 'December'],
    days:   ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    today:  'Today',
    clear:  'Clear',
    now:    'Now',
  },
  sk: {
    months: ['Január', 'Február', 'Marec', 'Apríl', 'Máj', 'Jún',
             'Júl', 'August', 'September', 'Október', 'November', 'December'],
    days:   ['Po', 'Ut', 'St', 'Št', 'Pi', 'So', 'Ne'],
    today:  'Dnes',
    clear:  'Vyčistiť',
    now:    'Teraz',
  },
};

function apply(locale: Locale) {
  const l = LOCALES[locale] ?? LOCALES.en;
  BDatePicker.setLocale({ months: l.months, days: l.days, today: l.today, clear: l.clear });
  BDatetimePicker.setLocale({ months: l.months, days: l.days, today: l.today, clear: l.clear, now: l.now });
  BTime.setLocale({ now: l.now, clear: l.clear });
}

apply(getLocale());
onLocaleChange(apply);
