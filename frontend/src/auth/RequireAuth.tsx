import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';

/**
 * Route guard: redirects unauthenticated users to the login page.
 * Shows a loading spinner while MSAL processes redirect responses.
 */
export const RequireAuth: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  // MSAL is processing a redirect response or performing an interaction
  if (isLoading) {
    return (
      <div style={{
        display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
        minHeight: '100vh', background: '#f8fafc',
      }}>
        <div style={{
          width: 40, height: 40,
          border: '4px solid #e2e8f0', borderTopColor: '#3b82f6',
          borderRadius: '50%', animation: 'spin 0.8s linear infinite',
          marginBottom: '1rem',
        }} />
        <p style={{ color: '#64748b' }}>Signing you in...</p>
        <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
      </div>
    );
  }

  if (!isAuthenticated) {
    // Redirect to login, preserving the intended destination
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};
