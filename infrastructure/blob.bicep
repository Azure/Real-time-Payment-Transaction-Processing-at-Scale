@description('Storage account name, max length 44 characters, lowercase')
param storageAccountName string

@description('Resource cocation')
param location string = resourceGroup().location

@description('API managed identity service principal Id')
param apiPrincipalId string

@description('Worker managed identity service principal Id')
param workerPrincipalId string

resource blob 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    isHnsEnabled: true
  }
}

// Grant Permissions to Identity for Storage
@description('This is the built-in "Storage Blob Data Contributor" role. See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-contributor')
resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource apiRoleAssignmentStorage 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: blob
  name: guid(apiPrincipalId, blob.id, 'FunctionOwner')
  properties: {
    roleDefinitionId: contributorRoleDefinition.id
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource workerRoleAssignmentStorage 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: blob
  name: guid(workerPrincipalId, blob.id, 'FunctionOwner')
  properties: {
    roleDefinitionId: contributorRoleDefinition.id
    principalId: workerPrincipalId
    principalType: 'ServicePrincipal'
  }
}
