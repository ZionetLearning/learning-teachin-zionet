import { ReactNode, useCallback, useEffect, useState } from "react";

import { AuthContext } from "@app-providers/context";
import { AppRoleType, Credentials, SignupData } from "@app-providers/types";

export interface AuthProviderProps {
  children: ReactNode;
  appRole: AppRoleType;
}

export const AuthProvider = ({ children, appRole }: AuthProviderProps) => {
  const [credentials, setCredentials] = useState<Credentials | null>(() => {
    let stored: Credentials = {} as Credentials;

    try {
      stored = JSON.parse(localStorage.getItem("credentials") || "{}");
    } catch (error) {
      console.warn("Error parsing credentials:", error);
      logout();
      return null;
    }
    const { email, password, sessionExpiry, role } = stored;
    const expiry = Number(sessionExpiry);
    if (email && password && expiry && Date.now() < expiry) {
      return { email, password, sessionExpiry: expiry, role };
    }

    localStorage.removeItem("credentials");
    return null;
  });

  const logout = useCallback(() => {
    localStorage.removeItem("credentials");
    setCredentials(null);
  }, []);

  useEffect(
    function checkAuth() {
      if (!credentials) return;
      const ms = credentials.sessionExpiry - Date.now();
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
    (email: string, password: string, role: AppRoleType) => {
      const sessionExpiry = Date.now() + 10 * 60 * 60 * 1000; // 10h
      const creds = { email, password, sessionExpiry, role };
      localStorage.setItem("credentials", JSON.stringify(creds));
      setCredentials(creds);
    },
    [],
  );

  const login = useCallback(
    (email: string, password: string, role: AppRoleType = appRole) => {
      persistSession(email, password, role);
    },
    [appRole, persistSession],
  );

  const signup = useCallback(
    (data: SignupData) => {
      persistSession(data.email, data.password, data.role);
    },
    [persistSession],
  );

  return (
    <AuthContext.Provider
      value={{
        isAuthorized: credentials !== null,
        role: credentials?.role || appRole,
        login,
        signup,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
