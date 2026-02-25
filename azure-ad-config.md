# Azure AD configuration (notes)

1. In the Azure Portal register two apps (or run the helper script in this repo):
   - `SurveyApi` (API) - expose an API scope (e.g., `api://{clientId}/access_as_user`)
   - `SurveyWeb` (SPA) - set redirect URI to `http://localhost:5173` (Vite default)

   You can try the CLI helper: `azure/register-apps.ps1` (PowerShell) which creates basic apps. Note: you may need to finish scope exposure and permissions in the portal depending on tenant permissions.

2. Grant `SurveyWeb` permission to call `SurveyApi` (API permissions -> Add a permission -> My APIs).

3. Configure `backend/appsettings.json` Authority and Audience values with your tenant and API client id.

4. For multi-tenant SaaS, during app registration: set `Supported account types` to `Accounts in any organizational directory (Any Azure AD directory - Multitenant)`.

5. Add the following GitHub repository secrets for CI/CD deploys:
   - `AZURE_CREDENTIALS` — service principal JSON used by `azure/login` action
   - `AZURE_WEBAPP_NAME` — name of the App Service for the backend
   - `AZURE_WEBAPP_PUBLISH_PROFILE` — publish profile XML for App Service (optional alternative to `AZURE_CREDENTIALS`)
   - `AZURE_STORAGE_ACCOUNT` and `AZURE_STORAGE_KEY` — storage account credentials for static website uploads

6. App Service + Key Vault (recommended production flow):
   - The Bicep template (`infra/main.bicep`) creates a Key Vault and stores the SQL connection string under the secret name `sql-connection-string`.
   - The App Service is configured with a system-assigned managed identity and an app setting `ConnectionStrings:DefaultConnection` that uses an App Service Key Vault reference in the form:
     `@Microsoft.KeyVault(SecretUri=https://<your-keyvault>.vault.azure.net/secrets/sql-connection-string)`
   - You should store additional secrets (recommended) in Key Vault rather than app settings: e.g., `AzureAd:Authority` and `AzureAd:Audience`. After adding them to Key Vault, update the App Service app settings to reference those secrets (or modify the Bicep template to include them similarly).
   - The Bicep template grants the App Service managed identity `get` permission on secrets so Key Vault references work without exposing secrets in the portal or CI logs.

7. Post-deploy steps (portal or CLI):
   - Verify the App Service identity is `SystemAssigned` and the objectId is present.
   - In the Key Vault Access policies (or RBAC), confirm the web app's principal has `get` permission for secrets.
   - Add any missing Key Vault secrets for `AzureAd:Authority` and `AzureAd:Audience` if you prefer Key Vault references. Alternatively, set those values as App Service app settings (preferably as Key Vault references for security).

8. Helpful CLI commands (replace placeholders):
   - Create a secret:
     ```bash
     az keyvault secret set --vault-name <your-keyvault> -n AzureAd--Authority --value "https://login.microsoftonline.com/<tenant>/v2.0"
     az keyvault secret set --vault-name <your-keyvault> -n AzureAd--Audience --value "api://<api-client-id>"
     ```
   - Verify Key Vault reference works in App Service by checking `Configuration` -> `Application settings` and confirming the setting value shows as a Key Vault reference.


