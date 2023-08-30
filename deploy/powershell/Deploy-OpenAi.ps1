#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$name,
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location,
    [parameter(Mandatory=$false)][string]$deployment=$null
)

Push-Location $($MyInvocation.InvocationName | Split-Path)

if ($name) {
    Write-Host "Searching for OpenAI instance ${name}..." -ForegroundColor Yellow
    $openAi=$(az cognitiveservices account show -g $resourceGroup -n $name -o json | ConvertFrom-Json)
    if (-not $openAi) {
        Write-Host "OpenAI instance ${name} not found - creating..." -ForegroundColor Yellow
        $openAi=$(az cognitiveservices account create -g $resourceGroup -n $name --kind OpenAI --sku S0 --location $location --yes -o json | ConvertFrom-Json)
    } else {
        Write-Host "Found OpenAI instance ${$openAi.name}..." -ForegroundColor Yellow
    }
} else {
    Write-Host "No OpenAI instance specified - searching for OpenAI instances in resource group ${resourceGroup}..." -ForegroundColor Yellow
    $openAi=$(az cognitiveservices account list -g $resourceGroup -o json | ConvertFrom-Json)[0]
}

if ($openAi.name) {
    if ($deployment) {
        Write-Host "Searching for OpenAI deployment ${deployment}..." -ForegroundColor Yellow
        $openAiDeployment=$(az cognitiveservices account deployment show -g $resourceGroup -n $openAi.name --deployment-name $deployment)
        if (-not $openAiDeployment) {
            Write-Host "OpenAI deployment ${deployment} not found - creating..." -ForegroundColor Yellow
            $openAiDeployment=$(az cognitiveservices account deployment create -g $resourceGroup -n $openAi.name --deployment-name $deployment --model-name 'gpt-35-turbo' --model-version '0301' --model-format OpenAI  --scale-settings-scale-type 'Standard')
        } else {
            Write-Host "Found OpenAI deployment ${deployment}..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "No OpenAI deployment specified - searching for default deployment completions..." -ForegroundColor Yellow
        $deployment='completions'
        $openAiDeployment=$(az cognitiveservices account deployment show -g $resourceGroup -n $openAi.name --deployment-name $deployment)
    }
} else {
    Write-Host "Could not find or create an OpenAI service..."
    Exit -1
}

Pop-Location