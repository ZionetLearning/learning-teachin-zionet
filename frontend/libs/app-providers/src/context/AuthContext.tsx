import { createContext } from "react";

import { AppRoleType } from "@app-providers/types";

export interface AuthContextValue {
  isAuthorized: boolean;
  role: AppRoleType;
  login: (email: string, password: string) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
