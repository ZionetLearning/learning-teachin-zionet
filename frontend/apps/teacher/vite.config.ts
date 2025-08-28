import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import viteTsConfigPaths from "vite-tsconfig-paths";

// https://vite.dev/config/
export default defineConfig({
  root: __dirname,
  server: {
    port: 4001,
    fs: { allow: [path.resolve(__dirname)] },
  },
  build: {
    sourcemap: true, // Source map generation is turned on
  },
  plugins: [
    react(), // point the paths plugin to the **workspace root** (tsconfig.base.json lives there)
    viteTsConfigPaths({ root: path.resolve(__dirname, "../../") }),
  ],
  resolve: {
    alias: {
      "@teacher": path.resolve(__dirname, "src"),
    },
  },
});
