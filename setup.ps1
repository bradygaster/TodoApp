param (
    $resourceBaseName="todoapp$( Get-Random -Maximum 1000)",
    $sqlServerDbUsername='todoapp',
    $sqlServerDbPwd='pass4Sql!4242',
    $location='westus'
)

dotnet tool install --global Zipper --version 1.0.1

dotnet build TodoApp.API\TodoApp.API.csproj
dotnet publish TodoApp.API\TodoApp.API.csproj --self-contained -r win-x86 -o publish\api
zipper compress -i publish\api -o api.zip

dotnet build TodoApp.Web\TodoApp.Web.csproj
dotnet publish TodoApp.Web\TodoApp.Web.csproj --self-contained -r win-x86 -o publish\web
zipper compress -i publish\web -o web.zip

#az group create -l westus -n $resourceBaseName
#az deployment group create --resource-group $resourceBaseName --template-file azure.bicep --parameters resourceBaseName=$resourceBaseName --parameters sqlUsername=$sqlServerDbUsername --parameters sqlPassword=$sqlServerDbPwd 
#az webapp deploy -n "$($resourceBaseName)-api" -g $resourceBaseName --src-path api.zip
#az webapp deploy -n "$($resourceBaseName)-web" -g $resourceBaseName --src-path web.zip
#az webapp browse -n "$($resourceBaseName)-web" -g $resourceBaseName