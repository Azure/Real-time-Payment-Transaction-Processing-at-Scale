#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$acrName,
    [parameter(Mandatory=$false)][string]$dockerTag="latest"
)

Push-Location $($MyInvocation.InvocationName | Split-Path)
$sourceFolder=$(./Join-Path-Recursively.ps1 -pathParts ..,powershell)

& ./BuildPush.ps1 -resourceGroup $resourceGroup -acrName $acrName -dockerTag $dockerTag -dockerBuild 1 -dockerPush 0

Pop-Location
