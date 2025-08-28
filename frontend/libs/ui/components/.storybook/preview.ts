import type { Preview } from "@storybook/react";
import { WithReactQuery } from './decorators';

const preview: Preview = {
  decorators: [WithReactQuery],
  parameters: {
    controls: { matchers: { color: /(background|color)$/i, date: /Date$/i } },
    a11y: { test: 'todo' },
  },
};

export default preview;
