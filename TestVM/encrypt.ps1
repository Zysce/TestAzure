az provider register -n Microsoft.KeyVault
$resourcegroup="myResourceGroup"
az group create --name $resourcegroup --location eastus

$keyvault_name=myvaultname$RANDOM
az keyvault create `
    --name $keyvault_name `
    --resource-group $resourcegroup `
    --location eastus `
    --enabled-for-disk-encryption True

az keyvault key create `
    --vault-name $keyvault_name `
    --name myKey `
    --protection software

	az vm create `
    --resource-group $resourcegroup `
    --name myVM `
    --image Canonical:UbuntuServer:16.04-LTS:latest `
    --admin-username azureuser `
    --generate-ssh-keys `
    --data-disk-sizes-gb 5

	az vm encryption enable `
    --resource-group $resourcegroup `
    --name myVM `
    --disk-encryption-keyvault $keyvault_name `
    --key-encryption-key myKey `
    --volume-type all

	az vm encryption show --resource-group $resourcegroup --name myVM --query 'status'

	# encrypt new disks:
az vm encryption enable `
    --resource-group $resourcegroup `
    --name myVM `
    --disk-encryption-keyvault $keyvault_name `
    --key-encryption-key myKey `
    --volume-type data