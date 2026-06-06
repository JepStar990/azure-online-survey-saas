import React from 'react';
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication, InteractionType } from '@azure/msal-browser';
import { msalConfig } from './msalConfig';

// Create MSAL instance once (singleton)
const msalInstance = new PublicClientApplication(msalConfig);

/**
 * Wraps the app with MSAL authentication context.
 * Provides token acquisition, login/logout, and user identity throughout the component tree.
 */
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <MsalProvider instance={msalInstance}>
      {children}
    </MsalProvider>
  );
};

export { msalInstance };
