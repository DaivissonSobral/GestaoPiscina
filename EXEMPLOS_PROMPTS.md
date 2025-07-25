# 📝 EXEMPLOS DE PROMPTS OTIMIZADOS

## 🎯 **Como Usar o Template**

1. **Copie** o template de `TEMPLATE_PROMPT_OTIMIZADO.md`
2. **Preencha** as seções com suas informações específicas
3. **Cole** em uma nova conversa
4. **Implemente** a solução
5. **Documente** no `SOLUCOES.md`

---

## 📋 **EXEMPLO 1: Autenticação JWT**

```markdown
# 🎯 TEMPLATE DE PROMPT OTIMIZADO - GESTÃO PISCINAS

## 📋 **INSTRUÇÕES PARA IA**

Você é um expert em desenvolvimento full-stack especializado em:
- **Frontend**: Blazor WebAssembly + Tailwind CSS 4.0
- **Backend**: ASP.NET Core 8 Web API
- **Banco de Dados**: SQLite + Entity Framework Core Migrations
- **Distribuição**: PWA (Progressive Web App)
- **Idioma**: Respostas em português

## 🏗️ **CONTEXTO DO PROJETO**

### **Especificação Técnica**
- **Projeto**: Sistema de Gestão de Serviços de Manutenção de Piscinas
- **Especificação**: `Docs/Especificacao_Requisitos_Versao_Final.txt`
- **Estrutura**: `Docs/Estrutura.txt`
- **Template Visual**: `Docs/template.html`
- **Imagens de Referência**: `Docs/imagens referência/`

### **Status Atual**
- ✅ **CRUD Básico**: Implementado para todas as entidades
- ✅ **Interface Responsiva**: Mobile, tablet e desktop
- ✅ **API REST**: Endpoints funcionais
- ✅ **PWA**: Configurado e funcionando
- 🔄 **Funcionalidades Avançadas**: Em desenvolvimento

## 📋 **REQUISITO ESPECÍFICO**

### **Código do Requisito**
- **RF**: RF12 - Perfis de Acesso
- **RN**: RN09 - Perfis e Restrições de Acesso
- **Stakeholder**: Todos os perfis (Gestor/Supervisor/Técnico/Cliente)

### **Descrição do Requisito**
Sistema deve implementar autenticação com 4 perfis de acesso distintos, cada um com permissões específicas conforme especificação.

## 🎯 **PROBLEMA FOCADO**

### **Situação Atual**
Sistema não possui autenticação, permitindo acesso livre a todas as funcionalidades

### **Problema Específico**
Necessário implementar sistema de login com JWT e proteção de rotas por perfil de usuário

### **Objetivo**
Implementar autenticação JWT com 4 perfis de acesso e proteção de rotas no Blazor WebAssembly

## 📁 **ARQUIVOS ENVOLVIDOS**

### **Frontend (Blazor WebAssembly)**
- **Páginas**: Pages/Login.razor, Pages/Home.razor
- **Componentes**: Shared/MainLayout.razor, Shared/NavMenu.razor
- **Serviços**: Services/AuthService.cs, Services/ApiService.cs
- **Modelos**: Models/Usuario.cs, Models/Perfil.cs

### **Backend (ASP.NET Core 8)**
- **Controllers**: Controllers/AuthController.cs
- **Models**: Models/Usuario.cs, Models/Perfil.cs
- **Data**: Data/GestaoPiscinaContext.cs
- **Services**: Services/JwtService.cs

### **Banco de Dados**
- **Migrations**: Nova migration para Usuario e Perfil
- **Context**: Data/GestaoPiscinaContext.cs

## ❓ **PERGUNTA DIRETA**

Como implementar autenticação JWT com 4 perfis de acesso (Gestor/Supervisor/Técnico/Cliente) e proteção de rotas no Blazor WebAssembly?

## 📊 **CRITÉRIOS DE ACEITAÇÃO**

### **Funcionalidade**
- [ ] Login com usuário/senha
- [ ] Tokens JWT válidos
- [ ] Proteção de rotas por perfil
- [ ] Logout funcional
- [ ] Recuperação de senha

### **Interface**
- [ ] Responsiva (mobile, tablet, desktop)
- [ ] Seguindo design system do projeto
- [ ] Componentes reutilizáveis
- [ ] Validações em tempo real

### **Backend**
- [ ] API REST implementada
- [ ] Validações de negócio
- [ ] Relacionamentos EF Core
- [ ] Tratamento de erros

### **Qualidade**
- [ ] Código limpo e documentado
- [ ] Testado e funcionando
- [ ] Documentado no SOLUCOES.md
- [ ] Commit com mensagem clara

## 🎯 **RESULTADO ESPERADO**

Sistema de autenticação completo com login, logout, proteção de rotas e 4 perfis de acesso funcionais.
```

---

## 📋 **EXEMPLO 2: Checklist Obrigatório**

```markdown
# 🎯 TEMPLATE DE PROMPT OTIMIZADO - GESTÃO PISCINAS

## 📋 **INSTRUÇÕES PARA IA**

Você é um expert em desenvolvimento full-stack especializado em:
- **Frontend**: Blazor WebAssembly + Tailwind CSS 4.0
- **Backend**: ASP.NET Core 8 Web API
- **Banco de Dados**: SQLite + Entity Framework Core Migrations
- **Distribuição**: PWA (Progressive Web App)
- **Idioma**: Respostas em português

## 🏗️ **CONTEXTO DO PROJETO**

### **Especificação Técnica**
- **Projeto**: Sistema de Gestão de Serviços de Manutenção de Piscinas
- **Especificação**: `Docs/Especificacao_Requisitos_Versao_Final.txt`
- **Estrutura**: `Docs/Estrutura.txt`
- **Template Visual**: `Docs/template.html`
- **Imagens de Referência**: `Docs/imagens referência/`

### **Status Atual**
- ✅ **CRUD Básico**: Implementado para todas as entidades
- ✅ **Interface Responsiva**: Mobile, tablet e desktop
- ✅ **API REST**: Endpoints funcionais
- ✅ **PWA**: Configurado e funcionando
- 🔄 **Funcionalidades Avançadas**: Em desenvolvimento

## 📋 **REQUISITO ESPECÍFICO**

### **Código do Requisito**
- **RF**: RF04 - Ordem de Serviço (OS) com checklist obrigatório
- **RN**: RN01 - Finalização de Ordem de Serviço
- **RN**: RN02 - Checklist Operacional
- **Stakeholder**: Técnico (execução), Supervisor (avaliação)

### **Descrição do Requisito**
Ordens de serviço devem ter checklist obrigatório para garantir qualidade do serviço, com registro de fotos/vídeos e validação de itens obrigatórios.

## 🎯 **PROBLEMA FOCADO**

### **Situação Atual**
Ordens de serviço existem mas não possuem checklist obrigatório

### **Problema Específico**
Necessário implementar checklist interativo com validação de itens obrigatórios e upload de fotos/vídeos

### **Objetivo**
Implementar checklist obrigatório em Ordens de Serviço com validação e registro multimídia

## 📁 **ARQUIVOS ENVOLVIDOS**

### **Frontend (Blazor WebAssembly)**
- **Páginas**: Pages/OrdensServico.razor
- **Componentes**: Shared/ChecklistModal.razor, Shared/FileUpload.razor
- **Serviços**: Services/ChecklistService.cs, Services/FileService.cs
- **Modelos**: Models/Checklist.cs, Models/ItemChecklist.cs

### **Backend (ASP.NET Core 8)**
- **Controllers**: Controllers/OrdensDeServicoController.cs, Controllers/ChecklistController.cs
- **Models**: Models/Checklist.cs, Models/ItemChecklist.cs
- **Data**: Data/GestaoPiscinaContext.cs
- **Services**: Services/ChecklistService.cs

### **Banco de Dados**
- **Migrations**: Nova migration para Checklist e ItemChecklist
- **Context**: Data/GestaoPiscinaContext.cs

## ❓ **PERGUNTA DIRETA**

Como implementar checklist obrigatório interativo em Ordens de Serviço com validação de itens obrigatórios e registro de fotos/vídeos?

## 📊 **CRITÉRIOS DE ACEITAÇÃO**

### **Funcionalidade**
- [ ] Checklist obrigatório implementado
- [ ] Validação de itens obrigatórios
- [ ] Upload de fotos/vídeos
- [ ] Finalização apenas com checklist completo
- [ ] Histórico de checklists

### **Interface**
- [ ] Responsiva (mobile, tablet, desktop)
- [ ] Seguindo design system do projeto
- [ ] Componentes reutilizáveis
- [ ] Validações em tempo real

### **Backend**
- [ ] API REST implementada
- [ ] Validações de negócio
- [ ] Relacionamentos EF Core
- [ ] Tratamento de erros

### **Qualidade**
- [ ] Código limpo e documentado
- [ ] Testado e funcionando
- [ ] Documentado no SOLUCOES.md
- [ ] Commit com mensagem clara

## 🎯 **RESULTADO ESPERADO**

Sistema de checklist obrigatório funcional com validação, upload de arquivos e controle de finalização de OS.
```

---

## 📋 **EXEMPLO 3: Cálculo Automático de Dosagem**

```markdown
# 🎯 TEMPLATE DE PROMPT OTIMIZADO - GESTÃO PISCINAS

## 📋 **INSTRUÇÕES PARA IA**

Você é um expert em desenvolvimento full-stack especializado em:
- **Frontend**: Blazor WebAssembly + Tailwind CSS 4.0
- **Backend**: ASP.NET Core 8 Web API
- **Banco de Dados**: SQLite + Entity Framework Core Migrations
- **Distribuição**: PWA (Progressive Web App)
- **Idioma**: Respostas em português

## 🏗️ **CONTEXTO DO PROJETO**

### **Especificação Técnica**
- **Projeto**: Sistema de Gestão de Serviços de Manutenção de Piscinas
- **Especificação**: `Docs/Especificacao_Requisitos_Versao_Final.txt`
- **Estrutura**: `Docs/Estrutura.txt`
- **Template Visual**: `Docs/template.html`
- **Imagens de Referência**: `Docs/imagens referência/`

### **Status Atual**
- ✅ **CRUD Básico**: Implementado para todas as entidades
- ✅ **Interface Responsiva**: Mobile, tablet e desktop
- ✅ **API REST**: Endpoints funcionais
- ✅ **PWA**: Configurado e funcionando
- 🔄 **Funcionalidades Avançadas**: Em desenvolvimento

## 📋 **REQUISITO ESPECÍFICO**

### **Código do Requisito**
- **RF**: RF05 - Cálculo Automático de Produtos
- **RN**: RN03 - Cálculo de Produtos
- **Stakeholder**: Técnico (execução), Técnica Química (parâmetros)

### **Descrição do Requisito**
Sistema deve calcular automaticamente a dosagem de produtos químicos baseada no volume da piscina e parâmetros configuráveis.

## 🎯 **PROBLEMA FOCADO**

### **Situação Atual**
Produtos estão cadastrados mas não há cálculo automático de dosagem

### **Problema Específico**
Necessário implementar algoritmo de cálculo de dosagem baseado no volume da piscina

### **Objetivo**
Implementar cálculo automático de dosagem de produtos químicos

## 📁 **ARQUIVOS ENVOLVIDOS**

### **Frontend (Blazor WebAssembly)**
- **Páginas**: Pages/OrdensServico.razor, Pages/CalculoDosagem.razor
- **Componentes**: Shared/CalculoModal.razor
- **Serviços**: Services/CalculoService.cs
- **Modelos**: Models/CalculoDosagem.cs

### **Backend (ASP.NET Core 8)**
- **Controllers**: Controllers/CalculoController.cs
- **Models**: Models/CalculoDosagem.cs, Models/ParametroQuimico.cs
- **Data**: Data/GestaoPiscinaContext.cs
- **Services**: Services/DosagemService.cs

### **Banco de Dados**
- **Migrations**: Nova migration para ParametroQuimico
- **Context**: Data/GestaoPiscinaContext.cs

## ❓ **PERGUNTA DIRETA**

Como implementar cálculo automático de dosagem de produtos químicos baseado no volume da piscina e parâmetros configuráveis?

## 📊 **CRITÉRIOS DE ACEITAÇÃO**

### **Funcionalidade**
- [ ] Cálculo automático implementado
- [ ] Parâmetros configuráveis
- [ ] Validação de dosagens
- [ ] Histórico de cálculos
- [ ] Interface intuitiva

### **Interface**
- [ ] Responsiva (mobile, tablet, desktop)
- [ ] Seguindo design system do projeto
- [ ] Componentes reutilizáveis
- [ ] Validações em tempo real

### **Backend**
- [ ] API REST implementada
- [ ] Validações de negócio
- [ ] Relacionamentos EF Core
- [ ] Tratamento de erros

### **Qualidade**
- [ ] Código limpo e documentado
- [ ] Testado e funcionando
- [ ] Documentado no SOLUCOES.md
- [ ] Commit com mensagem clara

## 🎯 **RESULTADO ESPERADO**

Sistema de cálculo automático de dosagem funcional com parâmetros configuráveis e validação de resultados.
```

---

## 🚀 **BENEFÍCIOS DO TEMPLATE**

### ✅ **Vantagens:**
- **Contexto completo** - Inclui especificação técnica
- **Foco específico** - Um problema por prompt
- **Arquivos claros** - Lista exata dos arquivos envolvidos
- **Critérios definidos** - Aceitação clara
- **Rastreabilidade** - Vinculado aos RFs e RNs

### 📊 **Eficiência:**
- **Conversas curtas** - Nunca atinge limite
- **Soluções rápidas** - Foco em um problema
- **Documentação organizada** - Progresso visível
- **Reutilização** - Soluções documentadas

### 🎯 **Como Usar:**
1. **Escolha um requisito** da especificação
2. **Copie o template** correspondente
3. **Adapte** para seu problema específico
4. **Use em nova conversa** focada
5. **Documente** a solução implementada

**Com este template, você nunca mais terá problemas com limites de conversa!** 🎯 