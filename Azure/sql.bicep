@secure()
param sqlUsername string
@secure()
param sqlPassword string
param resourceBaseName string = resourceGroup().name

resource sqlServer 'Microsoft.Sql/servers@2014-04-01' ={
  name: '${resourceBaseName}srv'
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
  name: '${resourceBaseName}db'
  location: resourceGroup().location
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    edition: 'Basic'
    maxSizeBytes: '2147483648'
    requestedServiceObjectiveName: 'Basic'
  }
}

output sqlDbConnectionString string = 'Data Source=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433; Initial Catalog=${resourceBaseName}db;User Id=${sqlUsername};Password=${sqlPassword};'
