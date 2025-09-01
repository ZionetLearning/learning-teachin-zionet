import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { initAppInsights } from "@app-providers";
import "./index.css";
import App from "./App.tsx";

initAppInsights("admin");

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
