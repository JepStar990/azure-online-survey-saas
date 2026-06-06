import React, { useState, useEffect } from 'react';
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication, EventType } from '@azure/msal-browser';
import { msalConfig } from './msalConfig';

// Create MSAL instance once (singleton)
const msalInstance = new PublicClientApplication(msalConfig);

/**
 * Wraps the app with MSAL authentication context.
 * Initializes the MSAL instance and handles redirect promises before rendering children.
 */
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [initialized, setInitialized] = useState(false);
  const [initError, setInitError] = useState<string | null>(null);

  useEffect(() => {
    const init = async () => {
      try {
        // MSAL v3 requires explicit initialization before any operations
        await msalInstance.initialize();

        // Handle redirect promise — processes the token response when returning from AAD login
        await msalInstance.handleRedirectPromise();

        // Optional: listen for login failures
        msalInstance.addEventCallback((event) => {
          if (event.eventType === EventType.LOGIN_FAILURE) {
            console.error('MSAL login failure:', event.error);
            setInitError(event.error?.errorMessage || 'Login failed. Please try again.');
          }
        });

        setInitialized(true);
      } catch (err: any) {
        console.error('MSAL initialization failed:', err);
        setInitError(err?.errorMessage || err?.message || 'Failed to initialize authentication.');
      }
    };

    init();
  }, []);

  // Show loading while MSAL initializes
  if (!initialized) {
    return (
      <div style={{
        display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
        minHeight: '100vh', fontFamily: 'Segoe UI, sans-serif', background: '#f8fafc',
      }}>
        {initError ? (
          <div style={{ textAlign: 'center', maxWidth: 400, padding: '2rem' }}>
            <h2 style={{ color: '#dc2626', marginBottom: '0.5rem' }}>Authentication Error</h2>
            <p style={{ color: '#64748b', marginBottom: '1rem' }}>{initError}</p>
            <button onClick={() => window.location.reload()} className="btn btn-primary">
              Retry
            </button>
          </div>
        ) : (
          <>
            <div style={{
              width: 40, height: 40,
              border: '4px solid #e2e8f0', borderTopColor: '#3b82f6',
              borderRadius: '50%', animation: 'spin 0.8s linear infinite',
              marginBottom: '1rem',
            }} />
            <p style={{ color: '#64748b' }}>Initializing...</p>
            <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
          </>
        )}
      </div>
    );
  }

  return (
    <MsalProvider instance={msalInstance}>
      {children}
    </MsalProvider>
  );
};

export { msalInstance };
