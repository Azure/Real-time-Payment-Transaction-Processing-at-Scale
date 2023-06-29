#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$name,
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location,
    [parameter(Mandatory=$true)][string]$suffix, 
    [parameter(Mandatory=$true)][string]$deployment
)

Push-Location $($MyInvocation.InvocationName | Split-Path)

$openAi=$(az cognitiveservices account show -g $resourceGroup -n $name -o json | ConvertFrom-Json)
if (-not $openAi) {
    $openAi=$(az cognitiveservices account create -g $resourceGroup -n $name -l $location --kind OpenAI --sku S0 --yes | ConvertFrom-Json)
}

if ($deployment) {
    $openAiDeployment=$(az cognitiveservices account deployment show -g $resourceGroup -n $openAi.name --deployment-name $deployment)
    if (-not $openAiDeployment) {
        $openAiDeployment=$(az cognitiveservices account deployment create -g $resourceGroup -n $openAi.name --deployment-name $deployment --model-name 'gpt-35-turbo' --model-version '0301' --model-format OpenAI)
    }
} else {
    $deployment='completions'
    $openAiDeployment=$(az cognitiveservices account deployment show -g $resourceGroup -n $openAi.name --deployment-name $deployment)
}

Pop-Location