param location string = resourceGroup().location
param appServicePlanName string = 'survey-asp'
param webAppName string
param storageAccountName string
param sqlServerName string
param sqlAdministratorLogin string
@secure()
param sqlAdministratorPassword string
param sqlDatabaseName string = 'SurveyDb'
param appInsightsName string
param keyVaultName string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  properties: {}
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ConnectionStrings:DefaultConnection'
          value: '@Microsoft.KeyVault(SecretUri=https://${keyVaultName}.vault.azure.net/secrets/sql-connection-string)'
        }
        {
          name: 'AzureAd:Authority'
          value: ''
        }
        {
          name: 'AzureAd:Audience'
          value: ''
        }
      ]
    }
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: true
  }
}

resource staticWebsite 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = if (true) {
  name: '${storage.name}/default/$web'
  properties: {}
}

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

// Key Vault to store secrets and allow the web app to read them via managed identity
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enablePurgeProtection: true
    enableSoftDelete: true
  }
}

// Store SQL connection string as a secret in Key Vault
resource sqlConnSecret 'Microsoft.KeyVault/vaults/secrets@2019-09-01' = {
  parent: keyVault
  name: 'sql-connection-string'
  properties: {
    value: 'Server=${sqlServer.properties.fullyQualifiedDomainName};Database=${sqlDatabaseName};User ID=${sqlAdministratorLogin};Password=${sqlAdministratorPassword};Encrypt=True;TrustServerCertificate=False;'
  }
  dependsOn: [ sqlDatabase ]
}

// Grant web app's system-assigned identity access to read secrets from Key Vault
resource keyVaultAccess 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: webApp.identity.principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
  }
}

output webAppDefaultHostName string = webApp.properties.defaultHostName
output storageEndpoint string = storage.properties.primaryEndpoints.web
output sqlFullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output keyVaultUri string = keyVault.properties.vaultUri
