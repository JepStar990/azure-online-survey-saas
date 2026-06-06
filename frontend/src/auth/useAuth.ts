import { useMsal, useIsAuthenticated, useAccount } from '@azure/msal-react';
import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { apiScopes } from './msalConfig';

/**
 * Hook providing authentication state and actions.
 * Wraps MSAL React hooks into a simplified interface for the rest of the app.
 */
export function useAuth() {
  const { instance, accounts, inProgress } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(accounts[0] || {});

  /** Trigger Azure AD login redirect. Throws on failure so callers can show errors. */
  const login = async () => {
    await instance.loginRedirect({
      scopes: apiScopes,
      prompt: 'select_account',
    });
  };

  /** Sign out and clear session. */
  const logout = async () => {
    await instance.logoutRedirect({
      postLogoutRedirectUri: window.location.origin,
    });
  };

  /**
   * Acquire an access token silently. Tries multiple approaches:
   * 1. Use the active account from MSAL React hooks
   * 2. Fall back to getAllAccounts() if the hook account isn't ready
   * 3. If interactive auth is required, redirect to login
   *
   * Returns null only if the user is truly not authenticated.
   */
  const getAccessToken = async (): Promise<string | null> => {
    // Try to get a valid account — use the hook account first, then fall back to cache
    const activeAccount = account || instance.getAllAccounts()[0] || null;

    if (!activeAccount) {
      console.warn('No account found in MSAL cache — user needs to re-authenticate');
      return null;
    }

    try {
      const response = await instance.acquireTokenSilent({
        scopes: apiScopes,
        account: activeAccount,
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        // Token expired, claims required, or consent needed — silently try to get a new one
        console.warn('Interaction required, attempting to re-acquire token silently');
        try {
          // Try with forceRefresh to get a fresh token from the server
          const response = await instance.acquireTokenSilent({
            scopes: apiScopes,
            account: activeAccount,
            forceRefresh: true,
          });
          return response.accessToken;
        } catch (retryError) {
          console.error('Token re-acquisition failed, redirecting to login');
          await login();
          return null;
        }
      }
      console.error('Token acquisition failed:', error);
      return null;
    }
  };

  /** Extract user display name from the active account. */
  const displayName = account?.name ?? account?.username ?? 'User';

  /** Extract Azure AD object ID (oid claim). */
  const userId = account?.localAccountId ?? null;

  /** Extract tenant ID from the account. */
  const tenantId = account?.tenantId ?? null;

  /** Extract roles from idTokenClaims. */
  const roles: string[] = (account?.idTokenClaims?.roles as string[]) ?? [];

  /** Whether MSAL is currently processing a redirect or interaction. */
  const isLoading = inProgress !== 'none';

  return {
    isAuthenticated,
    isLoading,
    account,
    displayName,
    userId,
    tenantId,
    roles,
    login,
    logout,
    getAccessToken,
  };
}
