import React from 'react';

/** Simple loading spinner with optional message. Used as a fallback during lazy loading and data fetches. */
export const LoadingSpinner: React.FC<{ message?: string }> = ({ message = 'Loading...' }) => (
  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', padding: '3rem' }}>
    <div style={{
      width: 40, height: 40,
      border: '4px solid #e2e8f0',
      borderTopColor: '#3b82f6',
      borderRadius: '50%',
      animation: 'spin 0.8s linear infinite',
    }} />
    <p style={{ marginTop: '1rem', color: '#64748b' }}>{message}</p>
    <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
  </div>
);
