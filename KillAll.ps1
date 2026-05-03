taskkill /F /IM CategoryMicroservices.exe 2>$null
taskkill /F /IM ProductMicroservices.exe 2>$null
taskkill /F /IM PurchaseMicroservices.exe 2>$null
taskkill /F /IM UserMicroservices.exe 2>$null
taskkill /F /IM iisexpress.exe 2>$null
taskkill /F /IM dotnet.exe 2>$null
Write-Host "All killed. Safe to rebuild!" -ForegroundColor Green
