import { ReactNode, useEffect } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";

import { useAuth, Credentials } from "@app-providers";

export const RequireAuth = ({ children }: { children: ReactNode }) => {
  const { isAuthorized, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(
    function checkCredentials() {
      let stored: Credentials = {} as Credentials;
      try {
        stored = JSON.parse(localStorage.getItem("credentials") || "{}");
      } catch (error) {
        console.warn("Error parsing credentials:", error);
        logout();
        return;
      }

      const { email, password, sessionExpiry, role } = stored;
      if (!email || !password || !sessionExpiry || !role) {
        sessionStorage.setItem("redirectAfterLogin", location.pathname);
        navigate("/signin", { replace: true });
      }
    },
    [location.pathname, logout, navigate],
  );

  if (!isAuthorized) {
    sessionStorage.setItem("redirectAfterLogin", location.pathname);
    return <Navigate to="/signin" replace />;
  }

  return <>{children}</>;
};
