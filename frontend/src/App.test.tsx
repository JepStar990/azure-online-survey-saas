import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import App from './App';

// Mock MSAL to avoid actual auth interactions in tests
vi.mock('@azure/msal-react', () => ({
  MsalProvider: ({ children }: any) => children,
  useMsal: () => ({
    instance: {
      loginRedirect: vi.fn(),
      logoutRedirect: vi.fn(),
      acquireTokenSilent: vi.fn().mockResolvedValue({ accessToken: 'test-token' }),
    },
    accounts: [],
  }),
  useIsAuthenticated: () => false,
  useAccount: () => null,
  AuthenticatedTemplate: ({ children }: any) => null,
  UnauthenticatedTemplate: ({ children }: any) => children,
}));

// Mock BrowserRouter to avoid navigation errors in JSDOM
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual };
});

describe('App', () => {
  it('renders login page when unauthenticated', async () => {
    render(<App />);
    // Should show the login page since we're not authenticated
    expect(await screen.findByText(/SurveySaaS/i)).toBeDefined();
  });
});
