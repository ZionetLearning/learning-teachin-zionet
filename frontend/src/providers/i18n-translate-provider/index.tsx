import { useEffect, useRef } from 'react';
import type { ReactNode } from 'react';
import { I18nextProvider } from 'react-i18next';
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { resources } from '../../i18n/locale';

interface Props {
  children: ReactNode;
}

export const I18nTranslateProvider = ({ children }: Props) => {
  const initialized = useRef(false);

  useEffect(() => {
    if (!initialized.current) {
      i18n
        .use(LanguageDetector)
        .use(initReactI18next)
        .init({
          fallbackLng: 'en',
          debug: false,
          interpolation: { escapeValue: false },
          resources
        });
      initialized.current = true;
    }
  }, []);

  return <I18nextProvider i18n={i18n}>{children}</I18nextProvider>;
};
