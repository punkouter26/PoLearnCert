@description('The name of the Container App')
param name string

@description('The location for the resource')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The Container Apps Environment ID')
param containerAppsEnvironmentId string

@description('The container image to deploy')
param containerImage string

@description('The target port for the container')
param targetPort int = 8080

@description('Environment variables for the container')
param env array = []

@description('CPU cores for the container')
param cpu string = '0.5'

@description('Memory for the container')
param memory string = '1Gi'

@description('Minimum number of replicas')
param minReplicas int = 0

@description('Maximum number of replicas')
param maxReplicas int = 3

// Container App
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: targetPort
        transport: 'http'
        allowInsecure: false
      }
    }
    template: {
      containers: [
        {
          name: 'main'
          image: containerImage
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          env: env
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

output id string = containerApp.id
output name string = containerApp.name
output uri string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
