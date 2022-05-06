param location string = resourceGroup().location
@secure()
param sqlUsername string
@secure()
param sqlPassword string

// create the azure container registry
resource acr 'Microsoft.ContainerRegistry/registries@2021-09-01' = {
  name: toLower('${resourceGroup().name}acr')
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// create the sql database server and database
module sql 'sql.bicep' = {
  name: 'sql'
  params: {
    sqlPassword: sqlPassword
    sqlUsername: sqlUsername
  }
}

// create the aca environment
module env 'environment.bicep' = {
  name: 'containerAppEnvironment'
  params: {
    location: location
  }
}

// create the various config pairs
var shared_config = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Development'
  }
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: env.outputs.appInsightsInstrumentationKey
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: env.outputs.appInsightsConnectionString
  }
]


var backend_config = [
  {
    name: 'TodoDb'
    value: sql.outputs.sqlDbConnectionString
  }
]

var frontend_config = [
  {
    name: 'ApiUrlBase'
    value: 'http://${backend.outputs.fqdn}'
  }
]

// create the backend api container app
module backend 'container_app.bicep' = {
  name: 'backend'
  params: {
    name: 'backend'
    location: location
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    envVars: union(shared_config, backend_config)
    externalIngress: false
  }
}

// create the frontend razor container app
module razorfrontend 'container_app.bicep' = {
  name: 'razor'
  params: {
    name: 'razor'
    location: location
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    envVars: union(shared_config, frontend_config)
    externalIngress: true
  }
}

// create the frontend blazor container app
module blazorfrontend 'container_app.bicep' = {
  name: 'blazor'
  params: {
    name: 'blazor'
    location: location
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    envVars: union(shared_config, frontend_config)
    externalIngress: true
  }
}
