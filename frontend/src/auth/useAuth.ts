import { useMsal, useIsAuthenticated, useAccount } from '@azure/msal-react';
import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { apiScopes } from './msalConfig';

/**
 * Hook providing authentication state and actions.
 * Wraps MSAL React hooks into a simplified interface for the rest of the app.
 */
export function useAuth() {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const account = useAccount(accounts[0] || {});

  /** Trigger Azure AD login flow. */
  const login = async () => {
    try {
      await instance.loginRedirect({
        scopes: apiScopes,
        prompt: 'select_account',
      });
    } catch (e) {
      console.error('Login failed:', e);
    }
  };

  /** Sign out and clear session. */
  const logout = async () => {
    try {
      await instance.logoutRedirect({
        postLogoutRedirectUri: window.location.origin,
      });
    } catch (e) {
      console.error('Logout failed:', e);
    }
  };

  /**
   * Acquire an access token silently, or prompt the user to re-authenticate if needed.
   * Returns null if not authenticated.
   */
  const getAccessToken = async (): Promise<string | null> => {
    if (!account) return null;
    try {
      const response = await instance.acquireTokenSilent({
        scopes: apiScopes,
        account,
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        // Token expired or needs re-authentication — redirect to login
        await instance.acquireTokenRedirect({ scopes: apiScopes, account });
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

  return {
    isAuthenticated,
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
