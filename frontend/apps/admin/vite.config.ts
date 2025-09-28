/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";
import viteTsConfigPaths from "vite-tsconfig-paths";
import mkcert from "vite-plugin-mkcert";

const r = (...x: string[]) => path.resolve(__dirname, "..", "..", ...x);

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
    viteTsConfigPaths({ projects: [r("tsconfig.base.json")] }),
    mkcert(),
  ],
  resolve: {
    alias: {
      "@admin": path.resolve(__dirname, "src"),
      "@ui-components": path.resolve(__dirname, "../../libs/ui/components/src"),
      "@ui-components/*": path.resolve(
        __dirname,
        "../../libs/ui/components/src/*",
      ),
      "@app-providers": path.resolve(__dirname, "../../libs/app-providers/src"),
      "@app-providers/*": path.resolve(
        __dirname,
        "../../libs/app-providers/src/*",
      ),
      "@student": path.resolve(__dirname, "../student/src"),
      "@teacher": path.resolve(__dirname, "../teacher/src"),
    },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./src/test/setup.ts",
    css: true,
    exclude: ["node_modules", "dist", "**/*.jest.{test,spec}.{ts,tsx,js,jsx}"],
  },
});
