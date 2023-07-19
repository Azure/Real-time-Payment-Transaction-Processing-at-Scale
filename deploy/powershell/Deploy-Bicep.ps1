#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$locations,
    [parameter(Mandatory=$false)][string]$template="main.bicep",
    [parameter(Mandatory=$false)][string]$openAiName,
    [parameter(Mandatory=$false)][string]$openAiRg,
    [parameter(Mandatory=$false)][string]$openAiDeployment,
    [parameter(Mandatory=$true)][string]$suffix
)

Push-Location $($MyInvocation.InvocationName | Split-Path)
$sourceFolder=$(Join-Path -Path ../.. -ChildPath infrastructure)

$script=$template

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host "Deploying Bicep script $script" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

$firstLocation = $locations.Split(',')[0]

$rg = $(az group show -n $resourceGroup -o json | ConvertFrom-Json)
if (-not $rg) {
    Write-Host "Creating resource group $resourceGroup in $firstLocation" -ForegroundColor Yellow
    az group create -n $resourceGroup -l $firstLocation
}

Write-Host "Beginning the Bicep deployment..." -ForegroundColor Yellow
Push-Location $sourceFolder
$deploymentState = $(az deployment group create -g $resourceGroup --template-file $script --parameters suffix=$suffix --parameters locations=$locations --parameters openAiName=$openAiName --parameters openAiDeployment=$openAiDeployment --parameters openAiResourceGroup=$openAiRg --query "properties.provisioningState" -o tsv)
Pop-Location
Pop-Location
