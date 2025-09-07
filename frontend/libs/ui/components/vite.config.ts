import { defineConfig } from "vitest/config";
import path from "node:path";

export default defineConfig({
  resolve: {
    // Dedupe React to avoid version conflicts
    dedupe: ["react", "react-dom"],
    alias: {
      "@ui-components": path.resolve(__dirname, "./src/index.ts"),
    },
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
    include: ["./src/**/*.test.{ts,tsx}"],
    // Enable globals so expect, describe, it are available
    globals: true,
  },
});
