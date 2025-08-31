import { ReactNode, useEffect, useState } from "react";
import { AuthContext } from "@app-providers/context";

export interface Credentials {
  email: string;
  password: string;
  sessionExpiry: number;
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [credentials, setCredentials] = useState<Credentials | null>(() => {
    let stored: Credentials = {} as Credentials;

    try {
      stored = JSON.parse(localStorage.getItem("credentials") || "{}");
    } catch (error) {
      console.warn("Error parsing credentials:", error);
      logout();
      return null;
    }
    const { email, password, sessionExpiry } = stored;
    const expiry = Number(sessionExpiry);
    if (email && password && expiry && Date.now() < expiry) {
      return { email, password, sessionExpiry: expiry };
    }

    localStorage.removeItem("credentials");
    return null;
  });

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
    [credentials],
  );

  const login = (email: string, password: string) => {
    const sessionExpiry = Date.now() + 10 * 60 * 60 * 1000;
    localStorage.setItem(
      "credentials",
      JSON.stringify({ email, password, sessionExpiry }),
    );
    setCredentials({ email, password, sessionExpiry });
  };

  const logout = () => {
    localStorage.removeItem("credentials");
    setCredentials(null);
  };

  return (
    <AuthContext.Provider
      value={{ isAuthorized: credentials !== null, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
};
