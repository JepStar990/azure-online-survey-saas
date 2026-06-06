import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

/**
 * Determine the API base URL based on the environment.
 * In development, the Vite dev server proxies /api requests to the backend.
 * In production, we point directly to the App Service URL.
 */
const getBaseUrl = (): string => {
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL;
  }
  return '/api/v1';
};

export const apiClient = axios.create({
  baseURL: getBaseUrl(),
  timeout: 30000,
  headers: { 'Content-Type': 'application/json' },
});

// --- Token management ---

let tokenGetter: (() => Promise<string | null>) | null = null;
let loginRedirect: (() => Promise<void>) | null = null;

export function setTokenGetter(getter: () => Promise<string | null>) {
  tokenGetter = getter;
}

export function setLoginRedirect(fn: () => Promise<void>) {
  loginRedirect = fn;
}

// --- Request interceptor: attach Bearer token, retry on failure ---

apiClient.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  if (tokenGetter) {
    // Try up to 3 times with increasing delays to let MSAL hydrate
    let token: string | null = null;
    for (let attempt = 0; attempt < 3; attempt++) {
      if (attempt > 0) {
        await new Promise(r => setTimeout(r, 500 * attempt));
      }
      token = await tokenGetter();
      if (token) break;
    }

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    } else if (loginRedirect) {
      // No token after retries — force re-login
      console.warn('No access token available, redirecting to login');
      await loginRedirect();
      throw new axios.Cancel('Redirecting to login');
    }
  }
  return config;
});

// --- Response interceptor: surface errors clearly ---

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.code === 'ERR_NETWORK') {
      console.error('Network error — API may be unreachable');
    }
    if (error.response?.status === 401) {
      console.warn('API returned 401 — token may be invalid or expired');
      if (loginRedirect) {
        loginRedirect();
      }
    }
    return Promise.reject(error);
  }
);
