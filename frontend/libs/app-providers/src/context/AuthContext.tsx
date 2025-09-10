import { createContext } from "react";
import { AppRoleType, SignupData } from "@app-providers/types";

export interface UserInfo {
  firstName: string;
  lastName: string;
  email: string;
  userId?: string;
  role?: AppRoleType;
}
export interface AuthContextValue {
  isAuthorized: boolean;
  role: AppRoleType;
  login: (email: string, password: string) => Promise<void> | void;
  signup: (data: SignupData) => void;
  logout: () => void;
  accessToken?: string | null;
  loginStatus?: { isLoading: boolean; error: unknown };
  user?: UserInfo | null;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
