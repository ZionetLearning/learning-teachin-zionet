import { createContext } from "react";
import { AppRoleType, SignupData, UserDto } from "@app-providers/types";

export interface AuthContextValue {
  isAuthorized: boolean;
  role?: AppRoleType;
  appRole: AppRoleType;
  login: (email: string, password: string) => Promise<void> | void;
  signup: (data: SignupData) => void;
  logout: () => void;
  refreshSession: () => Promise<boolean>;
  accessToken?: string | null;
  loginStatus?: { isLoading: boolean; error: unknown };
  user: UserDto | null;
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);
