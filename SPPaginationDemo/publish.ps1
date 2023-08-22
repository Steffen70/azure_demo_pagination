$publishPath = "./publish"
$zipPath =  $publishPath + ".zip"

if(Test-Path $publishPath) {
    Remove-Item $publishPath -Force -Recurse
}

if(Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Step 1: Deploy Your .NET Web API
Write-Host "Deploying .NET Web API..."
dotnet publish -c Debug -o $publishPath -r win-x86 --self-contained true

# Step 2: Zip the contents of the publish folder
Write-Host "Zipping the publish folder..."
Compress-Archive -Path ($publishPath + "/*") -DestinationPath $zipPath

# Step 3: Deploy to Azure Web App
Write-Host "Deploying to Azure Web App..."
az webapp deployment source config-zip --resource-group Demo-Env --name SPPaginationDemo --src $zipPath

Write-Host "Deployment Completed!"