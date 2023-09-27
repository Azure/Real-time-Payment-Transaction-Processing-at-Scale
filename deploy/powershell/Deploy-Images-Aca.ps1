#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$locations,
    [parameter(Mandatory=$false)][string]$acrName,
    [parameter(Mandatory=$false)][string]$acrResourceGroup=$resourceGroup,
    [parameter(Mandatory=$true)][string]$suffix,
    [parameter(Mandatory=$false)][string]$tag="latest"
)

function validate {
    $valid = $true

    if ([string]::IsNullOrEmpty($resourceGroup))  {
        Write-Host "No resource group. Use -resourceGroup to specify resource group." -ForegroundColor Red
        $valid=$false
    }

    if ([string]::IsNullOrEmpty($acrLoginServer))  {
        Write-Host "ACR login server can't be found. Are you using right ACR ($acrName) and RG ($resourceGroup)?" -ForegroundColor Red
        $valid=$false
    }

    if ($valid -eq $false) {
        exit 1
    }
}

$locArray = $locations.Split(",")

$i = 0
foreach($location in $locArray)
{
    Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
    Write-Host " Deploying images on Aca"  -ForegroundColor Yellow
    Write-Host " "  -ForegroundColor Yellow
    Write-Host " Additional parameters are:"  -ForegroundColor Yellow
    Write-Host " Images tag: $tag"  -ForegroundColor Yellow
    Write-Host " --------------------------------------------------------" 

    if ($acrName -ne "bydtochatgptcr") {
        $queryString = "[?location=='$($location.toLower())'].{name: name}"
        $acrName = $(az acr list -g $resourceGroup --query $queryString -o json | ConvertFrom-Json).name

        Write-Host "---------------------------------------------------" -ForegroundColor Yellow
        Write-Host "Getting info from ACR $resourceGroup/$acrName" -ForegroundColor Yellow
        Write-Host "---------------------------------------------------" -ForegroundColor Yellow
        az acr update -n $acrName --admin-enabled true
        $acrLoginServer=$(az acr show -g $resourceGroup -n $acrName -o json | ConvertFrom-Json).loginServer
        $acrCredentials=$(az acr credential show -g $resourceGroup -n $acrName -o json | ConvertFrom-Json)
        $acrPwd=$acrCredentials.passwords[0].value
        $acrUser=$acrCredentials.username
    }
    else {
        $acrLoginServer="bydtochatgptcr.azurecr.io"
    }

    validate

    Push-Location $($MyInvocation.InvocationName | Split-Path)

    Write-Host "Deploying images..." -ForegroundColor Yellow

    Write-Host "API deployment - api" -ForegroundColor Yellow
    $command = "az containerapp update --name aca-api-payments-${suffix}${i} --resource-group $resourceGroup --image $acrLoginServer/payments-api:$tag"
    Invoke-Expression "$command"

    Write-Host "Webapp deployment - worker" -ForegroundColor Yellow
    $command = "az containerapp update --name aca-worker-payments-${suffix}${i} --resource-group $resourceGroup --image $acrLoginServer/payments-worker:$tag"
    Invoke-Expression "$command"

    Pop-Location

    Write-Host "Microservices deployed to ACA" -ForegroundColor Yellow

    $i++
}