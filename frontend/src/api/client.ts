import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

/**
 * Determine the API base URL based on the environment.
 * In development, the Vite dev server proxies /api requests to the backend.
 * In production, we point directly to the App Service URL.
 */
const getBaseUrl = (): string => {
  // If a full API URL is provided via env, use it directly (production)
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL;
  }
  // In development, use relative path — Vite proxy handles forwarding to backend
  return '/api/v1';
};

/**
 * Axios instance pre-configured with the API base URL and Bearer token interceptor.
 * The token is acquired from MSAL at request time via the onRequest callback.
 */
export const apiClient = axios.create({
  baseURL: getBaseUrl(),
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Attach the Bearer token to every outgoing request.
 * The token getter is set by the app's auth initialization.
 */
let tokenGetter: (() => Promise<string | null>) | null = null;

export function setTokenGetter(getter: () => Promise<string | null>) {
  tokenGetter = getter;
}

apiClient.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  if (tokenGetter) {
    const token = await tokenGetter();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }
  return config;
});

/**
 * Handle common API errors uniformly.
 * Components can catch specific errors by checking error.response.status.
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      console.warn('API returned 401 Unauthorized');
    }
    if (error.response?.status === 403) {
      console.warn('API returned 403 Forbidden — insufficient permissions');
    }
    if (error.response?.status === 429) {
      console.warn('API returned 429 Too Many Requests — rate limited');
    }
    return Promise.reject(error);
  }
);
