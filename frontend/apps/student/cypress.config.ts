import { defineConfig } from "cypress";
import dotenv from "dotenv";

dotenv.config();

export default defineConfig({
  e2e: {
    experimentalStudio: true,
    // Prevent automatic re-run on every file save; you can still click Run in the UI
    watchForFileChanges: false,
    setupNodeEvents(on, config) {
      config.env.VITE_BASE_URL = process.env.VITE_BASE_URL;
      config.env.VITE_AI_URL = process.env.VITE_AI_URL;
      config.env.VITE_AUTH_URL = process.env.VITE_AUTH_URL;
      config.env.VITE_TASKS_URL = process.env.VITE_TASKS_URL;
      config.env.VITE_USERS_URL = process.env.VITE_USERS_URL;
      return config;
    },
  },
});
