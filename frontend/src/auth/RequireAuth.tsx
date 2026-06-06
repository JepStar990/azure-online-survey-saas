import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';
import { LoadingSpinner } from '../components/shared/LoadingSpinner';

/**
 * Route guard: redirects unauthenticated users to the login page.
 * Shows a loading spinner while MSAL initializes.
 */
export const RequireAuth: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  // During MSAL initialization, isAuthenticated may briefly be false
  // even if the user has a valid session. The MsalAuthenticationTemplate
  // handles this automatically, but for route guards we need a brief
  // loading state to avoid flashing the login redirect.
  const [isInitialized, setIsInitialized] = React.useState(false);

  React.useEffect(() => {
    // MSAL initializes asynchronously; give it a tick
    const timer = setTimeout(() => setIsInitialized(true), 500);
    return () => clearTimeout(timer);
  }, []);

  if (!isInitialized) {
    return <LoadingSpinner message="Checking authentication..." />;
  }

  if (!isAuthenticated) {
    // Redirect to login, preserving the intended destination
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};
