# Script para rodar o projeto com acesso local e público
# Requer: dotnet e cloudflared instalados

$ErrorActionPreference = "Continue"

Write-Host "=== SETUP GESTAO PISCINA ===" -ForegroundColor Cyan
Write-Host "Iniciando API em http://localhost:7001" -ForegroundColor Green
Start-Process powershell -ArgumentList "-Command", "dotnet run --project GestaoPiscina.Server" -WindowStyle Minimized

Write-Host "Aguardando inicializacao da API..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Iniciando frontend em http://localhost:7000" -ForegroundColor Green
Start-Process powershell -ArgumentList "-Command", "dotnet run --project GestaoPiscina.Client --urls http://localhost:7000" -WindowStyle Minimized

Write-Host "Aguardando inicializacao do frontend..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Iniciando tunnel publico fixo (app.blup.ia.br + api.blup.ia.br)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-Command", "cloudflared tunnel --config C:\Users\dsobr\.cloudflared\config.yml run gestaopiscina" -WindowStyle Minimized

Write-Host ""
Write-Host "URLs disponiveis:" -ForegroundColor Cyan
Write-Host "- Local: http://localhost:7000"
Write-Host "- API local: http://localhost:7001"
Write-Host "- Publico (frontend): https://app.blup.ia.br"
Write-Host "- Publico (API): https://api.blup.ia.br"
Write-Host ""