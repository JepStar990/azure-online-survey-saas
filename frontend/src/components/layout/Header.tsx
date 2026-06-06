import React from 'react';
import { useAuth } from '../../auth/useAuth';

/** Application header with branding and user controls. */
export const Header: React.FC = () => {
  const { isAuthenticated, displayName, logout } = useAuth();

  return (
    <header style={{
      background: '#1e293b',
      color: '#f8fafc',
      padding: '0 1.5rem',
      height: 56,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      borderBottom: '1px solid #334155',
    }}>
      <a href="/" style={{ color: '#f8fafc', textDecoration: 'none', fontSize: '1.1rem', fontWeight: 600 }}>
        SurveySaaS
      </a>

      {isAuthenticated && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <span style={{ fontSize: '0.875rem', color: '#94a3b8' }}>{displayName}</span>
          <button onClick={logout} style={{
            background: 'transparent',
            color: '#94a3b8',
            border: '1px solid #475569',
            borderRadius: 6,
            padding: '0.35rem 0.75rem',
            cursor: 'pointer',
            fontSize: '0.8rem',
          }}>
            Sign out
          </button>
        </div>
      )}
    </header>
  );
};
