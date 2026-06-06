import React from 'react';
import { useAuth } from '../auth/useAuth';
import { Navigate } from 'react-router-dom';

/** Login page — shows a sign-in button or redirects if already authenticated. */
const LoginPage: React.FC = () => {
  const { isAuthenticated, login } = useAuth();

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <div style={{
      display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
      minHeight: '100vh', background: '#f8fafc', padding: '1rem',
    }}>
      <div style={{ textAlign: 'center', maxWidth: 400 }}>
        <h1 style={{ fontSize: '1.75rem', color: '#0f172a', marginBottom: '0.5rem' }}>
          SurveySaaS
        </h1>
        <p style={{ color: '#64748b', marginBottom: '2rem' }}>
          Create, distribute, and analyze surveys with Azure AD integration.
        </p>
        <button onClick={login} className="btn btn-primary" style={{ padding: '0.75rem 2rem', fontSize: '1rem' }}>
          Sign in with Microsoft
        </button>
      </div>
    </div>
  );
};

export default LoginPage;
