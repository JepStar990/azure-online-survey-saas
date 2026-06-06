import React from 'react';
import { Link } from 'react-router-dom';

/** 404 fallback page. */
const NotFoundPage: React.FC = () => (
  <div style={{
    display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
    minHeight: '100vh', textAlign: 'center', padding: '2rem',
  }}>
    <h1 style={{ fontSize: '4rem', color: '#d1d5db', marginBottom: '0.5rem' }}>404</h1>
    <p style={{ color: '#64748b', marginBottom: '1.5rem', fontSize: '1.1rem' }}>
      The page you're looking for doesn't exist.
    </p>
    <Link to="/" className="btn btn-primary">Go to Dashboard</Link>
  </div>
);

export default NotFoundPage;
