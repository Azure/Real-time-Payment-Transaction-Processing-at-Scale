#!/usr/bin/pwsh
 
 Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$storageAccount
 )

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location $(./Join-Path-Recursively.ps1 -pathParts "..,..,ui".Split(","))

Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host " Building the website" -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow
if (Test-Path ./out)
{
   Remove-Item -Path ./out -Recurse -Force
}
Start-Sleep -Seconds 10
npm ci
npm run build

Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host " Deploying the website" -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow
az storage azcopy blob upload -c `$web --account-name $storageAccount -s "./out/*" --recursive

Pop-Location
Pop-Location