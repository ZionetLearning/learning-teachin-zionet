// import i18n from 'i18next';
// import { initReactI18next } from 'react-i18next';
// import LanguageDetector from 'i18next-browser-languagedetector';
// import { resources } from './locale';      // the { en: {translation}, he: … } object

// i18n
//   .use(LanguageDetector)
//   .use(initReactI18next)
//   .init({
//     resources,
//     fallbackLng: 'en',
//     //lng: 'en',
//     defaultNS: 'translation',
//     interpolation: { escapeValue: false },

//   });

import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { resources } from './locale';

const firstLng = localStorage.getItem('i18nextLng') || 'en';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    lng: firstLng,        
    fallbackLng: 'en',
    defaultNS: 'translation',

    detection: {
      order: ['localStorage', 'sessionStorage', 'navigator'],
      caches: ['localStorage'],   // remember the choice
    },

    interpolation: { escapeValue: false },
    react: { useSuspense: false },
  });

export default i18n;