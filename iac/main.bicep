targetScope = 'subscription'

@minLength(1)
@maxLength(17)
@description('Prefix for all resources, i.e. {basename}storage')
param basename string

@description('Primary location for all resources')
param location string = 'westus2'

param principalId string = ''

resource rg 'Microsoft.Resources/resourceGroups@2020-06-01' = {
  name: '${basename}rg'
  location: location
}

module resources './resources.bicep' = {
  name: '${rg.name}-resources'
  scope: rg
  params: {
    basename: toLower(basename)
  }
}

output ApiUrlBase string = resources.outputs.ApiUrlBase
