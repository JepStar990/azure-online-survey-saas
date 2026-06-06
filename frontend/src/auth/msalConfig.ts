import { Configuration, LogLevel } from '@azure/msal-browser';

/**
 * Azure AD MSAL configuration for the SPA.
 * Values are read from Vite environment variables at build time.
 * For local development, create a .env file with VITE_AZURE_* variables.
 *
 * @see azure-ad-config.md for app registration instructions
 */
export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID || '{spa-client-id}',
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID || '{tenant-id}'}`,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: true,
  },
  cache: {
    cacheLocation: 'sessionStorage', // Never use localStorage — security best practice
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            break;
          case LogLevel.Warning:
            console.warn(message);
            break;
          case LogLevel.Info:
            console.info(message);
            break;
          default:
            console.debug(message);
        }
      },
      logLevel: LogLevel.Warning,
    },
  },
};

/**
 * Scopes requested for the Survey API.
 * The scope value must match what was exposed in the Azure AD app registration.
 * Format: api://{api-client-id}/access_as_user
 */
export const apiScopes = [
  `api://${import.meta.env.VITE_AZURE_API_CLIENT_ID || '{api-client-id}'}/access_as_user`,
];
