import React, { Suspense } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './auth/AuthProvider';
import { AppShell } from './components/layout/AppShell';
import { RequireAuth } from './auth/RequireAuth';
import { LoadingSpinner } from './components/shared/LoadingSpinner';
import { setTokenGetter, setLoginRedirect } from './api/client';
import { useAuth } from './auth/useAuth';

// --- Lazy-loaded pages ---
const LoginPage = React.lazy(() => import('./pages/LoginPage'));
const DashboardPage = React.lazy(() => import('./pages/DashboardPage'));
const SurveyListPage = React.lazy(() => import('./pages/SurveyListPage'));
const SurveyBuilderPage = React.lazy(() => import('./pages/SurveyBuilderPage'));
const SurveyTakePage = React.lazy(() => import('./pages/SurveyTakePage'));
const SurveyThankYouPage = React.lazy(() => import('./pages/SurveyThankYouPage'));
const ResultsPage = React.lazy(() => import('./pages/ResultsPage'));
const NotFoundPage = React.lazy(() => import('./pages/NotFoundPage'));

// React Query client with sensible defaults
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,       // 30s before refetching
      retry: 2,
      refetchOnWindowFocus: false,
    },
  },
});

/**
 * Inner component that wires the MSAL token getter into the API client.
 * Must be rendered inside AuthProvider so useAuth() works.
 */
const TokenBridger: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { getAccessToken, login } = useAuth();
  React.useEffect(() => {
    setTokenGetter(getAccessToken);
    setLoginRedirect(login);
  }, [getAccessToken, login]);
  return <>{children}</>;
};

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <TokenBridger>
          <BrowserRouter>
            <Suspense fallback={<LoadingSpinner message="Loading page..." />}>
              <Routes>
                {/* Public routes */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/s/:publicLinkId" element={<SurveyTakePage />} />
                <Route path="/s/:publicLinkId/thanks" element={<SurveyThankYouPage />} />

                {/* Authenticated routes — wrapped in AppShell layout */}
                <Route element={
                  <RequireAuth>
                    <AppShell />
                  </RequireAuth>
                }>
                  <Route path="/" element={<DashboardPage />} />
                  <Route path="/surveys" element={<SurveyListPage />} />
                  <Route path="/surveys/new" element={<SurveyBuilderPage />} />
                  <Route path="/surveys/:id/edit" element={<SurveyBuilderPage />} />
                  <Route path="/surveys/:id/results" element={<ResultsPage />} />
                </Route>

                {/* Catch-all */}
                <Route path="*" element={<NotFoundPage />} />
              </Routes>
            </Suspense>
          </BrowserRouter>
        </TokenBridger>
      </AuthProvider>
    </QueryClientProvider>
  );
}
