# SurveySaaS — Azure AD-Integrated Online Survey Platform

A production-ready, multi-tenant SaaS platform for creating, distributing, and analyzing surveys. Built with .NET 8, React + TypeScript, and Azure infrastructure.

## Architecture

```
backend/
├── src/
│   ├── SurveyApi.Domain/         # Entities, enums, value objects
│   ├── SurveyApi.Application/    # DTOs, services, validators, interfaces
│   ├── SurveyApi.Infrastructure/ # EF Core, repositories, auth helpers
│   └── SurveyApi.Web/            # Minimal API endpoints, middleware, composition root
├── tests/
│   ├── SurveyApi.UnitTests/      # Service & validator tests
│   └── SurveyApi.IntegrationTests/ # Endpoint tests with WebApplicationFactory

frontend/
├── src/
│   ├── auth/          # MSAL React configuration, AuthProvider, useAuth, RequireAuth
│   ├── api/           # Axios client, TanStack Query hooks (surveys, responses, analytics)
│   ├── pages/         # Login, Dashboard, SurveyList, SurveyBuilder, SurveyTake, Results
│   ├── components/    # layout (AppShell, Header, Sidebar), survey-builder, survey-take, shared
│   ├── types/         # TypeScript interfaces
│   └── styles/        # Global CSS
└── e2e/               # Playwright E2E tests

infra/
├── main.bicep         # Azure infrastructure as code
└── parameters.json    # Deployment parameters
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- [Azure subscription](https://azure.microsoft.com/free/)

## Quick Start (Local Development)

### 1. Clone the repository

```bash
git clone https://github.com/JepStar990/azure-online-survey-saas.git
cd azure-online-survey-saas
```

### 2. Run the backend

```bash
# Restore packages and build
dotnet restore azure-online-survey-saas.sln
dotnet build azure-online-survey-saas.sln

# Run the API (uses in-memory database by default in development)
dotnet run --project backend/src/SurveyApi.Web

# The API is now running at https://localhost:5000
# Swagger UI: https://localhost:5000/swagger
```

The backend uses an in-memory database when `UseInMemory` is `true` (default in development). No SQL Server setup is needed for local dev.

### 3. Run the frontend

```bash
cd frontend
npm install
npm run dev

# The frontend is now running at http://localhost:5173
# API requests are proxied to the backend automatically (see vite.config.ts)
```

### 4. Configure Azure AD (required for authentication)

1. Register two apps in the [Azure Portal](https://portal.azure.com) under **Azure Active Directory > App registrations**:
   - **SurveyApi** (API): Expose an API scope `access_as_user`
   - **SurveyWeb** (SPA): Set redirect URI to `http://localhost:5173`

2. Grant **SurveyWeb** permission to call **SurveyApi** (API Permissions > Add a permission > My APIs)

3. Create a `.env` file in `frontend/`:
   ```env
   VITE_AZURE_CLIENT_ID={spa-client-id}
   VITE_AZURE_TENANT_ID={your-tenant-id}
   VITE_AZURE_API_CLIENT_ID={api-client-id}
   ```

4. Update `backend/src/SurveyApi.Web/appsettings.Development.json`:
   ```json
   {
     "AzureAd": {
       "TenantId": "{your-tenant-id}",
       "ClientId": "{api-client-id}",
       "Authority": "https://login.microsoftonline.com/{your-tenant-id}/v2.0",
       "Audience": "api://{api-client-id}"
     }
   }
   ```

Alternatively, run the helper script to create the app registrations:
```bash
# Requires Azure CLI and admin consent permissions
pwsh azure/register-apps.ps1 -TenantId {your-tenant-id}
```

## Running Tests

### Backend tests

```bash
# Unit tests (validators, services)
dotnet test backend/tests/SurveyApi.UnitTests

# Integration tests (requires SQL Server or uses in-memory fallback)
dotnet test backend/tests/SurveyApi.IntegrationTests
```

### Frontend tests

```bash
cd frontend

# Unit tests (Vitest + Testing Library)
npm run test

# E2E tests (Playwright)
npx playwright install --with-deps chromium
npx playwright test
```

## Environment Variables

### Backend (`appsettings.json` or environment variables)

| Variable | Description | Default |
|---|---|---|
| `AzureAd:Authority` | Azure AD authority URL | `https://login.microsoftonline.com/{tenant-id}/v2.0` |
| `AzureAd:Audience` | API audience (Application ID URI) | `api://{api-client-id}` |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | (in-memory when empty) |
| `UseInMemory` | Use in-memory database instead of SQL | `true` (dev) |
| `Cors:AllowedOrigins` | Allowed CORS origins | `["http://localhost:5173"]` |

### Frontend (`.env` file)

| Variable | Description |
|---|---|
| `VITE_AZURE_CLIENT_ID` | SPA app registration client ID |
| `VITE_AZURE_TENANT_ID` | Azure AD tenant ID |
| `VITE_AZURE_API_CLIENT_ID` | API app registration client ID |

## Production Deployment

### 1. Set up GitHub Secrets

Add the following secrets to your GitHub repository (Settings > Secrets and variables > Actions):

| Secret | Description |
|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON for Azure login |
| `AZURE_RESOURCE_GROUP` | Target resource group name |
| `AZURE_WEBAPP_NAME` | App Service name for the backend |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | App Service publish profile XML |
| `AZURE_STORAGE_ACCOUNT` | Storage account name for static frontend |
| `AZURE_STORAGE_KEY` | Storage account access key |
| `AZURE_SQL_SERVER_NAME` | SQL Server name |
| `AZURE_SQL_ADMIN_USER` | SQL admin username |
| `AZURE_SQL_ADMIN_PASSWORD` | SQL admin password |
| `AZURE_SQL_DB_NAME` | SQL database name |
| `AZURE_APPINSIGHTS_NAME` | Application Insights resource name |
| `AZURE_KEYVAULT_NAME` | Key Vault name |

### 2. Deploy infrastructure

```bash
az group create -n {resource-group} -l {location}

az deployment group create \
  -g {resource-group} \
  --template-file infra/main.bicep \
  --parameters \
    webAppName={webapp-name} \
    storageAccountName={storage-name} \
    sqlServerName={sql-server-name} \
    sqlAdministratorLogin={admin-user} \
    sqlAdministratorPassword={admin-password} \
    appInsightsName={appinsights-name} \
    keyVaultName={keyvault-name}
```

### 3. Store secrets in Key Vault

```bash
az keyvault secret set --vault-name {keyvault-name} \
  -n AzureAd--Authority \
  --value "https://login.microsoftonline.com/{tenant-id}/v2.0"

az keyvault secret set --vault-name {keyvault-name} \
  -n AzureAd--Audience \
  --value "api://{api-client-id}"
```

### 4. Deploy the application

Push to `main` to trigger the CD pipeline, or trigger manually:

```bash
gh workflow run CD
```

The CD pipeline:
1. Deploys infrastructure via Bicep
2. Builds and deploys the backend to an App Service staging slot
3. Runs smoke tests against staging
4. Swaps staging → production (zero-downtime)
5. Builds and deploys the frontend to Azure Storage static website

## API Endpoints

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/api/v1/health` | No | Liveness probe |
| `GET` | `/api/v1/health/ready` | No | Readiness probe (DB check) |
| `GET` | `/api/v1/surveys` | Yes | List surveys (paginated, filterable) |
| `GET` | `/api/v1/surveys/{id}` | Yes | Get survey with questions |
| `POST` | `/api/v1/surveys` | Yes | Create survey |
| `PUT` | `/api/v1/surveys/{id}` | Yes | Update survey |
| `DELETE` | `/api/v1/surveys/{id}` | Yes | Delete survey |
| `POST` | `/api/v1/surveys/{id}/publish` | Yes | Publish survey (generates public link) |
| `POST` | `/api/v1/surveys/{id}/close` | Yes | Close survey |
| `GET` | `/api/v1/s/{publicLinkId}` | No | Get published survey (for respondents) |
| `POST` | `/api/v1/s/{publicLinkId}/responses` | No | Submit response |
| `GET` | `/api/v1/surveys/{id}/responses` | Yes | List responses (paginated) |
| `GET` | `/api/v1/surveys/{id}/responses/{rid}` | Yes | Get individual response |
| `GET` | `/api/v1/surveys/{id}/analytics/summary` | Yes | Analytics summary per question |

## Question Types

| Type | Description |
|---|---|
| `SingleChoice` | Pick one option from a list |
| `MultipleChoice` | Pick multiple options |
| `Rating` | Star or numeric rating (1-5) |
| `Nps` | Net Promoter Score (0-10) |
| `TextShort` | Single-line text input |
| `TextLong` | Multi-line text area |
| `Date` | Date picker |
| `Dropdown` | Dropdown select |
| `Ranking` | Rank options in order |
| `FileUpload` | File or image upload |

## Security

- **Authentication**: Azure AD OAuth 2.0 with JWT Bearer tokens
- **Authorization**: Role-based access control (TenantAdmin, SurveyCreator, SurveyViewer, Respondent)
- **Secrets**: All secrets stored in Azure Key Vault, referenced via managed identity
- **Data isolation**: GUID primary keys, tenant-scoped queries (Phase 2)
- **Rate limiting**: Anonymous response submissions rate-limited per IP
- **Input validation**: FluentValidation on all request DTOs
- **Headers**: CSP, X-Frame-Options, X-Content-Type-Options, Referrer-Policy
- **HTTPS**: Enforced everywhere

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8, ASP.NET Core Minimal API, EF Core 8, FluentValidation |
| Frontend | React 18, TypeScript, Vite, TanStack Query, Recharts, MSAL |
| Authentication | Azure AD (Entra ID), JWT Bearer, Microsoft.Identity.Web |
| Database | Azure SQL Database (production), EF Core InMemory (development) |
| Infrastructure | Azure Bicep (App Service, SQL, Storage, Key Vault, App Insights) |
| CI/CD | GitHub Actions (build, test, security scan, deploy with slot swap) |
| Monitoring | Serilog, Application Insights, health checks |

## License

MIT — see [LICENSE](LICENSE)
