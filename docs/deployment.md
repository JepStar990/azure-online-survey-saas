# Deployment Guide

Step-by-step instructions for deploying SurveySaaS to Azure.

## Architecture

```
                          Azure Front Door (optional)
                                   │
                    ┌──────────────┴──────────────┐
                    │                             │
            App Service (Backend)         Storage Account (Frontend)
            ┌───────────────────┐         ┌──────────────────────┐
            │ Staging Slot      │         │ $web container        │
            │ Production Slot   │         │ (static website)      │
            └──────┬────────────┘         └──────────────────────┘
                   │
            ┌──────┴──────┐
            │             │
      Key Vault      SQL Database
      (secrets)      (SurveyDb)
            │
    Application Insights
      (monitoring)
```

## Prerequisites

1. **Azure subscription** with Contributor access
2. **Azure CLI** installed and logged in:
   ```bash
   az login
   ```
3. **GitHub repository** with the code pushed
4. **Azure AD app registrations** set up (see [azure-ad-config.md](../azure-ad-config.md))

## 1. Create Service Principal for GitHub Actions

```bash
# Create a service principal with Contributor role
az ad sp create-for-rbac \
  --name "survey-saas-github-actions" \
  --role Contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth
```

Copy the JSON output — this is your `AZURE_CREDENTIALS` secret.

## 2. Set Up Azure Resources

### Create Resource Group

```bash
az group create \
  -n survey-saas-rg \
  -l eastus2
```

### Deploy Infrastructure with Bicep

```bash
az deployment group create \
  -g survey-saas-rg \
  --template-file infra/main.bicep \
  --parameters \
    webAppName=survey-saas-api \
    storageAccountName=surveysaasstatic \
    sqlServerName=survey-saas-sql \
    sqlAdministratorLogin=sqladmin \
    sqlAdministratorPassword="$(openssl rand -base64 32)" \
    sqlDatabaseName=SurveyDb \
    appInsightsName=survey-saas-ai \
    keyVaultName=survey-saas-kv
```

> **Important**: Save the SQL admin password — you'll need it as a GitHub secret.

Get the outputs:

```bash
az deployment group show \
  -g survey-saas-rg \
  -n main \
  --query properties.outputs
```

## 3. Configure Key Vault Secrets

```bash
# Get the Key Vault name from Bicep output
KV_NAME="survey-saas-kv"

# Store Azure AD settings
az keyvault secret set --vault-name $KV_NAME \
  -n AzureAd--Authority \
  --value "https://login.microsoftonline.com/{tenant-id}/v2.0"

az keyvault secret set --vault-name $KV_NAME \
  -n AzureAd--Audience \
  --value "api://{api-client-id}"
```

## 4. Configure GitHub Secrets

Go to **GitHub repo** → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**:

| Secret Name | Value |
|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON from step 1 |
| `AZURE_RESOURCE_GROUP` | `survey-saas-rg` |
| `AZURE_WEBAPP_NAME` | `survey-saas-api` |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Download from Azure Portal: App Service → Get publish profile |
| `AZURE_STORAGE_ACCOUNT` | `surveysaasstatic` |
| `AZURE_STORAGE_KEY` | Get from: `az storage account keys list -g survey-saas-rg -n surveysaasstatic --query [0].value` |
| `AZURE_SQL_SERVER_NAME` | `survey-saas-sql` |
| `AZURE_SQL_ADMIN_USER` | `sqladmin` |
| `AZURE_SQL_ADMIN_PASSWORD` | Password from step 2 |
| `AZURE_SQL_DB_NAME` | `SurveyDb` |
| `AZURE_APPINSIGHTS_NAME` | `survey-saas-ai` |
| `AZURE_KEYVAULT_NAME` | `survey-saas-kv` |

## 5. Deploy the Application

### Option A: Push to main (triggers CD automatically)

```bash
git push origin main
```

### Option B: Manual trigger

```bash
gh workflow run CD
```

### Option C: Manual deployment without CI/CD

```bash
# Backend
dotnet publish backend/src/SurveyApi.Web/SurveyApi.Web.csproj -c Release -o publish
cd publish && zip -r ../deploy.zip . && cd ..
az webapp deploy --resource-group survey-saas-rg --name survey-saas-api --src-path deploy.zip

# Frontend
cd frontend && npm ci && npm run build
az storage blob upload-batch -s dist -d '$web' --account-name surveysaasstatic --auth-mode key
```

## 6. Post-Deployment Verification

### Health Check

```bash
curl https://survey-saas-api.azurewebsites.net/api/v1/health
# Expected: {"status":"Healthy","timestamp":"..."}
```

### Frontend

```bash
# Get the static website URL
az storage account show -g survey-saas-rg -n surveysaasstatic \
  --query primaryEndpoints.web -o tsv

# Open in browser and verify:
# 1. Login page loads
# 2. Sign in with Azure AD works
# 3. Dashboard loads after authentication
```

### Verify Key Vault References

```bash
# Check App Service references Key Vault correctly
az webapp config appsettings list \
  -g survey-saas-rg \
  -n survey-saas-api \
  --query "[?name=='ConnectionStrings:DefaultConnection']"
```

## 7. Configure Custom Domain (Optional)

### Backend (App Service)

```bash
# Add custom domain
az webapp config hostname add \
  -g survey-saas-rg \
  --webapp-name survey-saas-api \
  --hostname api.surveysaas.com

# Upload SSL certificate
az webapp config ssl upload \
  -g survey-saas-rg \
  --certificate-file cert.pfx \
  --certificate-password {password} \
  --name survey-saas-api
```

### Frontend (Azure CDN or Front Door)

```bash
# Create CDN endpoint
az cdn endpoint create \
  -g survey-saas-rg \
  --profile-name survey-saas-cdn \
  -n survey-saas-frontend \
  --origin surveysaasstatic.z13.web.core.windows.net \
  --origin-host-header surveysaasstatic.z13.web.core.windows.net

# Map custom domain
az cdn custom-domain create \
  -g survey-saas-rg \
  --profile-name survey-saas-cdn \
  --endpoint-name survey-saas-frontend \
  -n app-surveysaas-com \
  --hostname app.surveysaas.com
```

## 8. Monitoring Setup

### Application Insights Alerts

```bash
# Alert on high failure rate
az monitor metrics alert create \
  -g survey-saas-rg \
  -n "survey-api-failure-rate" \
  --scopes $(az monitor app-insights component show -g survey-saas-rg -n survey-saas-ai --query id -o tsv) \
  --condition "count requests/failed > 10" \
  --window-size 5m \
  --evaluation-frequency 1m

# Alert on high response time
az monitor metrics alert create \
  -g survey-saas-rg \
  -n "survey-api-response-time" \
  --scopes $(az webapp show -g survey-saas-rg -n survey-saas-api --query id -o tsv) \
  --condition "average HttpResponseTime > 3s" \
  --window-size 5m \
  --evaluation-frequency 1m
```

### Log Analytics Queries

```kusto
// Failed requests in the last hour
requests
| where timestamp > ago(1h)
| where success == false
| project timestamp, name, resultCode, duration

// Slow endpoints (top 10)
requests
| where timestamp > ago(24h)
| summarize avg(duration) by name
| top 10 by avg_duration desc

// Survey submission rate
customEvents
| where timestamp > ago(1h)
| where name == "SurveyResponseSubmitted"
| summarize count() by bin(timestamp, 5m)
| render timechart
```

## Rollback Procedure

If a deployment causes issues:

```bash
# Rollback App Service to previous deployment
az webapp deployment source show \
  -g survey-saas-rg \
  -n survey-saas-api

# Swap back from staging to production (if staging was the previous slot)
az webapp deployment slot swap \
  -g survey-saas-rg \
  -n survey-saas-api \
  --slot staging --target-slot production \
  --action swap

# If the issue is in frontend, redeploy from a previous build artifact
# (download from GitHub Actions artifacts and re-upload to storage)
```

## Environment Matrix

| Environment | Resource Group | App Service | SQL Tier | Purpose |
|---|---|---|---|---|
| **Dev** | `survey-saas-dev-rg` | S1, 1 instance | Basic | Local + CI testing |
| **Staging** | `survey-saas-rg` | Staging slot | Standard S2 | Pre-prod validation |
| **Production** | `survey-saas-rg` | Production slot | Standard S2 | Live traffic |

Dev uses in-memory database by default (`UseInMemory: true`). Staging and production use Azure SQL with Key Vault references.

## Cost Estimates (Approximate)

| Resource | Tier | Monthly Cost (USD) |
|---|---|---|
| App Service Plan | S1 (1 instance) | ~$70 |
| SQL Database | Basic (5 DTU) | ~$5 |
| Storage Account | LRS, < 1GB | ~$1 |
| Key Vault | Standard | ~$1 |
| Application Insights | Pay-as-you-go | ~$5 |
| Azure Front Door | Standard (optional) | ~$35 |
| **Total** | | **~$82-117/month** |
