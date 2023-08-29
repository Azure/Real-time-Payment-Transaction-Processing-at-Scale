#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location,
    [parameter(Mandatory=$false)][string]$template="vmdeploy.json",
    [parameter(Mandatory=$false)][string]$resourcePrefix
)

$sourceFolder=$(Join-Path -Path ../.. -ChildPath infrastructure)

Push-Location $($MyInvocation.InvocationName | Split-Path)

$script=$template

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host "Deploying ARM script $script" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

$rg = $(az group list --query "[?name=='$resourceGroup']" -o json | ConvertFrom-Json)
# Deployment without AKS can be done in a existing or non-existing resource group.
if ($rg.length -eq 0) {
    Write-Host "Creating resource group $resourceGroup in $location" -ForegroundColor Yellow
    az group create -n $resourceGroup -l $location
}

# TODO: Uncomment this when AZ CLI consistently returns a valid semantic version for AKS
# Write-Host "Getting last AKS version in location $location" -ForegroundColor Yellow
# $aksVersions=$(az aks get-versions -l $location --query  values[].version -o json | ConvertFrom-Json)
# $aksLastVersion=$aksVersions[$aksVersions.Length-1]
# Write-Host "AKS last version is $aksLastVersion" -ForegroundColor Yellow
$aksLastVersion="1.26.3"

$deploymentName = "cosmosdb-payments-vmdeploy"

Write-Host "Begining the ARM deployment..." -ForegroundColor Yellow
Push-Location $sourceFolder
az deployment group create -g $resourceGroup -n $deploymentName --template-file $script --parameters location=$($location.ToLower())

$outputVal = (az deployment group show -g $resourceGroup -n $deploymentName --query properties.outputs.resourcePrefix.value) | ConvertFrom-Json
Set-Variable -Name resourcePrefix -Value $outputVal.ToString() -Scope 1
Write-Host "The resource prefix used in deployment is $outputVal"

$outputVal = (az deployment group show -g $resourceGroup -n $deploymentName --query properties.outputs.deployedVM.value) | ConvertFrom-Json
Set-Variable -Name cosmosDbAccountName -Value $outputVal.ToString() -Scope 1
Write-Host "The deployed VM name used in deployment is $outputVal"

Pop-Location 
Pop-Location 
