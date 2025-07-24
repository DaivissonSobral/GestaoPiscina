# Script para rodar o projeto de desenvolvimento
Write-Host "Iniciando o servidor API..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-Command", "dotnet run --project GestaoPiscina.Server" -WindowStyle Minimized

Write-Host "Aguardando 3 segundos para o servidor inicializar..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

Write-Host "Iniciando o cliente Blazor..." -ForegroundColor Green
dotnet run --project GestaoPiscina.Client 