import type { Preview } from "@storybook/react";
import { withReactQuery } from '../../../../libs/ui/components/.storybook/decorators';

const preview: Preview = {
  decorators: [withReactQuery],
  parameters: {
    controls: { matchers: { color: /(background|color)$/i, date: /Date$/i } },
    a11y: { test: 'todo' },
  },
};

export default preview;
