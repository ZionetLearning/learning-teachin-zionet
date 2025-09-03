import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { AuthProvider, I18nTranslateProvider } from "@app-providers";
import { AppRole } from "@app-providers/types";
import App from "./App.tsx";

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <AuthProvider appRole={AppRole.teacher}>
      <StrictMode>
        <App />
      </StrictMode>
    </AuthProvider>,
  </I18nTranslateProvider>
);
