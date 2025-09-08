import { ReactNode, useCallback, useEffect, useRef, useState } from "react";

import {
  AppRoleType,
  AuthContext,
  Credentials,
  decodeJwtExp,
  SignupData,
  useCreateUser,
  useLoginMutation,
  useLogoutMutation,
  useRefreshTokensMutation,
} from "@app-providers";

export interface AuthProviderProps {
  children: ReactNode;
  appRole: AppRoleType;
}

const MIN_REFRESH_DELAY_MS = 5_000;
const FALLBACK_TOKEN_EXPIRY_MS = 15 * 60 * 1000;

export const AuthProvider = ({ children, appRole }: AuthProviderProps) => {
  const refreshTimerRef = useRef<number | null>(null);
  const refreshSkewMs = 60_000;
  const [credentials, setCredentials] = useState<Credentials | null>(
    function getCredentialsFromLocalStorage() {
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
    },
  );

  const {
    mutateAsync: loginMutation,
    isPending: isLoggingIn,
    error: loginError,
  } = useLoginMutation({
    onSuccess: (data, vars) => {
      persistSession(vars.email, data.accessToken, appRole);
    },
    onError: (err) => {
      console.error("Login error", err);
      logout();
    },
  });

  const {
    mutateAsync: createUserMutation,
    isPending: isCreatingUser,
    error: createUserError,
  } = useCreateUser();

  const { mutateAsync: refreshTokens, isPending: isRefreshing } =
    useRefreshTokensMutation();

  const { mutateAsync: logoutServerMutation, isPending: isLoggingOut } =
    useLogoutMutation();

  const clearSession = useCallback(() => {
    localStorage.removeItem("credentials");
    setCredentials(null);
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      await loginMutation({ email, password });
    },
    [loginMutation],
  );

  const logout = useCallback(async () => {
    try {
      await logoutServerMutation();
    } catch (e) {
      console.warn("Logout error", e);
    } finally {
      clearSession();
    }
  }, [logoutServerMutation, clearSession]);

  const persistSession = useCallback(
    (email: string, accessToken: string, role?: AppRoleType) => {
      const decodedExp = decodeJwtExp(accessToken);
      const fallback = Date.now() + FALLBACK_TOKEN_EXPIRY_MS;
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

  const signup = useCallback(
    async (data: SignupData) => {
      await createUserMutation({
        userId: crypto.randomUUID(),
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        role: data.role,
      });
      await login(data.email, data.password);
    },
    [createUserMutation, login],
  );

  const clearRefreshTimer = useCallback(() => {
    if (refreshTimerRef.current !== null) {
      clearTimeout(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }
  }, []);

  const scheduleRefresh = useCallback(
    (creds: Credentials) => {
      clearRefreshTimer();
      const now = Date.now();
      const target = creds.accessTokenExpiry - refreshSkewMs;
      let delay = target - now;
      if (delay < MIN_REFRESH_DELAY_MS) delay = MIN_REFRESH_DELAY_MS;
      refreshTimerRef.current = window.setTimeout(async () => {
        try {
          const { accessToken } = await refreshTokens();
          persistSession(creds.email, accessToken, creds.role);
        } catch {
          clearSession();
        }
      }, delay);
    },
    [clearRefreshTimer, refreshTokens, persistSession, clearSession],
  );

  useEffect(
    function handleRefresh() {
      if (credentials) {
        scheduleRefresh(credentials);
        return () => clearRefreshTimer();
      }
      clearRefreshTimer();
    },
    [credentials, scheduleRefresh, clearRefreshTimer],
  );

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
          isLoading:
            isLoggingIn || isCreatingUser || isRefreshing || isLoggingOut,
          error: loginError || createUserError,
        },
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
