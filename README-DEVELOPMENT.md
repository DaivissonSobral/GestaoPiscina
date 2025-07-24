# Instruções de Desenvolvimento

## Como rodar o projeto

### Opção 1: Script automatizado (Recomendado)
```powershell
.\run-dev.ps1
```

### Opção 2: Manual
1. **Inicie o servidor API primeiro:**
   ```powershell
   dotnet run --project GestaoPiscina.Server
   ```

2. **Em outro terminal, inicie o cliente Blazor:**
   ```powershell
   dotnet run --project GestaoPiscina.Client
   ```

## Portas utilizadas
- **Servidor API**: http://localhost:7001
- **Cliente Blazor**: http://localhost:7000

## Solução de problemas

### Erro "An unhandled error has occurred"
Este erro geralmente ocorre quando:
1. O servidor API não está rodando
2. Há problemas de conexão entre cliente e servidor

**Solução:**
- Certifique-se de que ambos os projetos estão rodando
- Verifique se a porta 7001 está livre
- Use o script `run-dev.ps1` para facilitar

### Erro de conexão com API
O cliente agora trata erros de conexão graciosamente:
- Retorna listas vazias em vez de falhar
- Logs de erro no console do navegador
- Interface continua funcionando mesmo sem API

## Estrutura do projeto
- `GestaoPiscina.Server/` - API REST (ASP.NET Core)
- `GestaoPiscina.Client/` - Interface Blazor WebAssembly
- `Docs/` - Documentação e especificações 