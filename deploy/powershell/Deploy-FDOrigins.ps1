Param(
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$true)][string]$locations
)

$locArray = $locations.Split(",")

$fdProfile = $(az afd profile list -g $resourceGroup --query "[].{name: name}" -o json | ConvertFrom-Json)
$fdEndpoint = $(az afd endpoint list -g $resourceGroup --profile-name $fdProfile.name --query "[].{name: name}" -o json | ConvertFrom-Json)
$fdOriginGroup = $(az afd origin-group list -g $resourceGroup --profile-name $fdProfile.name --query "[].{name: name}" -o json | ConvertFrom-Json)

$i = 1
foreach($location in $locArray)
{
    $queryString = "[?location=='$($location.toLower())'].{name: name,  endpoint: addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName}"
    $aks = $(az aks list -g $resourceGroup --query $queryString -o json | ConvertFrom-Json)

    az afd origin create --resource-group $resourceGroup --host-name $aks.endpoint --profile-name $fdProfile.name --origin-group-name $fdOriginGroup.name --origin-name $aks.name --origin-host-header $aks.endpoint --priority $i --weight 1000 --enabled-state Enabled --http-port 80 --https-port 443
    az afd route create --resource-group $resourceGroup --profile-name $fdProfile.name --endpoint-name $fdEndpoint.name --forwarding-protocol HttpsOnly --route-name $aks.name --https-redirect Enabled --origin-group $fdOriginGroup.name --supported-protocols Https --link-to-default-domain Enabled

    $i++
}