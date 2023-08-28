Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$suffix,
    [parameter(Mandatory=$false)][string[]]$outputFile=$null,
    [parameter(Mandatory=$false)][string[]]$gvaluesTemplate="..,..,gvalues.template.yml",
    [parameter(Mandatory=$false)][string[]]$dockerComposeTemplate="..,..,docker-compose.template.yml",
    [parameter(Mandatory=$false)][string]$ingressClass="addon-http-application-routing",
    [parameter(Mandatory=$false)][string]$domain
)

function EnsureAndReturnFirstItem($arr, $restype) {
    if (-not $arr -or $arr.Length -ne 1) {
        Write-Host "Fatal: No $restype found (or found more than one)" -ForegroundColor Red
        exit 1
    }

    return $arr[0]
}

# Check the rg
$rg=$(az group show -n $resourceGroup -o json | ConvertFrom-Json)

if (-not $rg) {
    Write-Host "Fatal: Resource group not found" -ForegroundColor Red
    exit 1
}

### Getting Resources
$tokens=@{}

## Getting storage info
# $storage=$(az storage account list -g $resourceGroup --query "[].{name: name, blob: primaryEndpoints.blob}" -o json | ConvertFrom-Json)
# $storage=EnsureAndReturnFirstItem $storage "Storage Account"
# Write-Host "Storage Account: $($storage.name)" -ForegroundColor Yellow

## Getting CosmosDb info
$docdb=$(az cosmosdb list -g $resourceGroup --query "[?kind=='GlobalDocumentDB'].{name: name, kind:kind, documentEndpoint:documentEndpoint}" -o json | ConvertFrom-Json)
$docdb=EnsureAndReturnFirstItem $docdb "CosmosDB (Document Db)"
$docdbKey=$(az cosmosdb keys list -g $resourceGroup -n $docdb.name -o json --query primaryMasterKey | ConvertFrom-Json)
Write-Host "Document Db Account: $($docdb.name)" -ForegroundColor Yellow

## Getting EventHub info
$eventHubName=$(az eventhubs namespace list -g $resourceGroup -o json | ConvertFrom-Json).name

## Getting App Insights instrumentation key, if required
$appinsightsId=@()
$appInsightsName=$(az resource list -g $resourceGroup --resource-type Microsoft.Insights/components --query [].name | ConvertFrom-Json)
if ($appInsightsName -and $appInsightsName.Length -eq 1) {
    $appinsightsConfig=$(az monitor app-insights component show --app $appInsightsName -g $resourceGroup -o json | ConvertFrom-Json)

    if ($appinsightsConfig) {
        $appinsightsId = $appinsightsConfig.instrumentationKey         
        $appinsightsConnectionString = $appinsightsConfig.connectionString   
    }
}
Write-Host "App Insights Instrumentation Key: $appinsightsId" -ForegroundColor Yellow

## Getting OpenAI info
$openAi=$(az cognitiveservices account list -g $resourceGroup --query "[?kind=='OpenAI'].{name: name, kind:kind, endpoint: properties.endpoint}" -o json | ConvertFrom-Json)
$openAiKey=$(az cognitiveservices account keys list -g $resourceGroup -n $openAi.name -o json --query key1 | ConvertFrom-Json)
$openAiDeployment = "completions"

$apiIdentityClientId=$(az identity show -g $resourceGroup -n mi-api-coreclaims-$suffix -o json | ConvertFrom-Json).clientId
$workerIdentityClientId=$(az identity show -g $resourceGroup -n mi-worker-coreclaims-$suffix -o json | ConvertFrom-Json).clientId
$tenantId=$(az account show --query homeTenantId --output tsv)

## Getting Frontdoor info
$frontdoor=$(az afd profile list -g $resourceGroup -o json | ConvertFrom-Json).name
Write-Host "az afd profile list -g $resourceGroup -o json"
Write-Host $frontdoor
$fdEndpoint=$(az afd endpoint list -g $resourceGroup --profile-name $frontdoor -o json | ConvertFrom-Json).hostName
Write-Host "az afd endpoint list -g $resourceGroup --profile-name $frontdoor.name -o json"
Write-Host $fdEndpoint

## Showing Values that will be used

Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "settings.json files will be generated with values:"

$tokens.cosmosDbConnectionString="AccountEndpoint=$($docdb.documentEndpoint);AccountKey=$docdbKey"
$tokens.cosmosEndpoint=$docdb.documentEndpoint
$tokens.eventHubEndpoint="https://$eventHubName.servicebus.windows.net"
$tokens.openAiEndpoint=$openAi.properties.endpoint
$tokens.openAiKey=$openAiKey
$tokens.openAiDeployment=$openAiDeployment
$tokens.apiUrl="https://${fdEndpoint}/api"
$tokens.apiClientId=$apiIdentityClientId
$tokens.workerClientId=$workerIdentityClientId
$tokens.tenantId=$tenantId
$tokens.aiConnectionString=$appinsightsConnectionString

# Standard fixed tokens
$tokens.ingressclass=$ingressClass
$tokens.ingressrewritepath="(/|$)(.*)"
$tokens.ingressrewritetarget="`$2"

if($ingressClass -eq "nginx") {
    $tokens.ingressrewritepath="(/|$)(.*)" 
    $tokens.ingressrewritetarget="`$2"
}

Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

$accountGeneratorSettingsTemplate="..,..,src,account-generator,local.settings.template.json"
$accountGeneratorSettings="..,..,src,account-generator,local.settings.json"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$accountGeneratorSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $accountGeneratorSettingsTemplate.Split(","))
$accountGeneratorSettingsPath=$(./Join-Path-Recursively -pathParts $accountGeneratorSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $accountGeneratorSettingsTemplatePath -outputFile $accountGeneratorSettingsPath -tokens $tokens
Pop-Location

$eventmonitorSettingsTemplate="..,..,src,CorePayments.EventMonitor,local.settings.template.json"
$eventmonitorSettings="..,..,src,CorePayments.EventMonitor,local.settings.json"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$eventmonitorSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $eventmonitorSettingsTemplate.Split(","))
$eventmonitorSettingsPath=$(./Join-Path-Recursively -pathParts $eventmonitorSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $eventmonitorSettingsTemplatePath -outputFile $eventmonitorSettingsPath -tokens $tokens
Pop-Location

$functionappSettingsTemplate="..,..,src,CorePayments.FunctionApp,local.settings.template.json"
$functionappSettings="..,..,src,CorePayments.FunctionApp,local.settings.json"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$functionappSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $functionappSettingsTemplate.Split(","))
$functionappSettingsPath=$(./Join-Path-Recursively -pathParts $functionappSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $functionappSettingsTemplatePath -outputFile $functionappSettingsPath -tokens $tokens
Pop-Location

$siteSettingsTemplate="..,..,ui,env.template"
$siteSettings="..,..,ui,.env.local"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$siteSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $siteSettingsTemplate.Split(","))
$siteSettingsPath=$(./Join-Path-Recursively -pathParts $siteSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $siteSettingsTemplatePath -outputFile $siteSettingsPath -tokens $tokens
Pop-Location
