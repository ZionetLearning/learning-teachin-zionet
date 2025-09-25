import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    experimentalStudio: true,
    watchForFileChanges: false,
    baseUrl: "https://localhost:4002",
  },
  viewportHeight: 1000,
  viewportWidth: 1400,
});
