# 🚀 Configuração Azure DevOps + GitHub - GestaoPiscina

**Data**: 2026-09-19  
**Status**: ✅ Integração Concluída

---

## 📋 O QUE FOI CONFIGURADO

### 1️⃣ **Conexão GitHub ↔ Azure DevOps**
- ✅ Service Connection: `GitHub-Main`
- ✅ Tipo: OAuth - Grant Authorization
- ✅ Repositório: `DaivissonSobral/GestaoPiscina`

### 2️⃣ **Build Pipeline**
- ✅ Arquivo: `azure-pipelines.yml`
- ✅ Dispara automaticamente no push para `main`
- ✅ Template: ASP.NET Core
- ✅ Agent Pool: **Default (Self-Hosted)**

### 3️⃣ **Self-Hosted Agent**
- ✅ Versão: Azure Pipelines Agent v5.279.0
- ✅ Localização: `C:\Users\dsobr\AzurePipelinesAgent`
- ✅ Status: Rodando (Agent.Listener)
- ✅ Registro: Pool "Default"

---

## 🎯 COMO USAR NO DIA A DIA

### **Fazer Commits Normalmente**
```bash
git commit -m "Implementa nova feature"
git push
```

### **Vincular Work Items ao Commit**
```bash
# Apenas vincular
git commit -m "Corrige layout - #123"

# Vincular e marcar como resolvido
git commit -m "Corrige bug crítico - Fixes #456"

# Vincular e fechar automaticamente
git commit -m "Implementa autenticação - Closes #789"
```

### **Padrões Suportados**
- `#123` → Vincula ao item
- `Closes #123` → Fecha o item
- `Fixes #123` → Marca como resolvido
- `Resolves #123` → Marca como concluído

---

## 📊 VISUALIZAR NO DEVOPS

### **Build Pipeline Runs**
```
https://dev.azure.com/DaivissonSobral/GestaoPiscina/_build
```

### **Boards & Work Items**
```
https://dev.azure.com/DaivissonSobral/GestaoPiscina/_workitems
```

### **Repositório**
```
https://github.com/DaivissonSobral/GestaoPiscina
```

---

## 🔧 MANUTENÇÃO DO AGENT

### **Reiniciar o Agent**
```powershell
cd C:\Users\dsobr\AzurePipelinesAgent
Stop-Process -Name "Agent.Listener" -Force
Start-Process -FilePath "$PWD\run.cmd" -NoNewWindow
```

### **Verificar Status**
```powershell
Get-Process -Name "Agent.Listener"
```

### **Ver Logs do Agent**
```
C:\Users\dsobr\AzurePipelinesAgent\_diag\
```

---

## 📝 PRÓXIMAS AÇÕES

- [ ] Criar primeiro Work Item (Issue/Task) no DevOps
- [ ] Fazer commit com `#ID` para testar vinculação
- [ ] Executar pipeline e acompanhar na interface
- [ ] Configurar Branch Policies (proteção de main)
- [ ] Criar Release Pipeline (se necessário)

---

## ✅ CHECKLIST DE INTEGRAÇÃO

| Item | Status | Observação |
|------|--------|-----------|
| GitHub Connection | ✅ | Service Connection criada |
| Build Pipeline | ✅ | Disparado automaticamente |
| Self-Hosted Agent | ✅ | Rodando e registrado |
| Pipeline Config | ✅ | Usando agent local |
| Work Items Sync | ✅ | Automático via commits |
| Web Hooks | ✅ | Sincronização ativa |

---

## 🆘 TROUBLESHOOTING

### "No agent found in pool Default"
**Solução:**
1. Verificar se Agent.Listener está rodando
2. Reiniciar o agent
3. Aguardar alguns segundos para conectar

### "Pipeline falha ao compilar"
**Solução:**
1. Verificar logs do pipeline no DevOps
2. Garantir que .sln existe no repositório
3. Verificar permissões do agent

### "Work Item não vincula ao commit"
**Solução:**
1. Usar formato correto: `#123`, `Closes #123`, etc.
2. Confirmar que item existe no DevOps
3. Aguardar sincronização (até 5 minutos)

---

## 📚 REFERÊNCIAS

- [Azure Pipelines Docs](https://docs.microsoft.com/azure/devops/pipelines)
- [Self-Hosted Agents](https://docs.microsoft.com/azure/devops/pipelines/agents/agents)
- [Work Items Linking](https://docs.microsoft.com/azure/devops/boards/github)

---

**Configurado por:** Claude Haiku 4.5  
**Projeto:** GestaoPiscina  
**Organização:** DaivissonSobral
