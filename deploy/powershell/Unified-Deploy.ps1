#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$locations="SouthCentralUS, NorthCentralUS, EastUS",
    [parameter(Mandatory=$true)][string]$subscription,
    [parameter(Mandatory=$false)][string]$template="main.bicep",
    [parameter(Mandatory=$false)][string]$openAiName=$null,
    [parameter(Mandatory=$false)][string]$openAiRg=$null,
    [parameter(Mandatory=$false)][string]$openAiDeployment=$null,
    [parameter(Mandatory=$false)][string]$suffix=$null,
    [parameter(Mandatory=$false)][bool]$stepDeployBicep=$true,
    [parameter(Mandatory=$false)][bool]$stepPublishFunctionApp=$true,
    [parameter(Mandatory=$false)][bool]$stepDeployOpenAi=$true,
    [parameter(Mandatory=$false)][bool]$stepPublishSite=$true,
    [parameter(Mandatory=$false)][bool]$stepLoginAzure=$true
)

az extension add --name  application-insights
az extension update --name  application-insights

az extension add --name storage-preview
az extension update --name storage-preview

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

if ($stepDeployOpenAi) {
    if (-not $openAiName) {
        $openAiName="openai-$suffix"
    }

    if (-not $openAiRg) {
        $openAiRg=$resourceGroup
    }

    & ./Deploy-OpenAi.ps1 -name $openAiName -resourceGroup $openAiRg -location $locations.Split(',')[0] -suffix $suffix -deployment $openAiDeployment
}

if ($stepDeployBicep) {
    & ./Deploy-Bicep.ps1 -resourceGroup $resourceGroup -locations $locations -template $template -suffix $suffix -openAiName $openAiName -openAiRg $openAiRg -openAiDeployment $openAiDeployment
}

& ./Generate-Config.ps1 -resourceGroup $resourceGroup -openAiName $openAiName -openAiRg $openAiRg -openAiDeployment $openAiDeployment

if ($stepPublishFunctionApp) {
    & ./Publish-FunctionApp.ps1 -resourceGroup $resourceGroup -projectName "CorePayments.FunctionApp"
}

if ($stepPublishSite) {
    & ./Publish-Site.ps1 -resourceGroup $resourceGroup -storageAccount "webpaysa$suffix"
}

Pop-Location