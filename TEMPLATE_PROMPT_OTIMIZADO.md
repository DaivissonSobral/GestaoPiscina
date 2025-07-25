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
- **RF**: [Código do Requisito Funcional da especificação]
- **RN**: [Código da Regra de Negócio relacionada]
- **Stakeholder**: [Perfil: Gestor/Supervisor/Técnico/Cliente]

### **Descrição do Requisito**
[Descrição detalhada do requisito conforme especificação]

## 🎯 **PROBLEMA FOCADO**

### **Situação Atual**
[Descrição do que já existe no sistema]

### **Problema Específico**
[Descrição clara do problema a resolver]

### **Objetivo**
[O que precisa ser implementado/resolvido]

## 📁 **ARQUIVOS ENVOLVIDOS**

### **Frontend (Blazor WebAssembly)**
- **Páginas**: [Lista de arquivos .razor]
- **Componentes**: [Lista de arquivos .razor em Shared/]
- **Serviços**: [Lista de arquivos .cs em Services/]
- **Modelos**: [Lista de arquivos .cs em Models/]

### **Backend (ASP.NET Core 8)**
- **Controllers**: [Lista de arquivos .cs]
- **Models**: [Lista de arquivos .cs]
- **Data**: [Lista de arquivos .cs]
- **Services**: [Lista de arquivos .cs]

### **Banco de Dados**
- **Migrations**: [Lista de arquivos de migration]
- **Context**: [Arquivo do DbContext]

## ❓ **PERGUNTA DIRETA**

[Uma pergunta específica e direta sobre a implementação]

## 📊 **CRITÉRIOS DE ACEITAÇÃO**

### **Funcionalidade**
- [ ] [Critério específico 1]
- [ ] [Critério específico 2]
- [ ] [Critério específico 3]

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
- **Framework**: Tailwind CSS 4.0
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