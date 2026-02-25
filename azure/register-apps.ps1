<#
PowerShell script to create Azure AD app registrations for the SurveyApi and SurveyWeb (SPA).
Requires Azure CLI: `az login` and permission to create app registrations.

Usage:
  .\register-apps.ps1 -TenantId <tenantId> -RedirectUri "http://localhost:5173"
#>

param(
  [string]$TenantId,
  [string]$RedirectUri = "http://localhost:5173"
)

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
  Write-Error "Azure CLI (az) is required. Install from https://aka.ms/azcli"
  exit 1
}

Write-Host "Creating API app (SurveyApi)..."
$apiApp = az ad app create --display-name "SurveyApi" --available-to-other-tenants false --query '{appId:appId, id:id}' | ConvertFrom-Json
Write-Host "API AppId: $($apiApp.appId)"

Write-Host "Exposing scope (you may need Graph permissions)."
# The user may need to add scope manually in the portal if Graph permissions are insufficient.

Write-Host "Creating SPA app (SurveyWeb)..."
$spaApp = az ad app create --display-name "SurveyWeb" --web-redirect-uris $RedirectUri --public-client-redirect-uris $RedirectUri --required-resource-accesses '[]' --query '{appId:appId, id:id}' | ConvertFrom-Json
Write-Host "SPA AppId: $($spaApp.appId)"

Write-Host "NOTE: You must grant permissions and expose scopes using the Azure Portal if the CLI cannot set them due to permissions."
