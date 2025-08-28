import type { Decorator } from "@storybook/react";
import { I18nTranslateProvider } from "@app-providers";

export const WithTranslation: Decorator = (Story) => {
  return (
    <I18nTranslateProvider>
      <Story />
    </I18nTranslateProvider>
  );
};
