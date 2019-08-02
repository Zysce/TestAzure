$stName = "storagebtest"
az storage account create `
    --resource-group test-rg-al `
    --name $stName `
    --location eastus2 `
    --sku Standard_LRS

az batch account create `
--name batchaccounttestal `
--storage-account $stName `
 --resource-group test-rg-al `
 --location eastus

 az batch account login `
  --name batchaccounttestal `
  --resource-group test-rg-al

 az batch pool create `
    --id "mypool" `
	--vm-size "Standard_A1_v2" `
    --target-dedicated-nodes 2 `
    --image "canonical:ubuntuserver:16.04-LTS" `
    --node-agent-sku-id "batch.node.ubuntu 16.04" 

az batch job create `
--id myjob `
--pool-id mypool


for ($i = 0 ; $i -lt 4 ; $i++) {
	az batch task create `
    --task-id "mytask$($i)" `
    --job-id myjob `
    --command-line "/bin/bash -c 'printenv | grep AZ_BATCH; sleep 90s'"
}