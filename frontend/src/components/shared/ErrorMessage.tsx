import React from 'react';

/** Displays an error message with an optional retry button. */
export const ErrorMessage: React.FC<{
  message: string;
  onRetry?: () => void;
}> = ({ message, onRetry }) => (
  <div style={{
    padding: '1.5rem',
    background: '#fef2f2',
    border: '1px solid #fecaca',
    borderRadius: 8,
    textAlign: 'center',
    maxWidth: 500,
    margin: '2rem auto',
  }}>
    <p style={{ color: '#dc2626', marginBottom: '0.75rem' }}>{message}</p>
    {onRetry && (
      <button onClick={onRetry} style={{
        padding: '0.5rem 1.25rem',
        background: '#dc2626',
        color: '#fff',
        border: 'none',
        borderRadius: 6,
        cursor: 'pointer',
      }}>
        Retry
      </button>
    )}
  </div>
);
