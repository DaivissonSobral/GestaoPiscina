# Script para limpar cache e reiniciar o projeto
Write-Host "Limpando cache do projeto..." -ForegroundColor Yellow

# Limpar bin e obj
Remove-Item -Path "GestaoPiscina.Client\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "GestaoPiscina.Client\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "GestaoPiscina.Server\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "GestaoPiscina.Server\obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Cache limpo! Agora execute:" -ForegroundColor Green
Write-Host ".\run-dev.ps1" -ForegroundColor Cyan 