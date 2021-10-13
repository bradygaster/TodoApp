param (
    $resourceBaseName="todoapp$( Get-Random -Maximum 1000)",
    $sqlServerDbUsername='todoapp',
    $sqlServerDbPwd='pass4Sql!4242',
    $location='westus'
)

if (Test-Path .\api.zip) {
    Remove-Item .\api.zip
}

if (Test-Path .\razor.zip) {
    Remove-Item .\razor.zip
}

if (Test-Path .\blazorserver.zip) {
    Remove-Item .\blazorserver.zip
}

try {
    dotnet tool install --global Zipper --version 1.0.1
} catch {
    Write-Host "Environment already has Zipper installed. Yay!" -ForegroundColor Yellow
}

dotnet build TodoApp.API\TodoApp.API.csproj
dotnet publish TodoApp.API\TodoApp.API.csproj --self-contained -r win-x86 -o publish\api
zipper compress -i publish\api -o api.zip

dotnet build TodoApp.Web.Razor\TodoApp.Web.Razor.csproj
dotnet publish TodoApp.Web.Razor\TodoApp.Web.Razor.csproj --self-contained -r win-x86 -o publish\razor
zipper compress -i publish\razor -o razor.zip

dotnet build TodoApp.Web.BlazorServer\TodoApp.Web.BlazorServer.csproj
dotnet publish TodoApp.Web.BlazorServer\TodoApp.Web.BlazorServer.csproj --self-contained -r win-x86 -o publish\blazorserver
zipper compress -i publish\blazorserver -o blazorserver.zip

az group create -l westus -n $resourceBaseName
az deployment group create --resource-group $resourceBaseName --template-file azure.bicep --parameters resourceBaseName=$resourceBaseName --parameters sqlUsername=$sqlServerDbUsername --parameters sqlPassword=$sqlServerDbPwd 
az webapp deploy -n "$($resourceBaseName)-api" -g $resourceBaseName --src-path api.zip
az webapp deploy -n "$($resourceBaseName)-blazorserver" -g $resourceBaseName --src-path blazorserver.zip
az webapp deploy -n "$($resourceBaseName)-razor" -g $resourceBaseName --src-path razor.zip
az webapp browse -n "$($resourceBaseName)-blazorserver" -g $resourceBaseName
az webapp browse -n "$($resourceBaseName)-razor" -g $resourceBaseName