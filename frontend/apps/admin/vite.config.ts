/// <reference types="vitest/config" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'node:path';
import viteTsConfigPaths from 'vite-tsconfig-paths';

const r = (...x: string[]) => path.resolve(__dirname, '..', '..', ...x);

export default defineConfig({
  root: __dirname,
  server: {
    port: 4002,
    fs: { allow: [path.resolve(__dirname)] },
  },
  build: { sourcemap: true },
  plugins: [
    react(),
    // point to the workspace tsconfig.base.json so aliases like @ui/components work
    viteTsConfigPaths({ projects: [r('tsconfig.base.json')] }),
  ],
  resolve: {
    alias: {
      '@admin': path.resolve(__dirname, 'src'),
    },
  },
  test: {
    environment: 'jsdom',   
    globals: true,
    setupFiles: './src/test/setup.ts',
    css: true,
    exclude: ['node_modules', 'dist', '**/*.jest.{test,spec}.{ts,tsx,js,jsx}'],
  },
});
