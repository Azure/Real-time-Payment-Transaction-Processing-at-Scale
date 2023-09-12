#!/usr/bin/pwsh
 
 Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$projectName
 )

function log {
    Param(
        $text
    )

    Write-Host $text -ForegroundColor Yellow -BackgroundColor DarkGreen
}

function publish {
    Param( 
        $projectName
    )

    $projectPath="src/${projectName}/${projectName}.csproj"
    $publishDestPath="publish/" + [guid]::NewGuid().ToString()

    log "Publishing project '$($projectName)' in folder '$($publishDestPath)' ..."
    dotnet publish $projectPath -c Release -o $publishDestPath

    # Add sleep to allow the publish to complete before zipping
    Start-Sleep -Seconds 10

    $zipArchiveFullPath="$($publishDestPath).Zip"
    log "Creating zip archive '$($zipArchiveFullPath)'"
    $compress = @{
        Path = $publishDestPath + "/*"
        CompressionLevel = "Fastest"
        DestinationPath = $zipArchiveFullPath
    }
    Compress-Archive @compress

    log "Cleaning up..."
    Remove-Item -Path "$($publishDestPath)" -recurse

    return $zipArchiveFullPath
}

function deploy {
    Param(
        $zipArchiveFullPath,
        $resourceGroup,
        $appName
    )

    log "Deploying '$($appName)' to Resource Group '$($resourceGroup)' from zip '$($zipArchiveFullPath)' ..."
    az functionapp deployment source config-zip -g "$($resourceGroup)" -n "$($appName)" --src "$($zipArchiveFullPath)"
}

function createArtifact {
    Param(
        $appName
    )

    $zipPath = publish $appName
    if ($zipPath -is [array]) {
        $zipPath = $zipPath[$zipPath.Length - 1]
    }

    return $zipPath
}

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location $(./Join-Path-Recursively.ps1 -pathParts "..,..".Split(","))

$functionAppNames=$(az functionapp list -g $resourceGroup -o json | ConvertFrom-Json).name
$zipPath = createArtifact $projectName

foreach ($functionAppName in $functionAppNames) {
    deploy $zipPath $resourceGroup $functionAppName
}

Pop-Location
Pop-Location