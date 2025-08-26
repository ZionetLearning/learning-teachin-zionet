// apps/student/eslint.config.mjs
import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import { globalIgnores } from "eslint/config";
import storybook from "eslint-plugin-storybook";

export default tseslint.config([
  globalIgnores([
    "dist",
    "build",
    "coverage",
    "storybook-static",
    "node_modules",
    "cypress",
  ]),
  {
    files: ["src/**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2020,
      sourceType: "module",
      globals: globals.browser,
    },
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs["recommended-latest"],
      reactRefresh.configs.vite,
    ],
  },

  {
    files: ["src/**/*.stories.@(ts|tsx)"],
    ...storybook.configs["flat/recommended"][0], // apply their first block
  },
]);
