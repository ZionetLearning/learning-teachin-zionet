/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import { sentryVitePlugin } from "@sentry/vite-plugin";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  build: {
    sourcemap: true, // Source map generation is turned on
  },
  plugins: [
    react(),
    sentryVitePlugin({
      org: "zionet",
      project: "teach-in",
      authToken: process.env.SENTRY_AUTH_TOKEN,
      release: { name: process.env.RELEASE || "local" },
    }),
  ],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "src"),
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
});
