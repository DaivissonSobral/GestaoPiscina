using System.Net.Http.Json;
using GestaoPiscina.Client.Models;

namespace GestaoPiscina.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "api/";

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Clientes
        public async Task<List<Cliente>> GetClientesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Cliente>>($"{_baseUrl}clientes") ?? new List<Cliente>();
        }

        public async Task<Cliente?> GetClienteAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Cliente>($"{_baseUrl}clientes/{id}");
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}clientes", cliente);
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new Exception(error != null && error.ContainsKey("message") ? error["message"] : "Já existe um cliente com este nome.");
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Cliente>() ?? cliente;
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}clientes/{cliente.IDCliente}", cliente);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteClienteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}clientes/{id}");
            response.EnsureSuccessStatusCode();
        }

        // Piscinas
        public async Task<List<Piscina>> GetPiscinasAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Piscina>>($"{_baseUrl}piscinas") ?? new List<Piscina>();
        }

        public async Task<List<Piscina>> GetPiscinasByClienteAsync(int clienteId)
        {
            return await _httpClient.GetFromJsonAsync<List<Piscina>>($"{_baseUrl}piscinas/cliente/{clienteId}") ?? new List<Piscina>();
        }

        public async Task<Piscina> CreatePiscinaAsync(Piscina piscina)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}piscinas", piscina);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Piscina>() ?? piscina;
        }

        // Produtos
        public async Task<List<Produto>> GetProdutosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Produto>>($"{_baseUrl}produtos") ?? new List<Produto>();
        }

        public async Task<Produto> CreateProdutoAsync(Produto produto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}produtos", produto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Produto>() ?? produto;
        }

        // Ordens de Serviço
        public async Task<List<OrdemDeServico>> GetOrdensDeServicoAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<OrdemDeServico>>($"{_baseUrl}ordensdeservico") ?? new List<OrdemDeServico>();
        }

        public async Task<List<OrdemDeServico>> GetOrdensDeHojeAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<OrdemDeServico>>($"{_baseUrl}ordensdeservico/hoje") ?? new List<OrdemDeServico>();
        }

        public async Task<OrdemDeServico> CreateOrdemDeServicoAsync(OrdemDeServico ordemDeServico)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}ordensdeservico", ordemDeServico);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrdemDeServico>() ?? ordemDeServico;
        }

        // Equipamentos
        public async Task<List<Equipamento>> GetEquipamentosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Equipamento>>($"{_baseUrl}equipamentos") ?? new List<Equipamento>();
        }

        public async Task<List<Equipamento>> GetEquipamentosByClienteAsync(int clienteId)
        {
            return await _httpClient.GetFromJsonAsync<List<Equipamento>>($"{_baseUrl}equipamentos/cliente/{clienteId}") ?? new List<Equipamento>();
        }

        public async Task<Equipamento> CreateEquipamentoAsync(Equipamento equipamento)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}equipamentos", equipamento);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Equipamento>() ?? equipamento;
        }

        public async Task UpdateEquipamentoAsync(Equipamento equipamento)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}equipamentos/{equipamento.IDEquipamento}", equipamento);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteEquipamentoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}equipamentos/{id}");
            response.EnsureSuccessStatusCode();
        }

        // Métodos adicionais para Piscinas
        public async Task UpdatePiscinaAsync(Piscina piscina)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}piscinas/{piscina.IDPiscina}", piscina);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeletePiscinaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}piscinas/{id}");
            response.EnsureSuccessStatusCode();
        }

        // Métodos adicionais para Produtos
        public async Task UpdateProdutoAsync(Produto produto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}produtos/{produto.IDProduto}", produto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProdutoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}produtos/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
} 