import { createRoot } from "react-dom/client";
import { ToastContainer } from "react-toastify";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
  initAppInsights,
} from "@app-providers";
import { AppRole } from "@app-providers/types";
import "./index.css";
import App from "./App.tsx";

initAppInsights("admin");

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider appRole={AppRole.admin}>
        <App />
        <ToastContainer />
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);
