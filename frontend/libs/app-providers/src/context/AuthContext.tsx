import { createContext } from "react";

import { AppRoleType, SignupData } from "@app-providers/types";

export interface AuthContextValue {
  isAuthorized: boolean;
  role: AppRoleType;
  login: (email: string, password: string) => void;
  signup: (data: SignupData) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
