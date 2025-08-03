import { ReactNode, useEffect } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";

import { useAuth } from "@/providers/auth";

export const RequireAuth = ({ children }: { children: ReactNode }) => {
  const { isAuthorized } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(
    function checkCredentials() {
      const { email, password, sessionExpiry } = JSON.parse(
        localStorage.getItem("credentials") || "{}",
      );

      if (!email || !password || !sessionExpiry) {
        sessionStorage.setItem("redirectAfterLogin", location.pathname);
        navigate("/signin", { replace: true });
      }
    },
    [location.pathname, navigate],
  );

  if (!isAuthorized) {
    sessionStorage.setItem("redirectAfterLogin", location.pathname);
    return <Navigate to="/signin" replace />;
  }

  return <>{children}</>;
};
