# Azure AD Configuration Guide

Complete guide for setting up Azure AD (Entra ID) authentication for the SurveySaaS platform.

## Overview

SurveySaaS uses Azure AD to authenticate users and protect API endpoints. Two app registrations are required:

| App Registration | Type | Purpose |
|---|---|---|
| **SurveyApi** | API | Backend API that validates JWT tokens and enforces authorization |
| **SurveyWeb** | SPA (Single Page Application) | Frontend React app that acquires tokens via MSAL |

## Step 1: Register the API Application (SurveyApi)

1. Go to **Azure Portal** → **Microsoft Entra ID** → **App registrations** → **New registration**
2. Configure:
   - **Name**: `SurveyApi`
   - **Supported account types**: `Accounts in any organizational directory (Any Azure AD directory - Multitenant)` — choose this for SaaS multi-tenant support
   - **Redirect URI**: Leave blank (no redirect needed for API)
3. Click **Register**
4. Note the **Application (client) ID** — this is your `{api-client-id}`
5. Go to **Expose an API** → **Add a scope**:
   - **Scope name**: `access_as_user`
   - **Who can consent?**: `Admins and users`
   - **Admin consent display name**: `Access Survey API`
   - **Admin consent description**: `Allows the app to create, manage, and view surveys on your behalf`
   - **State**: `Enabled`
6. Click **Add scope**
7. Note the scope URI: `api://{api-client-id}/access_as_user`

### Configure App Roles (Optional — for RBAC)

1. Go to **App roles** → **Create app role**:
   - **Display name**: `TenantAdmin`
   - **Allowed member types**: `Users/Groups`
   - **Value**: `TenantAdmin`
   - **Description**: `Full tenant administration privileges`
2. Repeat for: `SurveyCreator`, `SurveyViewer`, `Respondent`
3. Assign roles to users/ groups under **Enterprise applications** → **SurveyApi** → **Users and groups**

## Step 2: Register the SPA Application (SurveyWeb)

1. Go to **App registrations** → **New registration**
2. Configure:
   - **Name**: `SurveyWeb`
   - **Supported account types**: `Accounts in any organizational directory (Any Azure AD directory - Multitenant)`
   - **Redirect URI**: `Single-page application (SPA)` → `http://localhost:5173` (add production URL later)
3. Click **Register**
4. Note the **Application (client) ID** — this is your `{spa-client-id}`
5. Go to **Authentication**:
   - Ensure **Access tokens** and **ID tokens** are checked under Implicit grant
   - Add your production redirect URI (e.g., `https://your-domain.com`)
6. Go to **API permissions** → **Add a permission**:
   - Select **My APIs** → **SurveyApi**
   - Check `access_as_user`
   - Click **Add permissions**
7. Click **Grant admin consent** (requires admin privileges)

## Step 3: Configure Backend Settings

Update `backend/src/SurveyApi.Web/appsettings.json` (or `appsettings.Development.json` for local dev):

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "{your-tenant-id}",
    "ClientId": "{api-client-id}",
    "Authority": "https://login.microsoftonline.com/{your-tenant-id}/v2.0",
    "Audience": "api://{api-client-id}"
  }
}
```

### Multi-tenant Configuration

For multi-tenant SaaS (Phase 2+), set `ValidateIssuer` to `false` in `Program.cs` and use the common authority:

```json
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/common/v2.0",
    "Audience": "api://{api-client-id}"
  }
}
```

## Step 4: Configure Frontend Settings

Create a `.env` file in the `frontend/` directory:

```env
VITE_AZURE_CLIENT_ID={spa-client-id}
VITE_AZURE_TENANT_ID={your-tenant-id}
VITE_AZURE_API_CLIENT_ID={api-client-id}
```

For production, set these as environment variables in your CI/CD pipeline or Azure Static Web App configuration.

## Step 5: Verify Authentication Flow

1. Start the backend: `dotnet run --project backend/src/SurveyApi.Web`
2. Start the frontend: `cd frontend && npm run dev`
3. Open `http://localhost:5173` in a browser
4. Click **Sign in with Microsoft**
5. After authenticating with your Azure AD account, you should be redirected back to the dashboard
6. The frontend acquires a token via MSAL, attaches it to API requests, and the backend validates it

## Troubleshooting

### "AADSTS50011: The redirect URI does not match"

**Cause**: The redirect URI in the SPA app registration doesn't match the actual URL.

**Fix**: Go to **App registrations** → **SurveyWeb** → **Authentication** → **Redirect URIs**. Add the exact URL where your frontend is running (include trailing slash if present, port number, and protocol).

### "AADSTS65001: The user or administrator has not consented"

**Cause**: Admin consent hasn't been granted for the API permissions.

**Fix**: Go to **App registrations** → **SurveyWeb** → **API permissions** → **Grant admin consent**.

### "401 Unauthorized from API"

**Cause**: Token is valid but missing the correct audience or scope.

**Fix**: Verify:
- `AzureAd:Audience` in backend matches the Application ID URI exactly (`api://{api-client-id}`)
- The frontend requests the correct scope: `api://{api-client-id}/access_as_user`
- The token hasn't expired (check browser dev tools network tab for 401 responses)

### "Invalid issuer" or "Invalid signature"

**Cause**: Tenant ID mismatch or using wrong authority URL.

**Fix**: Verify `AzureAd:Authority` uses the correct tenant ID or `common` for multi-tenant. Check the `iss` claim in the JWT at [jwt.ms](https://jwt.ms).

### "Could not find 'dotnet' host for the 'ARM64' architecture"

**Cause**: Running tests on ARM64 device without the proper runtime.

**Fix**: Tests are designed to run in CI (GitHub Actions). On ARM64, use `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` and `DOTNET_GCHeapHardLimit=0x20000000`. Alternatively, build only: `dotnet build`.

## CLI Helper Script

Use `azure/register-apps.ps1` to automate the app registration process:

```bash
# PowerShell (requires Azure CLI and admin consent permissions)
pwsh azure/register-apps.ps1 -TenantId "your-tenant-id" -RedirectUri "http://localhost:5173"
```

Note: The script may not be able to set all permissions depending on your tenant's admin consent settings. Manual steps in the Azure Portal may still be required for scope exposure and API permissions.

## Key Vault Integration (Production)

In production, Azure AD settings should be stored in Key Vault rather than appsettings.json:

```bash
# Store secrets in Key Vault
az keyvault secret set \
  --vault-name {keyvault-name} \
  -n AzureAd--Authority \
  --value "https://login.microsoftonline.com/{tenant-id}/v2.0"

az keyvault secret set \
  --vault-name {keyvault-name} \
  -n AzureAd--Audience \
  --value "api://{api-client-id}"

# Reference in App Service as Key Vault references:
# @Microsoft.KeyVault(SecretUri=https://{keyvault-name}.vault.azure.net/secrets/AzureAd--Authority)
# @Microsoft.KeyVault(SecretUri=https://{keyvault-name}.vault.azure.net/secrets/AzureAd--Audience)
```

The Bicep template in `infra/main.bicep` automatically:
- Creates a Key Vault
- Grants the App Service managed identity `get` and `list` permissions on secrets
- Configures the App Service to use Key Vault references for the SQL connection string
