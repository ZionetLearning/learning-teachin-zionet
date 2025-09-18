import { ReactNode, useEffect } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { AppRoleType, useAuth } from "@app-providers";

interface RequireAuthProps {
  children: ReactNode;
  allowedRoles: AppRoleType[];
}
export const RequireAuth = ({ children, allowedRoles }: RequireAuthProps) => {
  const { isAuthorized, logout, role } = useAuth();
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
          console.warn(
            "Error validating stored credentials: missing accessToken or accessTokenExpiry",
          );
          logout();
          return;
        }
        if (Date.now() >= parsed.accessTokenExpiry) {
          console.warn(
            "Error validating stored credentials: accessToken expired",
          );
          logout();
          return;
        }
      } catch (e) {
        console.warn(
          "Error validating stored credentials: exception during parsing or validation",
          e,
        );
        logout();
      }
    },
    [isAuthorized, logout, role],
  );

  if (!isAuthorized) {
    if (!sessionStorage.getItem("redirectAfterLogin")) {
      sessionStorage.setItem("redirectAfterLogin", location.pathname);
    }
    return <Navigate to="/signin" replace />;
  }
  if (!role) {
    return <Navigate to="/signin" replace state={{ reason: "missing-role" }} />;
  }
  if (!allowedRoles.includes(role)) {
    return (
      <Navigate to="/signin" replace state={{ reason: "forbidden-role" }} />
    );
  }

  return <>{children}</>;
};
