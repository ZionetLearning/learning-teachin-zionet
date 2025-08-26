// frontend/libs/ui/components/eslint.config.js
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import tseslint from 'typescript-eslint';
import { globalIgnores } from 'eslint/config';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default tseslint.config([
    // folders we don't want to lint
    globalIgnores(['dist', 'build', 'coverage', 'storybook-static', 'node_modules']),
    {
        files: ['**/*.{ts,tsx,js,jsx}'],
        extends: [
            js.configs.recommended,
            tseslint.configs.recommended,
            reactHooks.configs['recommended-latest'],
        ],
        languageOptions: {
            parser: tseslint.parser,
            parserOptions: {
                project: ['./tsconfig.json'],
                tsconfigRootDir: __dirname,
                ecmaVersion: 2020,
                sourceType: 'module',
            },
            globals: { ...globals.browser, ...globals.node },
        },
        rules: {
            'react-hooks/rules-of-hooks': 'error',
            'react-hooks/exhaustive-deps': 'warn',
        },
    },
    {
        files: ["src/**/*.stories.@(ts|tsx)"],
    },
]);
