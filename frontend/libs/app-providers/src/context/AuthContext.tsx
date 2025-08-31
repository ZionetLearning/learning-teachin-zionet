import { createContext } from "react";

import { Role } from "@app-providers/types";

export interface AuthContextValue {
  isAuthorized: boolean;
  role: Role;
  login: (email: string, password: string) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
