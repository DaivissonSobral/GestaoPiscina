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
    }
} 