# Project Progress — June 6, 2026

## What was accomplished

### Backend: Clean Architecture (4 projects + 2 test projects)
- **Domain layer**: Entities (Survey, Question, QuestionOption, Response, ResponseAnswer), Enums (QuestionType, SurveyStatus, UserRole), ValueObjects (SurveySettings, QuestionSettings)
- **Application layer**: DTOs, Services (SurveyService, ResponseService, AnalyticsService), FluentValidation validators, Repository interfaces
- **Infrastructure layer**: EF Core AppDbContext with JSON columns, Repository implementations, CurrentUserService, SeedData (4 demo surveys)
- **Web API**: 14 endpoints across Health, Surveys, Questions, Responses, Analytics groups, middleware (ExceptionHandling, RequestLogging), Program.cs composition root with JWT auth, CORS, Swagger, Serilog
- **Tests**: Unit tests (validators) + Integration tests (WebApplicationFactory with in-memory DB)

### Frontend: React + TypeScript SPA
- **Auth**: MSAL React with Azure AD (AuthProvider with initialize, useAuth hook, RequireAuth guard)
- **API layer**: Axios client with Bearer token interceptor (retry logic), TanStack Query hooks
- **Pages**: Login, Dashboard, SurveyList, SurveyBuilder, SurveyTake, SurveyThankYou, Results, NotFound
- **Components**: QuestionEditor (10 question types), layout (AppShell, Header, Sidebar), shared (LoadingSpinner, ErrorMessage, EmptyState, ConfirmDialog)
- **Charts**: Recharts (BarChart, PieChart) on ResultsPage

### Azure Infrastructure (Deployed)
| Resource | Name | Region |
|---|---|---|
| Resource Group | survey-saas-rg | southafricanorth |
| App Service | survey-saas-api | S1 tier |
| SQL Server | survey-saas-sql | Basic DB |
| SQL Database | SurveyDb | 5 DTU |
| Storage Account | surveysaassta | Static website enabled |
| Key Vault | survey-saas-kv1 | Standard |
| App Insights | survey-saas-ai | Monitoring |

### Azure AD App Registrations
| App | Client ID | Type |
|---|---|---|
| SurveyApi | c618d4b8-dca7-4bc5-a12a-160a4990bb42 | Multi-tenant API |
| SurveyWeb | 3f50ac44-4c28-4029-80a6-dc5c6ecc5857 | Multi-tenant SPA |
| Tenant | f36fb8ee-44de-4d52-b910-fa4826ae3110 | Home tenant |

### Deployed URLs
- Frontend: https://surveysaassta.z1.web.core.windows.net/
- Backend API: https://survey-saas-api.azurewebsites.net/api/v1/health

## Issues encountered and fixed

1. **MSAL v3 initialization** — `loginRedirect` failed silently without `msalInstance.initialize()`. Fixed by adding init + `handleRedirectPromise()` in AuthProvider before rendering.

2. **API base URL in production** — Frontend used relative `/api/v1` which resolved to storage account (405 errors). Fixed by using `VITE_API_URL` env var pointing to App Service URL.

3. **Content-Security-Policy blocking API calls** — `connect-src` only allowed `login.microsoftonline.com`. Fixed by adding `https://*.azurewebsites.net`.

4. **App Service CORS conflict** — App Service CORS was blocking requests before .NET CORS middleware could handle them. Fixed by clearing App Service CORS origins (removing the redundant layer).

5. **JWT audience validation** — Azure AD v2 tokens sometimes use raw client ID as `aud` claim instead of App ID URI. Fixed by accepting both `api://{client-id}` and `{client-id}` in `ValidAudiences`.

6. **Role-based authorization blocking users** — App roles (TenantAdmin, SurveyCreator) were created and assigned but the `roles` claim wasn't flowing into tokens. Fixed by using `RequireAuthenticatedUser()` instead of `RequireRole()` as a temporary measure.

7. **SQL Server firewall** — No firewall rules existed, blocking App Service from reaching the database. Added `AllowAzureServices` rule (0.0.0.0).

8. **Key Vault reference not resolving** — The `@Microsoft.KeyVault(...)` reference for the SQL connection string wasn't being resolved by App Service. Fixed by setting the connection string directly via `az webapp config appsettings set`.

## Current state (blocked)

The backend returns 500 errors when querying the database despite:
- SQL firewall rule being in place
- Connection string being set directly (not via Key Vault reference)
- App Service restart completing successfully

The `/api/v1/health` endpoint returns 200 (no DB dependency), but `/api/v1/health/seed` returns 500 when trying to query `db.Surveys.CountAsync()`.

### Next steps for tomorrow

1. **Debug the 500 error** — Check App Service logs via Azure Portal or Kudu to see the actual exception
2. **Verify SQL connectivity** — Run `az webapp log tail` to see startup errors, check if connection string has special characters causing issues
3. **Test with in-memory DB** — Temporarily set `UseInMemory=true` to verify the API works without SQL dependency, then enable SQL once connectivity is confirmed
4. **Full end-to-end test** — Once the API works, test sign-in → dashboard → create survey → save → publish → take survey → view results
5. **Phase 2 planning** — Multi-tenancy, proper RBAC with app roles, rate limiting

### Quick redeploy commands (values from Azure AD / Key Vault)

```bash
# Build backend
cd /root/azure-online-survey-saas
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 DOTNET_GCHeapHardLimit=0x20000000 dotnet publish backend/src/SurveyApi.Web/SurveyApi.Web.csproj -c Release -o /tmp/publish

# Package and deploy
cd /tmp/publish && zip -qr /tmp/backend-deploy.zip .
az webapp deploy -g survey-saas-rg -n survey-saas-api --src-path /tmp/backend-deploy.zip --type zip

# Build and deploy frontend
cd /root/azure-online-survey-saas/frontend
VITE_AZURE_CLIENT_ID=<spa-client-id> \
VITE_AZURE_TENANT_ID=<tenant-id> \
VITE_AZURE_API_CLIENT_ID=<api-client-id> \
VITE_API_URL=https://survey-saas-api.azurewebsites.net/api/v1 \
npm run build
STORAGE_KEY=$(az storage account keys list -g survey-saas-rg -n surveysaassta --query "[0].value" -o tsv)
az storage blob upload-batch -s dist -d '$web' --account-name surveysaassta --account-key "$STORAGE_KEY" --overwrite
```

### Git commits today (12 commits)
```
f1db56b Add seed surveys and use RequireAuthenticatedUser policies
55989a7 Use RequireAuthenticatedUser for policies while roles claim matures
9e663a8 Remove auto-redirect on 401; accept both audience formats
e66f650 Accept both App ID URI and raw client ID as valid audiences
d9d236e Add token acquisition retry logic and automatic re-login
24f63d3 Fix API base URL for production
9b3ecbd Fix CSP blocking API calls in production
778bdcb Fix survey save: robust MSAL token acquisition
97ff635 Fix MSAL authentication initialization and error handling
bc40cb7 Fix Bicep template for ARM deployment compatibility
dc45d59 Add comprehensive deployment guide and Azure AD config docs
cdeff3c Update README with comprehensive documentation
```
