#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$acrName="bydtochatgptcr",
    [parameter(Mandatory=$false)][string]$acrResourceGroup="ms-byd-to-chatgpt",
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location,
    [parameter(Mandatory=$true)][string]$subscription,
    [parameter(Mandatory=$false)][string]$suffix,
    [parameter(Mandatory=$false)][bool]$stepDeployBicep=$true,
    [parameter(Mandatory=$false)][bool]$stepBuildPush=$false,
    [parameter(Mandatory=$false)][bool]$stepDeployCertManager=$true,
    [parameter(Mandatory=$false)][bool]$stepDeployTls=$true,
    [parameter(Mandatory=$false)][bool]$stepDeployImages=$true,
    [parameter(Mandatory=$false)][bool]$stepSetupSynapse=$true,
    [parameter(Mandatory=$false)][bool]$stepPublishSite=$true,
    [parameter(Mandatory=$false)][bool]$stepLoginAzure=$false
)

az extension add --name  application-insights
az extension update --name  application-insights

az extension add --name storage-preview
az extension update --name storage-preview

winget install --id=Kubernetes.kubectl  -e --accept-package-agreements --accept-source-agreements --silent
winget install --id=Microsoft.Azure.Kubelogin  -e --accept-package-agreements --accept-source-agreements --silent

$gValuesFile="configFile.yaml"

Push-Location $($MyInvocation.InvocationName | Split-Path)

if (-not $suffix) {
    $crypt = New-Object -TypeName System.Security.Cryptography.SHA256Managed
    $utf8 = New-Object -TypeName System.Text.UTF8Encoding
    $hash = [System.BitConverter]::ToString($crypt.ComputeHash($utf8.GetBytes($resourceGroup)))
    $hash = $hash.replace('-','').toLower()
    $suffix = $hash.Substring(0,5)
}

Write-Host "Resource suffix is $suffix" -ForegroundColor Yellow

if ($stepLoginAzure) {
    az login
}

az account set --subscription $subscription

if ($stepDeployBicep) {
    & ./Deploy-Bicep.ps1 -resourceGroup $resourceGroup -location $location -suffix $suffix
}

# Connecting kubectl to AKS
Write-Host "Retrieving Aks Name" -ForegroundColor Yellow
$aksName = $(az aks list -g $resourceGroup -o json | ConvertFrom-Json).name
Write-Host "The name of your AKS: $aksName" -ForegroundColor Yellow

az aks enable-addons -g $resourceGroup -n $aksName --addons http_application_routing

# Write-Host "Retrieving credentials" -ForegroundColor Yellow
az aks get-credentials -n $aksName -g $resourceGroup --overwrite-existing --admin

# Generate Config
New-Item -ItemType Directory -Force -Path $(./Join-Path-Recursively.ps1 -pathParts ..,..,__values)
$gValuesLocation=$(./Join-Path-Recursively.ps1 -pathParts ..,..,__values,$gValuesFile)
& ./Generate-Config.ps1 -resourceGroup $resourceGroup -suffix $suffix -outputFile $gValuesLocation

# Create Secrets
if ([string]::IsNullOrEmpty($acrName))
{
    $acrName = $(az acr list --resource-group $resourceGroup -o json | ConvertFrom-Json).name
    $acrResourceGroup = $resourceGroup
}

Write-Host "The Name of your ACR: $acrName" -ForegroundColor Yellow

if ($stepDeployCertManager) {
    # Deploy Cert Manager
    & ./DeployCertManager.ps1
}

if ($stepDeployTls) {
    # Deploy TLS
    & ./DeployTlsSupport.ps1 -sslSupport prod -resourceGroup $resourceGroup -aksName $aksName
}

if ($stepBuildPush) {
    # Build an Push
    & ./BuildPush.ps1 -resourceGroup $acrResourceGroup -acrName $acrName
}

if ($stepDeployImages) {
    # Deploy images in AKS
    $gValuesLocation=$(./Join-Path-Recursively.ps1 -pathParts ..,..,__values,$gValuesFile)
    $chartsToDeploy = "*"
    & ./Deploy-Images-Aks.ps1 -aksName $aksName -resourceGroup $resourceGroup -charts $chartsToDeploy -acrName $acrName -valuesFile $gValuesLocation
}

if ($stepSetupSynapse) {
    & ./Setup-Synapse.ps1 -resourceGroup $resourceGroup
}

if ($stepPublishSite) {
    & ./Publish-Site.ps1 -resourceGroup $resourceGroup -storageAccount "webcoreclaims$suffix"
}

Pop-Location