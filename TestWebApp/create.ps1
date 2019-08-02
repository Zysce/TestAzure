az appservice plan create --name myAppServicePlan --resource-group myResourceGroup --sku S1 --is-linux

az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name <app-name> --multicontainer-config-type compose --multicontainer-config-file docker-compose-wordpress.yml
