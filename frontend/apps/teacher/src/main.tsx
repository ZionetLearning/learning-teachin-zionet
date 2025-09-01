import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { AuthProvider } from "@app-providers";
import { AppRole } from "@app-providers/types";
import App from "./App.tsx";

createRoot(document.getElementById("root")!).render(
  <AuthProvider appRole={AppRole.teacher}>
    <StrictMode>
      <App />
    </StrictMode>
  </AuthProvider>,
);
