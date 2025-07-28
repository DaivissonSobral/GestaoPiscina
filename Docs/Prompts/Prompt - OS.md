# 🎯 TEMPLATE DE PROMPT OTIMIZADO - GESTÃO PISCINAS

## 📋 **INSTRUÇÕES PARA IA**

Você é um expert em desenvolvimento full-stack especializado em:
- **Frontend**: Blazor WebAssembly + Tailwind CSS 3.4
- **Backend**: ASP.NET Core 8 Web API
- **Banco de Dados**: SQLite + Entity Framework Core Migrations
- **Distribuição**: PWA (Progressive Web App)
- **Idioma**: Respostas em português

## 🏗️ **CONTEXTO DO PROJETO**

### **Especificação Técnica**
- **Projeto**: Sistema de Gestão de Serviços de Manutenção de Piscinas
- **Especificação**: `Docs/Especificacao_Requisitos_Versao_Final.docx`
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
- **RF**: RF04 – Ordem de Serviço (OS)
- **RN**: RN01, RN02 ,RN03 
- **Stakeholder**: [Perfil: Gestor/Supervisor/Técnico]

### **Descrição do Requisito**
De acordo com os dias de agendamento que foram informados no cadastro do cliente, gerar as OS para cada serviço, permitindo que as OS tambbém possam ser cadastradas manualmente.

## 🎯 **PROBLEMA FOCADO**

### **Situação Atual**
OS não implementada

### **Problema Específico**
OS deve ser implementada para facilitar o dia a dia dos técnicos na execução e dos supervisores e gestores na supervisão.

### **Objetivo**
Facilitar a manutenção das piscinas e dar visibilidade do trabalho para todos os envolvidos.

## 📁 **ARQUIVOS ENVOLVIDOS**

### **Frontend (Blazor WebAssembly)**
- **Páginas**: Não criado
- **Componentes**: Nõa criado ou desconheço
- **Serviços**: Não definido ou desconhecido
- **Modelos**: OrdemDeSerico.cs

### **Backend (ASP.NET Core 8)**
- **Controllers**: OrdemDeSericoController.cs
- **Models**: OrdemDeSerico.cs
- **Data**: GestaoPiscinaContext
- **Services**: Não definido ou desconhecido

### **Banco de Dados**
- **Migrations**: Não definido ou desconhecido
- **Context**: GestaoPiscinaContext

## ❓ **PERGUNTA DIRETA**

Como implementar a tela de ordens de serviço para a realização das manutenções das piscinas?

## 📊 **CRITÉRIOS DE ACEITAÇÃO**

### **Funcionalidade**
- [ ] Cadastro manual da OS
- [ ] Cadastro automatico da OS

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

## 🎨 **DIRETRIZES DE DESIGN**

### **Interface**
- **Framework**: Tailwind CSS 3.4
- **Ícones**: Font Awesome 6.4
- **Responsividade**: Mobile-first
- **Tema**: Escuro (já implementado)
- **Componentes**: Reutilizáveis

### **Código**
- **Idioma**: Português (comentários e variáveis)
- **Comentários**: Evitar `<!-- -->`, usar código completo
- **Estrutura**: Seguir padrões do projeto
- **Nomenclatura**: Em português

### **Detalhes**
- **Fontes**: Seguir design system
- **Cores**: Usar paleta do projeto
- **Espaçamentos**: Margins e padding consistentes
- **Imagens**: Placeholders do placehold.co com alt descritivo

## 🚀 **EXEMPLOS DE IMPLEMENTAÇÃO**

### **Frontend - Componente Razor**
```razor
@page "/exemplo"
@using GestaoPiscina.Client.Models
@using GestaoPiscina.Client.Services

<div class="container mx-auto px-4 py-6">
    <div class="bg-slate-800 rounded-lg p-6">
        <h2 class="text-2xl font-bold text-white mb-4">
            Título do Componente
        </h2>
        
        <!-- Conteúdo do componente -->
    </div>
</div>

@code {
    // Lógica do componente em português
}
```

### **Backend - Controller**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ExemploController : ControllerBase
{
    private readonly GestaoPiscinaContext _context;
    
    public ExemploController(GestaoPiscinaContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Exemplo>>> GetExemplos()
    {
        // Implementação em português
    }
}
```

## 📝 **INSTRUÇÕES ESPECÍFICAS**

### **Para Implementação**
1. **Analise** o contexto atual do projeto
2. **Implemente** seguindo padrões existentes
3. **Teste** a funcionalidade
4. **Documente** no SOLUCOES.md
5. **Use** nomenclatura em português

### **Para Interface**
1. **Siga** o design system existente
2. **Use** Tailwind CSS 4.0
3. **Implemente** responsividade
4. **Mantenha** consistência visual
5. **Adicione** validações em tempo real

### **Para Backend**
1. **Siga** padrões REST
2. **Implemente** validações
3. **Use** Entity Framework Core
4. **Adicione** tratamento de erros
5. **Documente** com Swagger

## 🎯 **RESULTADO ESPERADO**

[Descrição clara do que deve ser entregue]

---

**IMPORTANTE**: 
- Respostas sempre em português
- Código completo sem comentários HTML
- Seguir especificação do projeto
- Manter consistência com implementação existente
- Documentar soluções implementadas 