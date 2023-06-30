#!/usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$location,
    [parameter(Mandatory=$true)][string]$subscription,
    [parameter(Mandatory=$false)][string]$template="starter.bicep",
    [parameter(Mandatory=$false)][string]$suffix,
    [parameter(Mandatory=$false)][bool]$stepDeployBicep=$true,
    [parameter(Mandatory=$false)][bool]$stepPublishFunctionApp=$false,
    [parameter(Mandatory=$false)][bool]$stepSetupSynapse=$false,
    [parameter(Mandatory=$false)][bool]$stepPublishSite=$false,
    [parameter(Mandatory=$false)][bool]$stepLoginAzure=$true
)

Push-Location $($MyInvocation.InvocationName | Split-Path)

& ./Unified-Deploy.ps1 -acrName $acrName `
                       -resourceGroup $resourceGroup `
                       -location $location `
                       -subscription $subscription `
                       -armTemplate $armTemplate `
                       -openAiName $null `
                       -openAiRg $null `
                       -openAiDeployment $null `
                       -suffix $suffix `
                       -stepDeployBicep $stepDeployBicep `
                       -stepDeployOpenAi $false `
                       -stepPublishFunctionApp $stepPublishFunctionApp `
                       -stepPublishSite $stepPublishSite `
                       -stepLoginAzure $stepLoginAzure

Pop-Location