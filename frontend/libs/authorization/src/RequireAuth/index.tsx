import { ReactNode, useEffect, useRef } from "react";
import { Navigate, useLocation } from "react-router-dom";

import { AppRoleType, useAuth } from "@app-providers";

interface RequireAuthProps {
  children: ReactNode;
  allowedRoles: AppRoleType[];
}

const TOKEN_GRACE_PERIOD_MS = 2 * 60 * 1000; // 2 minutes

export const RequireAuth = ({ children, allowedRoles }: RequireAuthProps) => {
  const { isAuthorized, logout, refreshSession, role } = useAuth();
  const location = useLocation();
  const isRefreshingRef = useRef(false);

  useEffect(
    function validateAndRefreshCredentials() {
      if (!isAuthorized) return;

      const validateCredentials = async () => {
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

          const now = Date.now();
          const timeUntilExpiry = parsed.accessTokenExpiry - now;

          // token has expired
          if (timeUntilExpiry <= 0) {
            const timeSinceExpiry = Math.abs(timeUntilExpiry);

            // within grace period - attempt refresh before logging out
            if (timeSinceExpiry <= TOKEN_GRACE_PERIOD_MS) {
              if (isRefreshingRef.current) return;

              console.warn(
                `Token expired ${Math.round(timeSinceExpiry / 1000)}s ago, attempting refresh within grace period`,
              );

              isRefreshingRef.current = true;
              try {
                const success = await refreshSession();
                if (success) {
                  console.log("Successfully refreshed expired token");
                  return;
                }
                console.warn("Failed to refresh expired token, logging out");
                logout();
              } finally {
                isRefreshingRef.current = false;
              }
            } else {
              // beyond grace period - immediate logout
              console.warn(
                `Token expired ${Math.round(timeSinceExpiry / 1000)}s ago (beyond grace period), logging out`,
              );
              logout();
            }
          }
        } catch (e) {
          console.warn(
            "Error validating stored credentials: exception during parsing or validation",
            e,
          );
          logout();
        }
      };

      validateCredentials();
    },
    [isAuthorized, logout, refreshSession, role],
  );

  useEffect(
    function handleVisibilityChange() {
      if (!isAuthorized) return;

      const handleVisibilityChange = async () => {
        if (document.visibilityState !== "visible") return;

        try {
          const raw = localStorage.getItem("credentials");
          if (!raw) return;

          const parsed = JSON.parse(raw) as {
            accessToken?: string;
            accessTokenExpiry?: number;
          };

          if (!parsed.accessToken || !parsed.accessTokenExpiry) return;

          const now = Date.now();
          const timeUntilExpiry = parsed.accessTokenExpiry - now;

          if (timeUntilExpiry <= 3 * 60 * 1000) {
            if (isRefreshingRef.current) return;

            console.log(
              "Tab became visible with token close to expiry, refreshing proactively",
            );

            isRefreshingRef.current = true;
            try {
              await refreshSession();
            } catch (error) {
              console.warn(
                "Proactive refresh on visibility change failed:",
                error,
              );
            } finally {
              isRefreshingRef.current = false;
            }
          }
        } catch (e) {
          console.warn("Error during visibility change handler:", e);
        }
      };

      document.addEventListener("visibilitychange", handleVisibilityChange);
      return () => {
        document.removeEventListener(
          "visibilitychange",
          handleVisibilityChange,
        );
      };
    },
    [isAuthorized, refreshSession],
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
