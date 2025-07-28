# 📝 COMO USAR O TEMPLATE DE PROMPT

## 🎯 **Arquivo Criado: `PROMPT_TEMPLATE_FINAL.md`**

O template está salvo no arquivo `PROMPT_TEMPLATE_FINAL.md` e está pronto para uso!

## 📋 **Como Usar o Template**

### **1. Copie o Template**
- Abra o arquivo `PROMPT_TEMPLATE_FINAL.md`
- Copie todo o conteúdo

### **2. Preencha as Seções**
Substitua os campos entre `[colchetes]` com suas informações específicas:

#### **📋 REQUISITO ESPECÍFICO**
```markdown
- **RF**: RF12 - Perfis de Acesso
- **RN**: RN09 - Perfis e Restrições de Acesso
- **Stakeholder**: Gestor/Supervisor/Técnico/Cliente
```

#### **🎯 PROBLEMA FOCADO**
```markdown
### **Situação Atual**
Sistema não possui autenticação implementada

### **Problema Específico**
Necessário implementar login com JWT e proteção de rotas

### **Objetivo**
Implementar autenticação completa com 4 perfis de acesso
```

#### **📁 ARQUIVOS ENVOLVIDOS**
```markdown
### **Frontend (Blazor WebAssembly)**
- **Páginas**: Pages/Login.razor, Pages/Home.razor
- **Componentes**: Shared/MainLayout.razor
- **Serviços**: Services/AuthService.cs
- **Modelos**: Models/Usuario.cs

### **Backend (ASP.NET Core 8)**
- **Controllers**: Controllers/AuthController.cs
- **Models**: Models/Usuario.cs
- **Data**: Data/GestaoPiscinaContext.cs
```

#### **❓ PERGUNTA DIRETA**
```markdown
Como implementar autenticação JWT com 4 perfis de acesso e proteção de rotas no Blazor WebAssembly?
```

#### **📊 CRITÉRIOS DE ACEITAÇÃO**
```markdown
### **Funcionalidade**
- [ ] Login com usuário/senha
- [ ] Tokens JWT válidos
- [ ] Proteção de rotas por perfil
- [ ] Logout funcional
```

#### **🎯 RESULTADO ESPERADO**
```markdown
Sistema de autenticação completo com login, logout, proteção de rotas e 4 perfis de acesso funcionais.
```

## 🚀 **Exemplos Práticos**

### **Exemplo 1: Autenticação**
```markdown
**RF**: RF12 - Perfis de Acesso
**Problema**: Sistema sem autenticação
**Pergunta**: Como implementar autenticação JWT?
```

### **Exemplo 2: Checklist**
```markdown
**RF**: RF04 - Ordem de Serviço com checklist
**Problema**: OS sem checklist obrigatório
**Pergunta**: Como implementar checklist interativo?
```

### **Exemplo 3: Cálculo**
```markdown
**RF**: RF05 - Cálculo Automático de Produtos
**Problema**: Sem cálculo de dosagem
**Pergunta**: Como implementar cálculo automático?
```

## 📝 **Passo a Passo**

### **1. Escolha um Requisito**
- Abra `Docs/Especificacao_Requisitos_Versao_Final.txt`
- Escolha um RF (Requisito Funcional)
- Identifique as RNs (Regras de Negócio) relacionadas

### **2. Analise o Status Atual**
- Verifique o que já existe no projeto
- Identifique o que precisa ser implementado
- Liste os arquivos que serão modificados

### **3. Preencha o Template**
- Copie o template de `PROMPT_TEMPLATE_FINAL.md`
- Preencha todas as seções
- Seja específico e direto

### **4. Use em Nova Conversa**
- Cole o prompt preenchido
- Aguarde a implementação
- Teste a funcionalidade

### **5. Documente a Solução**
- Atualize `SOLUCOES.md`
- Marque como concluído em `TODOS.md`
- Faça commit com mensagem clara

## 🎯 **Benefícios do Template**

### ✅ **Eficiência**
- **Conversas curtas** - Nunca atinge limite
- **Foco específico** - Um problema por vez
- **Soluções rápidas** - Implementação direta
- **Documentação clara** - Progresso visível

### ✅ **Qualidade**
- **Contexto completo** - Inclui especificação
- **Arquivos precisos** - Lista exata dos arquivos
- **Critérios definidos** - Aceitação clara
- **Rastreabilidade** - Vinculado aos RFs/RNs

## 📊 **Estrutura do Template**

```markdown
# 🎯 TEMPLATE DE PROMPT OTIMIZADO - GESTÃO PISCINAS

## 📋 **INSTRUÇÕES PARA IA**
[Stack técnico e idioma]

## 🏗️ **CONTEXTO DO PROJETO**
[Especificação e status atual]

## 📋 **REQUISITO ESPECÍFICO**
[RF, RN e stakeholder]

## 🎯 **PROBLEMA FOCADO**
[Situação atual, problema e objetivo]

## 📁 **ARQUIVOS ENVOLVIDOS**
[Lista precisa de arquivos]

## ❓ **PERGUNTA DIRETA**
[Pergunta específica]

## 📊 **CRITÉRIOS DE ACEITAÇÃO**
[Checklist de validação]

## 🎯 **RESULTADO ESPERADO**
[Descrição do que deve ser entregue]
```

## 🚀 **Próximos Passos**

1. **Escolha um requisito** da especificação
2. **Copie o template** de `PROMPT_TEMPLATE_FINAL.md`
3. **Preencha** todas as seções
4. **Use em nova conversa** focada
5. **Implemente** e teste a solução
6. **Documente** no `SOLUCOES.md`

**Com este template, você nunca mais terá problemas com limites de conversa!** 🎯

---

**Arquivos Criados:**
- ✅ `PROMPT_TEMPLATE_FINAL.md` - Template pronto para uso
- ✅ `COMO_USAR_TEMPLATE.md` - Instruções de uso
- ✅ `TODOS.md` - Lista de tarefas organizadas
- ✅ `SOLUCOES.md` - Documentação de soluções
- ✅ `EXEMPLOS_PROMPTS.md` - Exemplos práticos

**Todos os arquivos estão prontos para uso!** 🚀 