import { createContext } from "react";

export interface AuthContextValue {
  isAuthorized: boolean;
  login: (email: string, password: string) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
