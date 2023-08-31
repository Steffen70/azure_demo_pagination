Write-Host "Stopping Web App and rebooting Redis Cache..."

az webapp stop --resource-group SPAGDS-Dev-RG --name SPAGDS-DevWebApp

az redis force-reboot --name SPAGDS-Dev-RedisDB --resource-group SPAGDS-Dev-RG --reboot-type AllNodes

$publishPath = "./publish"
$zipPath =  $publishPath + ".zip"

if(Test-Path $publishPath) {
    Remove-Item $publishPath -Force -Recurse
}

if(Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Deploying .NET Web API..."
dotnet publish -c Debug -o $publishPath -r win-x86 --self-contained true

Write-Host "Zipping the publish folder..."
Compress-Archive -Path ($publishPath + "/*") -DestinationPath $zipPath

Write-Host "Deploying to Azure Web App..."
az webapp deployment source config-zip --resource-group SPAGDS-Dev-RG --name SPAGDS-DevWebApp --src $zipPath

Write-Host "Deployment Completed!"