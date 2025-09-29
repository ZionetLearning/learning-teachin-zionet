import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    experimentalStudio: true,
    watchForFileChanges: false,
    baseUrl: "https://localhost:4000",
  },
  viewportHeight: 1000,
  viewportWidth: 1400,
});
