#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$locations,
    [parameter(Mandatory=$true)][string]$suffix,
    [parameter(Mandatory={-not $deployAks})][string]$openAiName,
    [parameter(Mandatory={-not $deployAks})][string]$openAiCompletionsDeployment,
    [parameter(Mandatory={-not $deployAks})][string]$openAiRg,
    [parameter(Mandatory=$true)][bool]$deployAks
)

Push-Location $($MyInvocation.InvocationName | Split-Path)
$sourceFolder=$(Join-Path -Path ../.. -ChildPath infrastructure)

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host "Deploying Bicep script $script" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------- " -ForegroundColor Yellow

$env:BICEP_RESOURCE_TYPED_PARAMS_AND_OUTPUTS_EXPERIMENTAL="true"
$firstLocation = $locations.Split(',')[0]

$rg = $(az group show -n $resourceGroup -o json | ConvertFrom-Json)
if (-not $rg) {
    Write-Host "Creating resource group $resourceGroup in $firstLocation" -ForegroundColor Yellow
    az group create -n $resourceGroup -l $firstLocation
}

Write-Host "Beginning the Bicep deployment..." -ForegroundColor Yellow
Push-Location $sourceFolder

if ($deployAks) {
    $script="aksmain.bicep"
    $deploymentState = $(az deployment group create -g $resourceGroup --template-file $script --parameters suffix=$suffix --parameters locations=$locations --query "properties.provisioningState" -o tsv)
} else {
    $script="acamain.bicep"
    $deploymentState = $(az deployment group create -g $resourceGroup --template-file $script --parameters suffix=$suffix --parameters locations=$locations --parameters openAiName=$openAiName --parameters openAiDeployment=$openAiCompletionsDeployment --parameters openAiRg=$openAiRg --query "properties.provisioningState" -o tsv)
}

Pop-Location
Pop-Location
