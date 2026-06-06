import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';

/** Navigation sidebar — role-aware (shows links based on user roles). */
export const Sidebar: React.FC = () => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) return null;

  const linkStyle = (isActive: boolean): React.CSSProperties => ({
    display: 'block',
    padding: '0.6rem 1rem',
    color: isActive ? '#f8fafc' : '#94a3b8',
    background: isActive ? '#334155' : 'transparent',
    textDecoration: 'none',
    borderRadius: 6,
    fontSize: '0.9rem',
    marginBottom: 2,
  });

  return (
    <nav style={{
      width: 220,
      background: '#1e293b',
      padding: '1rem',
      minHeight: 'calc(100vh - 56px)',
      borderRight: '1px solid #334155',
    }}>
      <NavLink to="/" end style={({ isActive }) => linkStyle(isActive)}>
        Dashboard
      </NavLink>
      <NavLink to="/surveys" style={({ isActive }) => linkStyle(isActive)}>
        Surveys
      </NavLink>
      <NavLink to="/surveys/new" style={({ isActive }) => linkStyle(isActive)}>
        + New Survey
      </NavLink>
    </nav>
  );
};
