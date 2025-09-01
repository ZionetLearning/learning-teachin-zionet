import { ReactNode, useCallback, useEffect, useState } from "react";

import { useLoginMutation } from "@app-providers/api/auth";
import { AuthContext } from "@app-providers/context";
import { AppRoleType, Credentials } from "@app-providers/types";

const decodeJwtExp = (token: string): number | undefined => {
  try {
    const [, payload] = token.split(".");
    if (!payload) return undefined;
    const json = JSON.parse(
      atob(payload.replace(/-/g, "+").replace(/_/g, "/")),
    );
    if (typeof json.exp === "number") {
      return json.exp * 1000;
    }
  } catch {
    /* noop */
  }
  return undefined;
};

export interface AuthProviderProps {
  children: ReactNode;
  appRole: AppRoleType;
}

export const AuthProvider = ({ children, appRole }: AuthProviderProps) => {
  const [credentials, setCredentials] = useState<Credentials | null>(() => {
    try {
      const raw = localStorage.getItem("credentials");
      if (!raw) return null;
      const parsed = JSON.parse(raw) as Partial<
        Credentials & { sessionExpiry?: number; password?: string }
      >;
      if (
        parsed &&
        parsed.email &&
        parsed.accessToken &&
        parsed.accessTokenExpiry
      ) {
        if (Date.now() < parsed.accessTokenExpiry) {
          return {
            email: parsed.email,
            accessToken: parsed.accessToken,
            accessTokenExpiry: parsed.accessTokenExpiry,
            role: parsed.role,
          };
        } else {
          localStorage.removeItem("credentials");
        }
      } else if (parsed && parsed.email && parsed.sessionExpiry) {
        localStorage.removeItem("credentials");
      }
    } catch (error) {
      console.warn("Error parsing credentials:", error);
      localStorage.removeItem("credentials");
    }
    return null;
  });

  const logout = useCallback(() => {
    localStorage.removeItem("credentials");
    setCredentials(null);
  }, []);

  useEffect(
    function checkAuth() {
      if (!credentials) return;
      const ms = credentials.accessTokenExpiry - Date.now();
      if (ms <= 0) {
        logout();
        return;
      }
      const timer = setTimeout(logout, ms);
      return () => clearTimeout(timer);
    },
    [credentials, logout],
  );

  const persistSession = useCallback(
    (email: string, accessToken: string, role?: AppRoleType) => {
      const decodedExp = decodeJwtExp(accessToken);
      const fallback = Date.now() + 15 * 60 * 1000;
      const accessTokenExpiry =
        decodedExp && decodedExp > Date.now() ? decodedExp : fallback;
      const creds: Credentials = {
        email,
        accessToken,
        accessTokenExpiry,
        role,
      };
      localStorage.setItem("credentials", JSON.stringify(creds));
      setCredentials(creds);
    },
    [],
  );

  const loginMutation = useLoginMutation({
    onSuccess: (data, vars) => {
      persistSession(vars.email, data.accessToken, appRole);
    },
    onError: (err) => {
      console.error("Login error", err);
      logout();
    },
  });

  const login = useCallback(
    async (email: string, password: string) => {
      await loginMutation.mutateAsync({ email, password });
    },
    [loginMutation],
  );

  const signup = useCallback(() => {
    // TODO: Implement real signup flow when backend endpoint is available.
    logout();
    console.warn("Signup placeholder: implement backend signup before using.");
  }, [logout]);

  return (
    <AuthContext.Provider
      value={{
        isAuthorized: credentials !== null,
        role: credentials?.role || appRole,
        login,
        signup,
        logout,
        accessToken: credentials?.accessToken || null,
        loginStatus: {
          isLoading: loginMutation.isPending,
          error: loginMutation.error,
        },
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
