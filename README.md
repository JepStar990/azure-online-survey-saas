# Azure Online Survey SaaS (starter)

Minimal starter workspace for an Azure AD-integrated online survey SaaS.

Quick steps (local development):

- Backend (.NET 8):
  - Build: `dotnet build "./backend"`
  - Run: `dotnet run --project "./backend"`
  - Tests: `dotnet test "./backend/Tests"`

- Frontend (Vite + React + TypeScript):
  - Install: `cd frontend && npm install`
  - Dev: `npm run dev`

Notes:
- The backend contains placeholder Azure AD configuration in `backend/appsettings.json`.
- See `azure-ad-config.md` for Azure AD app registration guidance.
 - Use the infra Bicep template in `infra/main.bicep` to provision App Service, Storage (static website), SQL, and App Insights.
 - Use `azure/register-apps.ps1` to bootstrap Azure AD app registrations (may require manual portal steps).
 - CI/CD workflows: see `.github/workflows/ci.yml` and `.github/workflows/cd.yml` (GitHub Actions). Set required secrets as described in `azure-ad-config.md`.
 - Use the infra Bicep template in `infra/main.bicep` to provision App Service, Storage (static website), SQL, Key Vault, and App Insights.
   - Bicep config will create a Key Vault, store the SQL connection string in the secret `sql-connection-string`, and configure the App Service with a system-assigned identity and an app setting `ConnectionStrings:DefaultConnection` which references the Key Vault secret.
   - For production, store `AzureAd:Authority` and `AzureAd:Audience` in Key Vault and reference them from App Service (or add them as App Service app settings if you prefer).

Production deploy overview:

1. Register Azure AD apps (API + SPA) and record tenant id and client ids.
2. Add required secrets to GitHub (see `azure-ad-config.md`) and Key Vault as needed.
3. Deploy infra (Bicep) using the `infra/main.bicep` template (CD workflow can do this automatically using the secrets).
4. Deploy backend and frontend (CD workflow uploads the static site and deploys to App Service).

See `azure-ad-config.md` for detailed Key Vault, App Service, and Azure AD configuration examples and CLI snippets.
