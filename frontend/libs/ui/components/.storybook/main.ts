import type { StorybookConfig } from "@storybook/react-vite";
import * as path from "node:path";
import tsconfigPaths from "vite-tsconfig-paths";
const config: StorybookConfig = {
  framework: { name: "@storybook/react-vite", options: {} },
  stories: ["../src/**/*.stories.@(ts|tsx|mdx)"],
  addons: ["@storybook/addon-essentials", "@storybook/addon-interactions"],
  viteFinal: async (config) => {
    config.plugins = [
      ...(config.plugins || []),
      tsconfigPaths({
        projects: [path.resolve(__dirname, "../../../../tsconfig.base.json")],
      }),
    ];
    return config;
  },
};
export default config;
