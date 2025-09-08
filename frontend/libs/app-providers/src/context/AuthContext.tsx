import { createContext } from "react";

import { AppRoleType, SignupData } from "@app-providers/types";

export interface AuthContextValue {
  isAuthorized: boolean;
  role: AppRoleType;
  login: (email: string, password: string) => Promise<void> | void;
  signup: (data: SignupData) => void;
  logout: () => void;
  accessToken?: string | null;
  loginStatus?: { isLoading: boolean; error: unknown };
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
