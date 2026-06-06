import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { Sidebar } from './Sidebar';

/**
 * Root layout shell: header + sidebar + content area.
 * Uses React Router's Outlet to render the matched child route.
 */
export const AppShell: React.FC = () => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Header />
      <div style={{ display: 'flex', flex: 1 }}>
        <Sidebar />
        <main style={{ flex: 1, padding: '1.5rem', background: '#f8fafc', minHeight: 0 }}>
          <React.Suspense fallback={<div>Loading...</div>}>
            <Outlet />
          </React.Suspense>
        </main>
      </div>
    </div>
  );
};
