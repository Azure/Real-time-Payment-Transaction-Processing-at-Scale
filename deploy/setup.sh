SUBSCRIPTION_ID=$1
RESOURCE_GROUP=$2
LOCATIONS=$3
MULTIMASTER=$4
SUFFIX=$5

echo 'Subscription Id     :' $SUBSCRIPTION_ID
echo 'Resource Group      :' $RESOURCE_GROUP
echo 'Locations           :' $LOCATIONS
echo 'Enable Multi Master :' $MULTIMASTER
echo 'Deploy Suffix       :' $SUFFIX

echo 'Validate variables above and press any key to continue setup...'
read -n 1

#Start infrastructure deployment
cd ../infrastructure
echo "Directory changed: '$(pwd)'"

az account set --subscription $SUBSCRIPTION_ID
az account show

echo 'Validate current subscription and press any key to continue setup...'
read -n 1

arrLOCATIONS=(${LOCATIONS//","/ })

RGCREATED=$(az group create \
                --name $RESOURCE_GROUP \
                --location ${arrLOCATIONS[0]} \
                --query "properties.provisioningState" \
                -o tsv)

if [ "$RGCREATED" != "Succeeded" ] 
then
    echo 'Resource group creation failed! Exiting...'
    exit
fi

INFRADEPLOYED=$(az deployment group create \
                    --name CosmosDemoDeployment \
                    --resource-group $RESOURCE_GROUP \
                    --template-file ./main.bicep \
                    --parameters suffix=$SUFFIX locations="$LOCATIONS" enableCosmosMultiMaster=$MULTIMASTER \
                    --query "properties.provisioningState" \
                    -o tsv)

if [ "$INFRADEPLOYED" != "Succeeded" ] 
then
    echo 'Infrastructure deployment failed! Exiting...'
    exit
fi

echo 'Press any key to continue setup...'
read -n 1

#eventHubConnection=$(az eventhubs namespace authorization-rule keys list -g $RESOURCE_GROUP --namespace-name eventhubdemo$SUFFIX -n RootManageSharedAccessKey --query primaryConnectionString -o tsv)

#cd ../src/marketdata-generator/
#echo "Directory changed: '$(pwd)'"

## File to modify
#FILE_TO_REPLACE=settings.json

## Pattern for your tokens -- e.g. ${token}
#TOKEN_PATTERN='(?<=\$\{)\w+(?=\})'

## Find all tokens to replace
#TOKENS=$(grep -oP ${TOKEN_PATTERN} ${FILE_TO_REPLACE} | sort -u)

## Loop over tokens and use sed to replace
#for token in $TOKENS
#do
#  echo "Replacing \${${token}} with ${!token}"
#  sed -i "s|\${${token}}|${!token}|" ${FILE_TO_REPLACE}
#done

cd ../src/cosmos-payments-demo

for ((i = 0; i < ${#arrLOCATIONS[@]}; ++i)); do
    func azure functionapp publish functionpay$SUFFIX$i --csharp
done

echo ""
echo "***************************************************"
echo "*************  Deploy completed!  *****************"
echo "Next steps:"
echo "1. Run account-generator"
echo "2. Call APIs"
echo "***************************************************"