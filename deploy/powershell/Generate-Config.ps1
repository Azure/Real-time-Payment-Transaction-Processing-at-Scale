Param(
    [parameter(Mandatory=$true)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$locations,
    [parameter(Mandatory=$true)][string]$suffix,
    [parameter(Mandatory=$false)][string[]]$outputFile=$null,
    [parameter(Mandatory=$false)][string[]]$gvaluesTemplate="..,..,gvalues.template.yml",
    [parameter(Mandatory=$false)][string]$ingressClass="addon-http-application-routing",
    [parameter(Mandatory=$false)][string]$domain
)

$locArray = $locations.Split(',')

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

## Getting OpenAI info
$openAi=$(az cognitiveservices account list -g $resourceGroup --query "[?kind=='OpenAI'].{name: name, kind:kind, endpoint: properties.endpoint}" -o json | ConvertFrom-Json)
$openAiKey=$(az cognitiveservices account keys list -g $resourceGroup -n $openAi.name -o json --query key1 | ConvertFrom-Json)
$openAiDeployment = "completions"

$apiIdentityClientId=$(az identity show -g $resourceGroup -n miapi$suffix -o json | ConvertFrom-Json).clientId
$workerIdentityClientId=$(az identity show -g $resourceGroup -n miworker$suffix -o json | ConvertFrom-Json).clientId
$tenantId=$(az account show --query homeTenantId --output tsv)

## Getting Frontdoor info
$frontdoor=$(az afd profile list -g $resourceGroup -o json | ConvertFrom-Json).name
Write-Host "az afd profile list -g $resourceGroup -o json"
Write-Host $frontdoor
$fdEndpoint=$(az afd endpoint list -g $resourceGroup --profile-name $frontdoor -o json | ConvertFrom-Json).hostName
Write-Host "az afd endpoint list -g $resourceGroup --profile-name $frontdoor.name -o json"
Write-Host $fdEndpoint

$aksInstances=$(az aks list -g $resourceGroup --query "[].{name: name, endpoint: addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName}" -o json | ConvertFrom-Json)
$appInsightsNames=$(az resource list -g $resourceGroup --resource-type Microsoft.Insights/components --query [].name -o json| ConvertFrom-Json)

for ($i = 0; $i -lt 3; $i++)
{
    ## Getting App Insights instrumentation key, if required
    $appinsightsConfig=$(az monitor app-insights component show --app $appInsightsNames[$i] -g $resourceGroup -o json | ConvertFrom-Json)

    if ($appinsightsConfig) {
        $appinsightsId = $appinsightsConfig.instrumentationKey         
        $appinsightsConnectionString = $appinsightsConfig.connectionString   
    }
    Write-Host "App Insights Instrumentation Key: $appinsightsId" -ForegroundColor Yellow

    ## Showing Values that will be used

    Write-Host "===========================================================" -ForegroundColor Yellow
    Write-Host "settings.json files will be generated with values:"

    $tokens.cosmosDbConnectionString="AccountEndpoint=$($docdb.documentEndpoint);AccountKey=$docdbKey"
    $tokens.cosmosEndpoint=$docdb.documentEndpoint
    $tokens.eventHubEndpoint="https://$eventHubName.servicebus.windows.net"
    $tokens.openAiEndpoint=$openAi.endpoint
    $tokens.openAiKey=$openAiKey
    $tokens.openAiCompletionsDeployment=$openAiDeployment
    $tokens.apiUrl="https://${fdEndpoint}/api"
    $tokens.apiClientId=$apiIdentityClientId
    $tokens.workerClientId=$workerIdentityClientId
    $tokens.tenantId=$tenantId
    $tokens.aiConnectionString=$appinsightsConnectionString
    $tokens.aksName=$aksInstances[$i].name
    $tokens.aksEndpoint=$aksInstances[$i].endpoint

    $indices = 0..($locArray.length-1) | ForEach-Object {($_ + $i) % $locArray.Length}

    $tokens.preferredLocations=[system.String]::Join(",", $locArray[$indices].Trim())

    # Standard fixed tokens
    $tokens.ingressclass=$ingressClass
    $tokens.ingressrewritepath="(.*)"
    $tokens.ingressrewritetarget="`$1"

    if($ingressClass -eq "nginx") {
        $tokens.ingressrewritepath="(/|$)(.*)" 
        $tokens.ingressrewritetarget="`$2"
    }

    Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
    Write-Host "===========================================================" -ForegroundColor Yellow

    Push-Location $($MyInvocation.InvocationName | Split-Path)
    $gvaluesTemplatePath=$(./Join-Path-Recursively -pathParts $gvaluesTemplate.Split(","))
    Write-Host $gvaluesTemplatePath
    $outputFilePath=$(./Join-Path-Recursively -pathParts $outputFile.Split(","))
    Write-Host "${outputFilePath}${i}.yml"
    & ./Token-Replace.ps1 -inputFile $gvaluesTemplatePath -outputFile "${outputFilePath}${i}.yml" -tokens $tokens
    Pop-Location
}

$accountGeneratorSettingsTemplate="..,..,src,account-generator,local.settings.template.json"
$accountGeneratorSettings="..,..,src,account-generator,local.settings.json"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$accountGeneratorSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $accountGeneratorSettingsTemplate.Split(","))
$accountGeneratorSettingsPath=$(./Join-Path-Recursively -pathParts $accountGeneratorSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $accountGeneratorSettingsTemplatePath -outputFile $accountGeneratorSettingsPath -tokens $tokens
Pop-Location

$webapiSettingsTemplate="..,..,src,CorePayments.WebAPI,appsettings.Development.template.json"
$webapiSettings="..,..,src,CorePayments.WebAPI,appsettings.Development.json"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$webapiSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $webapiSettingsTemplate.Split(","))
$webapiSettingsPath=$(./Join-Path-Recursively -pathParts $webapiSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $webapiSettingsTemplatePath -outputFile $webapiSettingsPath -tokens $tokens
Pop-Location

$workerserviceSettingsTemplate="..,..,src,CorePayments.WorkerService,appsettings.Development.template.json"
$workerserviceSettings="..,..,src,CorePayments.WorkerService,appsettings.Development.json"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$workerserviceSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $workerserviceSettingsTemplate.Split(","))
$workerserviceSettingsPath=$(./Join-Path-Recursively -pathParts $workerserviceSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $workerserviceSettingsTemplatePath -outputFile $workerserviceSettingsPath -tokens $tokens
Pop-Location

$siteSettingsTemplate="..,..,ui,env.template"
$siteSettings="..,..,ui,.env.local"
Push-Location $($MyInvocation.InvocationName | Split-Path)
$siteSettingsTemplatePath=$(./Join-Path-Recursively -pathParts $siteSettingsTemplate.Split(","))
$siteSettingsPath=$(./Join-Path-Recursively -pathParts $siteSettings.Split(","))
& ./Token-Replace.ps1 -inputFile $siteSettingsTemplatePath -outputFile $siteSettingsPath -tokens $tokens
Pop-Location
