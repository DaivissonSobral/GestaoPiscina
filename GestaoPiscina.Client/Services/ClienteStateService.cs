using GestaoPiscina.Client.Models;
using GestaoPiscina.Client.Models.DTOs;

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
        
        // Dados temporários
        public List<Piscina> PiscinasTemporarias { get; set; } = new();
        public List<Equipamento> EquipamentosTemporarios { get; set; } = new();
        public List<EstoqueCliente> EstoquesTemporarios { get; set; } = new();
        
        // Estado dos formulários
        public bool ShowPiscinaForm { get; set; }
        public bool ShowEquipamentoForm { get; set; }
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
            PiscinasTemporarias.Clear();
            EquipamentosTemporarios.Clear();
            EstoquesTemporarios.Clear();
            ShowPiscinaForm = false;
            ShowEquipamentoForm = false;
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
        
        // Métodos para piscinas
        public void AddPiscinaTemporaria(Piscina piscina)
        {
            piscina.IDPiscina = -(PiscinasTemporarias.Count + 1);
            piscina.IDCliente = 0;
            PiscinasTemporarias.Add(piscina);
            NotifyStateChanged();
        }
        
        public void RemovePiscinaTemporaria(Piscina piscina)
        {
            PiscinasTemporarias.Remove(piscina);
            NotifyStateChanged();
        }
        
        // Métodos para equipamentos
        public void AddEquipamentoTemporario(Equipamento equipamento)
        {
            equipamento.IDEquipamento = -(EquipamentosTemporarios.Count + 1);
            equipamento.IDCliente = 0;
            EquipamentosTemporarios.Add(equipamento);
            NotifyStateChanged();
        }
        
        public void RemoveEquipamentoTemporario(Equipamento equipamento)
        {
            EquipamentosTemporarios.Remove(equipamento);
            NotifyStateChanged();
        }
        
        // Métodos para estoque
        public void AddEstoqueTemporario(EstoqueCliente estoque)
        {
            estoque.IDEstoque = -(EstoquesTemporarios.Count + 1);
            estoque.IDCliente = 0;
            EstoquesTemporarios.Add(estoque);
            NotifyStateChanged();
        }
        
        public void RemoveEstoqueTemporario(EstoqueCliente estoque)
        {
            EstoquesTemporarios.Remove(estoque);
            NotifyStateChanged();
        }
        
        // Método para criar DTO completo
        public ClienteCompletoDTO CreateClienteCompletoDTO()
        {
            var clienteCompleto = new ClienteCompletoDTO
            {
                IDCliente = CurrentCliente.IDCliente,
                Nome = CurrentCliente.Nome,
                Tipo = CurrentCliente.Tipo,
                Telefone = CurrentCliente.Telefone,
                Email = CurrentCliente.Email,
                Endereco = CurrentCliente.Endereco,
                DiasDeVisita = CurrentCliente.DiasDeVisita,
                Observacoes = CurrentCliente.Observacoes,
                Piscinas = new List<PiscinaDTO>(),
                Equipamentos = new List<EquipamentoDTO>(),
                Estoques = new List<EstoqueClienteDTO>()
            };

            // Adicionar piscinas (incluindo temporárias)
            if (Piscinas != null)
            {
                foreach (var piscina in Piscinas)
                {
                    clienteCompleto.Piscinas.Add(new PiscinaDTO
                    {
                        IDPiscina = piscina.IDPiscina,
                        IDCliente = piscina.IDCliente,
                        Tipo = piscina.Tipo,
                        VolumeLitros = piscina.VolumeLitros,
                        Localizacao = piscina.Localizacao,
                        Coberta = piscina.Coberta,
                        Aquecida = piscina.Aquecida
                    });
                }
            }
            
            // Adicionar piscinas temporárias
            foreach (var piscina in PiscinasTemporarias)
            {
                clienteCompleto.Piscinas.Add(new PiscinaDTO
                {
                    IDPiscina = 0,
                    IDCliente = 0,
                    Tipo = piscina.Tipo,
                    VolumeLitros = piscina.VolumeLitros,
                    Localizacao = piscina.Localizacao,
                    Coberta = piscina.Coberta,
                    Aquecida = piscina.Aquecida
                });
            }

            // Adicionar equipamentos (incluindo temporários)
            if (Equipamentos != null)
            {
                foreach (var equipamento in Equipamentos)
                {
                    clienteCompleto.Equipamentos.Add(new EquipamentoDTO
                    {
                        IDEquipamento = equipamento.IDEquipamento,
                        IDCliente = equipamento.IDCliente,
                        Descricao = equipamento.Descricao,
                        NumeroSerie = equipamento.NumeroSerie,
                        UltimaCalibragem = equipamento.UltimaCalibragem,
                        Observacao = equipamento.Observacao
                    });
                }
            }
            
            // Adicionar equipamentos temporários
            foreach (var equipamento in EquipamentosTemporarios)
            {
                clienteCompleto.Equipamentos.Add(new EquipamentoDTO
                {
                    IDEquipamento = 0,
                    IDCliente = 0,
                    Descricao = equipamento.Descricao,
                    NumeroSerie = equipamento.NumeroSerie,
                    UltimaCalibragem = equipamento.UltimaCalibragem,
                    Observacao = equipamento.Observacao
                });
            }

            // Adicionar estoques (incluindo temporários)
            if (Estoques != null)
            {
                foreach (var estoque in Estoques)
                {
                    clienteCompleto.Estoques.Add(new EstoqueClienteDTO
                    {
                        IDEstoque = estoque.IDEstoque,
                        IDCliente = estoque.IDCliente,
                        IDProduto = estoque.IDProduto,
                        QuantidadeAtual = estoque.QuantidadeAtual,
                        QuantidadeMinima = estoque.QuantidadeMinima,
                        NomeProduto = estoque.Produto.Nome,
                        ConcentracaoProduto = estoque.Produto.Concentracao,
                        UnidadeProduto = estoque.Produto.Unidade
                    });
                }
            }
            
            // Adicionar estoques temporários
            foreach (var estoque in EstoquesTemporarios)
            {
                clienteCompleto.Estoques.Add(new EstoqueClienteDTO
                {
                    IDEstoque = 0,
                    IDCliente = 0,
                    IDProduto = 0,
                    QuantidadeAtual = estoque.QuantidadeAtual,
                    QuantidadeMinima = estoque.QuantidadeMinima,
                    NomeProduto = estoque.Produto.Nome.Trim(),
                    ConcentracaoProduto = estoque.Produto.Concentracao,
                    UnidadeProduto = estoque.Produto.Unidade.Trim()
                });
            }

            return clienteCompleto;
        }
        
        // Método para limpar dados temporários após salvamento
        public void ClearTemporaryData()
        {
            PiscinasTemporarias.Clear();
            EquipamentosTemporarios.Clear();
            EstoquesTemporarios.Clear();
            NotifyStateChanged();
        }
    }
}