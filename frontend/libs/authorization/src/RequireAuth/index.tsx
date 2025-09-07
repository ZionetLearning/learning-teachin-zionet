import { ReactNode, useEffect } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { useAuth } from "@app-providers";

export const RequireAuth = ({ children }: { children: ReactNode }) => {
  const { isAuthorized, logout } = useAuth();
  const location = useLocation();

  useEffect(
    function checkCredentials() {
      if (!isAuthorized) return;
      try {
        const raw = localStorage.getItem("credentials");
        if (!raw) return;
        const parsed = JSON.parse(raw) as {
          accessToken?: string;
          accessTokenExpiry?: number;
        };
        if (!parsed.accessToken || !parsed.accessTokenExpiry) {
          logout();
          return;
        }
        if (Date.now() >= parsed.accessTokenExpiry) {
          return;
        }
      } catch (e) {
        console.warn("Error validating stored credentials", e);
        logout();
      }
    },
    [isAuthorized, logout],
  );

  if (!isAuthorized) {
    sessionStorage.setItem("redirectAfterLogin", location.pathname);
    return <Navigate to="/signin" replace />;
  }

  return <>{children}</>;
};
