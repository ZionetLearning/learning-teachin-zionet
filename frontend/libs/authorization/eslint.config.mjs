import path from "node:path";
import { fileURLToPath } from "node:url";
import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import tseslint from "typescript-eslint";
import { globalIgnores } from "eslint/config";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default tseslint.config([
  globalIgnores([
    "dist",
    "build",
    "coverage",
    "storybook-static",
    "node_modules",
  ]),

  // TypeScript files (non type-aware; no parserOptions.project)
  {
    files: ["**/*.ts", "**/*.tsx"],
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: {
        tsconfigRootDir: __dirname,
        ecmaVersion: 2020,
        sourceType: "module",
      },

      globals: { ...globals.browser, ...globals.node },
    },
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs["recommended-latest"],
    ],
    rules: {
      "react-hooks/rules-of-hooks": "error",
      "react-hooks/exhaustive-deps": "warn",
    },
  },

  // JavaScript files
  {
    files: ["**/*.js", "**/*.jsx"],
    languageOptions: {
      ecmaVersion: 2020,
      sourceType: "module",
      globals: { ...globals.browser, ...globals.node },
    },
    extends: [js.configs.recommended],
  },
]);
