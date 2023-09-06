#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$locations="SouthCentralUS,NorthCentralUS,EastUS",
    [parameter(Mandatory=$true)][string]$subscription,
    [parameter(Mandatory=$false)][string]$template="main.bicep",
    [parameter(Mandatory=$false)][string]$suffix=$null,
    [parameter(Mandatory=$false)][bool]$stepDeployBicep=$true,
    [parameter(Mandatory=$false)][bool]$stepDeployFD=$true,
    [parameter(Mandatory=$false)][bool]$stepDeployImages=$true,
    [parameter(Mandatory=$false)][bool]$stepPublishSite=$true
)

az extension add --name  application-insights
az extension update --name  application-insights

az extension add --name storage-preview
az extension update --name storage-preview

$gValuesFile="configFile"

Push-Location $($MyInvocation.InvocationName | Split-Path)

if (-not $suffix) {
    $crypt = New-Object -TypeName System.Security.Cryptography.SHA256Managed
    $utf8 = New-Object -TypeName System.Text.UTF8Encoding
    $hash = [System.BitConverter]::ToString($crypt.ComputeHash($utf8.GetBytes($resourceGroup)))
    $hash = $hash.replace('-','').toLower()
    $suffix = $hash.Substring(0,5)
}

Write-Host "Resource suffix is $suffix" -ForegroundColor Yellow

az account set --subscription $subscription

$rg = $(az group show -g $resourceGroup -o json | ConvertFrom-Json)
if (-not $rg) {
    $rg=$(az group create -g $resourceGroup -l $locations.Split(',')[0] --subscription $subscription)
}

# Waiting to make sure resource group is available
Start-Sleep -Seconds 10

if ($stepDeployBicep) {
    & ./Deploy-Bicep.ps1 -resourceGroup $resourceGroup -locations $locations -template $template -suffix $suffix
}

# Connecting kubectl to AKS
Write-Host "Retrieving Aks Names" -ForegroundColor Yellow
$aksNames = $(az aks list -g $resourceGroup -o json | ConvertFrom-Json).name
Write-Host "The names of your AKS instances: $aksNames" -ForegroundColor Yellow

# Generate Config
New-Item -ItemType Directory -Force -Path $(./Join-Path-Recursively.ps1 -pathParts ..,..,__values)
$gValuesLocation=$(./Join-Path-Recursively.ps1 -pathParts ..,..,__values,$gValuesFile)
& ./Generate-Config.ps1 -resourceGroup $resourceGroup -locations $locations -suffix $suffix -outputFile $gValuesLocation

if ($stepDeployFD)
{
    & ./Deploy-FDOrigins.ps1 -resourceGroup $resourceGroup -locations $locations
}

if ($stepDeployImages) {
    # Deploy images in AKS
    $gValuesLocation=$(./Join-Path-Recursively.ps1 -pathParts ..,..,__values,$gValuesFile)
    $chartsToDeploy = "*"
    & ./Deploy-Images-Aks.ps1 -resourceGroup $resourceGroup -locations $locations -charts $chartsToDeploy -valuesFile $gValuesLocation
}

if ($stepPublishSite) {
    & ./Publish-Site.ps1 -resourceGroup $resourceGroup -storageAccount "webpaysa$suffix"
}

Pop-Location