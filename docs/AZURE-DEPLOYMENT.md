# Azure Deployment Guide - PoLearnCert

## Quick Deploy to Azure (Manual Steps)

Due to Azure subscription quota limitations, follow these steps to deploy manually:

### Prerequisites
- Azure CLI installed and logged in: `az login`
- Azure subscription with available quota

### Option 1: Deploy to PoLearnCert Resource Group (Recommended)

#### Step 1: Create Resources

```powershell
# Set variables
$resourceGroup = "PoLearnCert"
$location = "eastus"
$appName = "polearncert-$(Get-Random -Minimum 1000 -Maximum 9999)"
$storageName = "polearncert$(Get-Random -Minimum 1000 -Maximum 9999)"

# Create resource group
az group create --name $resourceGroup --location $location

# Create storage account
az storage account create `
    --name $storageName `
    --resource-group $resourceGroup `
    --location $location `
    --sku Standard_LRS

# Get connection string
$connectionString = az storage account show-connection-string `
    --name $storageName `
    --resource-group $resourceGroup `
    --query connectionString `
    --output tsv

Write-Host "Storage Connection String: $connectionString"
```

#### Step 2: Create App Service (Choose One Method)

**Method A: Use Existing Shared Plan** (if quota limited)
```powershell
# Use existing PoShared plan from another resource group
az webapp create `
    --name $appName `
    --resource-group "PoShared" `
    --plan "PoShared" `
    --runtime "DOTNETCORE:9.0"
```

**Method B: Create New Plan** (if quota available)
```powershell
# Create new plan (requires quota)
az appservice plan create `
    --name "PoLearnCertPlan" `
    --resource-group $resourceGroup `
    --location $location `
    --sku F1 `
    --is-linux

az webapp create `
    --name $appName `
    --resource-group $resourceGroup `
    --plan "PoLearnCertPlan" `
    --runtime "DOTNETCORE:9.0"
```

#### Step 3: Configure App Settings

```powershell
az webapp config appsettings set `
    --name $appName `
    --resource-group $resourceGroup `
    --settings `
        "AzureTableStorage__ConnectionString=$connectionString" `
        "ASPNETCORE_ENVIRONMENT=Production"
```

#### Step 4: Build and Deploy

```powershell
# Build the application
cd C:\Users\punko\Downloads\PoLearnCert\PoLearnCert
dotnet publish src/Po.LearnCert.Api/Po.LearnCert.Api.csproj `
    --configuration Release `
    --output ./publish

# Create ZIP package
Compress-Archive -Path "publish\*" -DestinationPath "publish.zip" -Force

# Deploy
az webapp deploy `
    --name $appName `
    --resource-group $resourceGroup `
    --src-path "publish.zip" `
    --type zip

# Open in browser
Write-Host "`nApplication URL: https://$appName.azurewebsites.net"
Start-Process "https://$appName.azurewebsites.net"
```

### Option 2: Deploy via Azure Portal (No CLI)

1. **Create Resource Group**
   - Navigate to https://portal.azure.com
   - Click "Resource groups" → "+ Create"
   - Name: `PoLearnCert`
   - Region: `East US`
   - Click "Review + create"

2. **Create Storage Account**
   - In PoLearnCert resource group, click "+ Create"
   - Search "Storage account" → Create
   - Name: `polearncert####` (must be unique)
   - Performance: Standard
   - Redundancy: LRS
   - Click "Review + create"
   - After creation, go to "Access keys" → Copy "Connection string"

3. **Create Web App**
   - In PoLearnCert resource group, click "+ Create"
   - Search "Web App" → Create
   - Name: `polearncert-####` (must be globally unique)
   - Publish: Code
   - Runtime stack: .NET 9
   - Operating System: Linux
   - Region: East US
   - App Service Plan: Create new (F1 Free tier) or use existing
   - Click "Review + create"

4. **Configure App Settings**
   - Open the Web App resource
   - Go to "Configuration" → "Application settings"
   - Click "+ New application setting"
   - Name: `AzureTableStorage__ConnectionString`
   - Value: (paste connection string from step 2)
   - Click "OK" → "Save"

5. **Deploy Application**
   - Build locally:
     ```powershell
     cd C:\Users\punko\Downloads\PoLearnCert\PoLearnCert
     dotnet publish src/Po.LearnCert.Api/Po.LearnCert.Api.csproj -c Release -o ./publish
     Compress-Archive -Path "publish\*" -DestinationPath "publish.zip" -Force
     ```
   - In Azure Portal, go to Web App → "Deployment Center"
   - Choose "ZIP Deploy" → Upload `publish.zip`
   - Wait for deployment to complete

6. **Seed Questions**
   - Update `tools/Po.LearnCert.QuestionGenerator/appsettings.json`:
     ```json
     {
       "AzureTableStorage": {
         "ConnectionString": "YOUR_PRODUCTION_CONNECTION_STRING"
       }
     }
     ```
   - Run generator:
     ```powershell
     cd tools/Po.LearnCert.QuestionGenerator
     dotnet run -- 50 -y
     ```

### Post-Deployment

1. **Verify Deployment**
   - Navigate to `https://YOUR-APP-NAME.azurewebsites.net`
   - Should see Blazor app load
   - Check `/api/health` endpoint returns 200 OK

2. **Monitor Application**
   - Azure Portal → Web App → "Log stream"
   - Check for errors or warnings

3. **Configure Custom Domain** (Optional)
   - Azure Portal → Web App → "Custom domains"
   - Follow wizard to add custom domain

## Troubleshooting

### Quota Issues
**Error**: "Operation cannot be completed without additional quota"

**Solution**: Use existing App Service Plan from another resource group:
```powershell
az appservice plan list --query "[].{Name:name, RG:resourceGroup, Location:location}" -o table
# Use an existing plan with available capacity
```

### .NET Runtime Not Found
**Error**: Application doesn't start after deployment

**Solution**: Verify runtime configuration:
```powershell
az webapp config show --name $appName --resource-group $resourceGroup --query linuxFxVersion
```
Should return: `DOTNETCORE|9.0`

### Storage Connection Issues
**Error**: "Table storage connection failed"

**Solution**: Verify connection string in Application Settings:
```powershell
az webapp config appsettings list --name $appName --resource-group $resourceGroup
```

### Deployment Hangs
**Solution**: Use async deployment and check status:
```powershell
az webapp deploy --name $appName --resource-group $resourceGroup --src-path publish.zip --type zip --async true

# Check deployment logs
az webapp log tail --name $appName --resource-group $resourceGroup
```

## Resource Summary

After successful deployment, you'll have:

| Resource Type | Name | Purpose |
|--------------|------|---------|
| Resource Group | PoLearnCert | Container for all resources |
| Storage Account | polearncert#### | Azure Table Storage for data |
| App Service Plan | PoLearnCertPlan or PoShared | Hosting infrastructure |
| Web App | polearncert-#### | ASP.NET Core application |

## Cost Estimate

- **Free Tier (F1)**:
  - App Service: $0/month (1 GB RAM, 60 min/day compute)
  - Storage Account: ~$0.50/month (LRS, <1GB data)
  - **Total**: ~$0.50/month

- **Basic Tier (B1)** (recommended for production):
  - App Service: ~$13/month (1.75 GB RAM, unlimited compute)
  - Storage Account: ~$0.50/month
  - **Total**: ~$13.50/month

## Next Steps

1. ✅ Deploy to PoLearnCert resource group
2. ✅ Configure storage connection string
3. ✅ Seed questions using AI generator
4. ✅ Test application end-to-end
5. Configure monitoring (Application Insights)
6. Set up CI/CD pipeline (GitHub Actions)
7. Configure custom domain (optional)

---

**Created**: October 27, 2025  
**Last Updated**: October 27, 2025
