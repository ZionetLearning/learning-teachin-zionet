import { ReactNode } from "react";
import { I18nextProvider } from "react-i18next";
import i18n from "@app-providers/i18n";

interface Props {
  children: ReactNode;
}

export const I18nTranslateProvider = ({ children }: Props) => (
  <I18nextProvider i18n={i18n}>{children}</I18nextProvider>
);
