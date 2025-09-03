import { useMutation, UseMutationOptions } from "@tanstack/react-query";
import axios from "axios";
import {
  extractCsrf,
  storeCsrf,
  getCsrf,
  clearCsrf,
} from "@app-providers/auth";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  csrfToken?: string;
}

export interface RefreshTokensResponse {
  accessToken: string;
  csrfToken?: string;
}

export interface LogoutResponse {
  message: string;
}

export const login = async (request: LoginRequest): Promise<LoginResponse> => {
  const res = await axios.post<Partial<LoginResponse>>(
    `${import.meta.env.VITE_AUTH_URL}/login`,
    request,
    { withCredentials: true },
  );
  const headerToken = (res.headers?.["X-CSRF-Token"] as string) || undefined;
  const bodyToken = extractCsrf(res.data);
  const csrfToken = bodyToken || headerToken;
  if (csrfToken) storeCsrf(csrfToken);
  if (!res.data?.accessToken)
    throw new Error("Missing accessToken in response");
  return { accessToken: res.data.accessToken, csrfToken };
};

export const refreshTokens = async (): Promise<RefreshTokensResponse> => {
  const csrf = getCsrf() || undefined;
  const res = await axios.post<Partial<RefreshTokensResponse>>(
    `${import.meta.env.VITE_AUTH_URL}/refresh-tokens`,
    {},
    {
      withCredentials: true,
      headers: csrf ? { "X-CSRF-Token": csrf } : undefined,
    },
  );
  const headerToken = (res.headers?.["X-CSRF-Token"] as string) || undefined;
  const bodyToken = extractCsrf(res.data);
  const csrfToken = bodyToken || headerToken;
  if (csrfToken) storeCsrf(csrfToken);
  if (!res.data?.accessToken)
    throw new Error("Missing accessToken in refresh response");
  return { accessToken: res.data.accessToken, csrfToken };
};

export const logout = async (): Promise<LogoutResponse> => {
  const csrf = getCsrf() || undefined;
  const { data } = await axios.post<Partial<LogoutResponse>>(
    `${import.meta.env.VITE_AUTH_URL}/logout`,
    {},
    {
      withCredentials: true,
      headers: csrf ? { "X-CSRF-Token": csrf } : undefined,
    },
  );
  clearCsrf();
  return { message: data?.message || "Logged out" };
};

export const useLoginMutation = (
  options?: UseMutationOptions<LoginResponse, unknown, LoginRequest, unknown>,
) => {
  return useMutation<LoginResponse, unknown, LoginRequest>({
    mutationKey: ["auth", "login"],
    mutationFn: login,
    ...options,
  });
};

export const useRefreshTokensMutation = (
  options?: UseMutationOptions<RefreshTokensResponse, unknown, void, unknown>,
) => {
  return useMutation<RefreshTokensResponse, unknown, void>({
    mutationKey: ["auth", "refresh"],
    mutationFn: refreshTokens,
    ...options,
  });
};

export const useLogoutMutation = (
  options?: UseMutationOptions<LogoutResponse, unknown, void, unknown>,
) => {
  return useMutation<LogoutResponse, unknown, void>({
    mutationKey: ["auth", "logout"],
    mutationFn: logout,
    ...options,
  });
};
