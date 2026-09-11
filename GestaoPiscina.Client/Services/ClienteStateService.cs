using GestaoPiscina.Client.Models;

namespace GestaoPiscina.Client.Services
{
    public class ClienteStateService
    {
        // Estado do cliente atual
        public Cliente CurrentCliente { get; set; } = new();
        
        // Dados relacionados
        public List<Piscina>? Piscinas { get; set; }
        public List<Equipamento>? Equipamentos { get; set; }
        public List<EstoqueCliente>? Estoques { get; set; }
        public List<Produto>? Produtos { get; set; }

        // Estado dos formulários
        public bool ShowEstoqueForm { get; set; }
        
        // Objetos atuais dos formulários
        public Piscina CurrentPiscina { get; set; } = new();
        public Equipamento CurrentEquipamento { get; set; } = new();
        public EstoqueCliente CurrentEstoque { get; set; } = new();
        public int SelectedProdutoId { get; set; }
        
        // Estado da visualização
        public string CurrentView { get; set; } = "lista";
        public string ViewMode { get; set; } = "lista";
        
        private string _activeTab = "dados";
        public string ActiveTab 
        { 
            get => _activeTab;
            set
            {
                if (_activeTab != value)
                {
                    _activeTab = value;
                    NotifyStateChanged();
                }
            }
        }
        
        // Filtros
        public string SearchTerm { get; set; } = "";
        public string SelectedType { get; set; } = "";
        
        // Eventos para notificar mudanças
        public event Action? StateChanged;
        
        public void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }
        
        // Métodos para limpar estado
        public void ClearState()
        {
            CurrentCliente = new Cliente();
            Piscinas = null;
            Equipamentos = null;
            Estoques = null;
            Produtos = null;
            ShowEstoqueForm = false;
            CurrentPiscina = new Piscina();
            CurrentEquipamento = new Equipamento();
            CurrentEstoque = new EstoqueCliente();
            SelectedProdutoId = 0;
            CurrentView = "lista";
            ViewMode = "lista";
            ActiveTab = "dados";
            SearchTerm = "";
            SelectedType = "";
            NotifyStateChanged();
        }
        
    }
}