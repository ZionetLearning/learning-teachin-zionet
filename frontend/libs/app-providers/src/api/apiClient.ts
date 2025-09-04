import axios, { InternalAxiosRequestConfig, AxiosHeaders } from "axios";
import { getCsrf } from "@app-providers";

/**
 * Shared Axios instance used across the app. Automatically:
 *  - Injects Authorization: Bearer <accessToken> header (if present in localStorage)
 *  - Injects X-CSRF-Token header (if available via getCsrf())
 *  - Skips auth header injection for login / refresh / logout endpoints
 */
export const apiClient = axios.create({
  withCredentials: true,
});

// Endpoints that must NOT send the bearer token
const AUTH_SKIP_PATHS = ["/login", "/refresh-tokens", "/logout"];

const getAccessTokenFromStorage = (): { accessToken?: string } | null => {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem("credentials");
    if (!raw) return null;
    return JSON.parse(raw) as { accessToken?: string };
  } catch {
    return null;
  }
};

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (!(config.headers instanceof AxiosHeaders)) {
    config.headers = new AxiosHeaders(config.headers || {});
  }
  const headers = config.headers as AxiosHeaders;

  const url = config.url || "";
  let pathname = "";
  try {
    pathname = new URL(
      url,
      typeof window !== "undefined"
        ? window.location.origin
        : "http://localhost",
    ).pathname;
  } catch {
    pathname = url;
  }
  const skipAuth = AUTH_SKIP_PATHS.includes(pathname);
  if (!skipAuth) {
    const token = getAccessTokenFromStorage()?.accessToken;
    if (token && !headers.has("Authorization")) {
      headers.set("Authorization", `Bearer ${token}`);
    }
  }

  const csrf = getCsrf();
  if (csrf && !headers.has("X-CSRF-Token")) {
    headers.set("X-CSRF-Token", csrf);
  }
  return config;
});

export default apiClient;
