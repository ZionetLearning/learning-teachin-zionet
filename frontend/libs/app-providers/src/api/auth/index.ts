import { useMutation, UseMutationOptions } from "@tanstack/react-query";
import axios, { AxiosError } from "axios";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
}

const buildAuthBaseUrl = (): string => {
  const viteEnv = (
    import.meta as unknown as {
      env?: Record<string, string | undefined> & { VITE_AUTH_URL?: string };
    }
  ).env;
  const raw = viteEnv?.VITE_AUTH_URL || "/auth";
  return raw.replace(/\/$/, "");
};

export const loginApi = async (
  request: LoginRequest,
): Promise<LoginResponse> => {
  const { email, password } = request;
  const base = buildAuthBaseUrl();
  const url = /\/auth$/i.test(base) ? `${base}/login` : `${base}/auth/login`;
  try {
    const { data } = await axios.post<Partial<LoginResponse>>(url, {
      email,
      password,
    });
    if (!data.accessToken) throw new Error("Missing accessToken in response");
    return { accessToken: data.accessToken };
  } catch (err) {
    if (err && (err as AxiosError).isAxiosError) {
      const axErr = err as AxiosError<unknown>;
      const status = axErr.response?.status;
      let serverMsg = axErr.message;
      const respData = axErr.response?.data as unknown;
      if (respData && typeof respData === "object") {
        const rec = respData as Record<string, unknown>;
        const messageVal = rec.message;
        const errorVal = rec.error;
        if (typeof messageVal === "string" && messageVal.trim()) {
          serverMsg = messageVal;
        } else if (typeof errorVal === "string" && errorVal.trim()) {
          serverMsg = errorVal;
        }
      }
      throw new Error(
        `Login failed${status ? ` (${status})` : ""}: ${serverMsg}`,
      );
    }
    throw err as Error;
  }
};

export const useLoginMutation = (
  options?: UseMutationOptions<LoginResponse, unknown, LoginRequest, unknown>,
) => {
  return useMutation<LoginResponse, unknown, LoginRequest>({
    mutationKey: ["auth", "login"],
    mutationFn: loginApi,
    ...options,
  });
};
