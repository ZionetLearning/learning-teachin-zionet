import type { Preview } from "@storybook/react";
import { WithReactQuery, WithTranslation } from "./decorators";

const preview: Preview = {
  decorators: [WithReactQuery, WithTranslation],
  parameters: {
    controls: { matchers: { color: /(background|color)$/i, date: /Date$/i } },
    a11y: { test: "todo" },
  },
};

export default preview;
