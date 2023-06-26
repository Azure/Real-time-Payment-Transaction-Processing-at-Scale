#!/usr/bin/pwsh
 
 Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$functionAppPath
 )

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location $(./Join-Path-Recursively.ps1 -pathParts $functionAppPath.Split(","))


Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host " Building the function app" -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow
Remove-Item -Path ./bin/Release -Recurse -Force
dotnet build /p:DeployOnBuild=true /p:DeployTarget=Package -c Release

Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host " Archiving the function app" -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow
Compress-Archive -Path ./bin/Release/net7.0/publish/* -DestinationPath ./bin/Release/dist.zip

$functionAppNames=$(az functionapp list -g $resourceGroup -o json | ConvertFrom-Json).name

foreach ($functionAppName in $functionAppNames) {

    Write-Host "===========================================================" -ForegroundColor Yellow
    Write-Host " Deploying to function app $functionAppName" -ForegroundColor Yellow
    Write-Host "===========================================================" -ForegroundColor Yellow
    $deployment=$(az functionapp deployment source config-zip -g $resourceGroup -n $functionAppName --src ./bin/Release/dist.zip -o json | ConvertFrom-Json)
    if ($deployment.provisioningState -eq "Succeeded") {
        Write-Host "Function app deployment succeeded!" -ForegroundColor Green
    } else {
        Write-Host "Function app deployment failed!" -ForegroundColor Red
    }

}

Pop-Location
Pop-Location