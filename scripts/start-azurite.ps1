# Start Azurite for local Azure Table Storage development
# Requires: npm install -g azurite

Write-Host "Starting Azurite (Azure Storage Emulator)..." -ForegroundColor Green

# Check if azurite is installed
if (!(Get-Command azurite -ErrorAction SilentlyContinue)) {
    Write-Host "Azurite is not installed. Install it with: npm install -g azurite" -ForegroundColor Red
    exit 1
}

# Start Azurite with table storage only
azurite --silent --location ./azurite-data --debug ./azurite-debug.log

# Note: Use Ctrl+C to stop Azurite
