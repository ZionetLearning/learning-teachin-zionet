import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import { globalIgnores } from "eslint/config";

const __dirname = path.dirname(fileURLToPath(import.meta.url))

export default tseslint.config([
  globalIgnores(["dist"]),
  {
    files: ["**/*.{ts,tsx}"],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs["recommended-latest"],
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: {
        project: [
          "./tsconfig.json",
          "./tsconfig.app.json",
          "./tsconfig.node.json",
        ],
        // IMPORTANT: anchor the parser to this folder
        tsconfigRootDir: __dirname,
        ecmaVersion: 2020,
        sourceType: "module",
      },
      globals: globals.browser,
    },
    extends: [
      js.configs.recommended,
      tseslint.configs.recommendedTypeChecked, // type-aware rules
      reactHooks.configs["recommended-latest"],
      reactRefresh.configs.vite,
    ],
  },
]);
