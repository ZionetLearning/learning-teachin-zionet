import type { StorybookConfig } from "@storybook/react-vite";
import { mergeConfig } from "vite";
import * as path from "node:path";
import tsconfigPaths from "vite-tsconfig-paths";

const ROOT = path.resolve(__dirname, "../../../../");
const p = (...x: string[]) => path.resolve(ROOT, ...x).replace(/\\/g, "/");

const config: StorybookConfig = {
  framework: { name: "@storybook/react-vite", options: {} },
  stories: [p("libs/**/*.stories.@(ts|tsx|mdx)"), p("apps/**/*.stories.@(ts|tsx|mdx)")],
  addons: ["@storybook/addon-essentials", "@storybook/addon-interactions"],
  viteFinal: async (base) =>
    mergeConfig(base, {
      plugins: [
        tsconfigPaths({
          projects: [p("tsconfig.base.json")],
        }),
      ],
      resolve: {
        alias: {
          "@student": p("apps/student/src"),
          "@teacher": p("apps/teacher/src"),
          "@admin": p("apps/admin/src"),
          "@ui-components": p("libs/ui/components/src"),
        },
      },
      server: { fs: { allow: [ROOT] } },
    }),
};

export default config;
