import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    experimentalStudio: true,
    // Prevent automatic re-run on every file save; you can still click Run in the UI
    watchForFileChanges: false,
  },
  viewportHeight: 1000,
  viewportWidth: 1400,
});
