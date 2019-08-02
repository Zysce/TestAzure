$rgName = "test-rg-al"
az login

az vm create `
    --resource-group $rgName `
    --name myDockerVM `
    --image UbuntuLTS `
    --admin-username azureuser `
    --generate-ssh-keys

ssh azureuser@publicIpAddress

sudo apt install docker.io -y

sudo docker run -it hello-world