param sqlUsername string = 'sqluser'
param sqlPassword string = 'zjnv8374mt7yj867g6'
param basename string

resource sqlServer 'Microsoft.Sql/servers@2014-04-01' ={
  name: '${basename}srv'
  location: resourceGroup().location
  properties: {
    administratorLogin: sqlUsername
    administratorLoginPassword: sqlPassword
  }
}

resource sqlFirewallRules 'Microsoft.Sql/servers/firewallRules@2014-04-01' = {
  parent: sqlServer
  name: 'dbfirewallrules'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlServerDatabase 'Microsoft.Sql/servers/databases@2014-04-01' = {
  parent: sqlServer
  name: '${basename}db'
  location: resourceGroup().location
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    edition: 'Basic'
    maxSizeBytes: '2147483648'
    requestedServiceObjectiveName: 'Basic'
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: '${basename}hostingplan'
  location: resourceGroup().location
  sku: {
    name: 'F1'
    capacity: 1
  }
}

resource appInsightsComponents 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: '${basename}ai'
  location: resourceGroup().location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}


resource api 'Microsoft.Web/sites@2018-11-01' = {
  name: '${basename}-api'
  location: resourceGroup().location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Development'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsComponents.properties.InstrumentationKey
        }
      ]
      connectionStrings: [
        {
          name: 'TodoDb'
          connectionString: 'Data Source=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433; Initial Catalog=${basename}db;User Id=${sqlUsername};Password=${sqlPassword};'
        }
      ]
    }
  }
}

resource ui 'Microsoft.Web/sites@2018-11-01' = {
  name: '${basename}-web'
  location: resourceGroup().location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Development'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsComponents.properties.InstrumentationKey
        }
        {
          name: 'ApiUrlBase'
          value: 'https://${api.properties.defaultHostName}'
        }
      ]
    }
  }
}

output ApiUrlBase string = api.properties.defaultHostName
