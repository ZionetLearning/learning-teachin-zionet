/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import { sentryVitePlugin } from "@sentry/vite-plugin";
import react from "@vitejs/plugin-react";
import path from "path";
import viteTsConfigPaths from "vite-tsconfig-paths";
import mkcert from "vite-plugin-mkcert";

const r = (...x: string[]) =>
  path.resolve(__dirname, "..", "..", ...x).replace(/\\/g, "/");

export default defineConfig(({ mode }) => ({
  root: __dirname,
  server: {
    port: 4000,
    fs: { allow: [path.resolve(__dirname)] },
  },
  build: {
    sourcemap: true, // Source map generation is turned on
  },
  plugins: [
    react(),
    // point the paths plugin to the **workspace root** (tsconfig.base.json lives there)
    //viteTsConfigPaths({ root: path.resolve(__dirname, "../../") }),
    viteTsConfigPaths({
      projects: [path.resolve(__dirname, "../../tsconfig.base.json")],
    }),
    sentryVitePlugin({
      org: "zionet",
      project: "teach-in",
      authToken: process.env.SENTRY_AUTH_TOKEN,
      release: { name: process.env.RELEASE || "local" },
      applicationKey: "teach-in-app", // for thirdPartyErrorFilterIntegration
      debug: true,
    }),
    // Only use mkcert in development mode, not during tests
    ...(mode !== 'test' && process.env.NODE_ENV !== 'test' ? [mkcert()] : []),
  ],
  resolve: {
    alias: {
      "@student": r("apps/student/src"),
      "@teacher": r("apps/teacher/src"),
      "@admin": r("apps/admin/src"),
      "@ui-components": r("libs/ui/components/src"),
    },
  },
  define: {
    "import.meta.env.VITE_RELEASE": JSON.stringify(process.env.RELEASE || ""), //take the RELEASE value from the build environment and inject it into frontend code
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
    exclude: ["node_modules", "dist", "**/*.jest.{test,spec}.{ts,tsx,js,jsx}"],
  },
}));
