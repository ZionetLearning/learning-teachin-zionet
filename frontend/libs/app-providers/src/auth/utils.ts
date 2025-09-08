export const decodeJwtExp = (token: string): number | undefined => {
  try {
    const [, payload] = token.split(".");
    if (!payload) return undefined;
    const json = JSON.parse(
      atob(
        payload.replace(/-/g, "+").replace(/_/g, "/") +
          "=".repeat((4 - (payload.length % 4)) % 4),
      ),
    );
    if (typeof json.exp === "number") {
      return json.exp * 1000;
    }
  } catch (e) {
    console.warn("Failed to decode JWT expiration", e);
  }
  return undefined;
};

export const decodeJwtPayload = (token: string): Record<string, unknown> | undefined => {
  try {
    const [, payload] = token.split(".");
    if (!payload) return undefined;
    const json = JSON.parse(
      atob(
        payload.replace(/-/g, "+").replace(/_/g, "/") +
          "=".repeat((4 - (payload.length % 4)) % 4),
      ),
    );
    return json;
  } catch (e) {
    console.warn("Failed to decode JWT payload", e);
  }
  return undefined;
};

let csrfTokenMemory: string | null = null;

const CSRF_KEY = "csrfToken";

export const getStoredCsrf = () => {
  if (typeof window === "undefined") return null;
  try {
    return sessionStorage.getItem(CSRF_KEY);
  } catch (e) {
    console.warn("Failed to get CSRF token from sessionStorage", e);
    return null;
  }
};

export const storeCsrf = (v: string) => {
  csrfTokenMemory = v;
  if (typeof window === "undefined") return;
  try {
    sessionStorage.setItem(CSRF_KEY, v);
  } catch (e) {
    console.warn("Failed to store CSRF token in sessionStorage", e);
  }
};

export const clearCsrf = () => {
  csrfTokenMemory = null;
  if (typeof window === "undefined") return;
  try {
    sessionStorage.removeItem(CSRF_KEY);
  } catch (e) {
    console.warn("Failed to remove CSRF token from sessionStorage", e);
  }
};

export const getCsrf = () => csrfTokenMemory || getStoredCsrf();

export const extractCsrf = (data: unknown): string | undefined => {
  if (data && typeof data === "object" && "csrfToken" in data) {
    const v = (data as { csrfToken?: unknown }).csrfToken;
    return typeof v === "string" ? v : undefined;
  }
  return undefined;
};
