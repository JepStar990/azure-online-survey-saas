import React, { useState } from 'react';
import { useAuth } from '../auth/useAuth';
import { Navigate } from 'react-router-dom';

/** Login page — shows a sign-in button or redirects if already authenticated. */
const LoginPage: React.FC = () => {
  const { isAuthenticated, login } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  const handleLogin = async () => {
    setError(null);
    setLoading(true);
    try {
      await login();
    } catch (err: any) {
      console.error('Login error:', err);
      setError(err?.errorMessage || err?.message || 'Sign-in failed. Please try again.');
      setLoading(false);
    }
    // If loginRedirect succeeds, the browser navigates away — loading stays true
    // If it fails (e.g., popup blocked), we show the error
    setTimeout(() => setLoading(false), 3000);
  };

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

        {error && (
          <div style={{
            background: '#fef2f2', border: '1px solid #fecaca', borderRadius: 8,
            padding: '0.75rem 1rem', marginBottom: '1rem', textAlign: 'left',
          }}>
            <p style={{ color: '#dc2626', fontSize: '0.875rem', margin: 0 }}>{error}</p>
          </div>
        )}

        <button
          onClick={handleLogin}
          disabled={loading}
          className="btn btn-primary"
          style={{ padding: '0.75rem 2rem', fontSize: '1rem', opacity: loading ? 0.7 : 1 }}
        >
          {loading ? 'Redirecting to Microsoft...' : 'Sign in with Microsoft'}
        </button>

        <p style={{ color: '#94a3b8', fontSize: '0.75rem', marginTop: '1.5rem' }}>
          Uses your Azure AD / Microsoft 365 work account.
        </p>
      </div>
    </div>
  );
};

export default LoginPage;
