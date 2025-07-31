using System.Net.Http.Json;
using GestaoPiscina.Client.Models;
using GestaoPiscina.Client.Models.DTOs;
using System.Text.Json;
using Microsoft.JSInterop;

namespace GestaoPiscina.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly string _baseUrl = "api/";

        public ApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        // Métodos auxiliares para tratamento de erros
        private async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                        if (errorResponse != null && errorResponse.ContainsKey("message"))
                        {
                            return errorResponse["message"].ToString() ?? "Erro desconhecido";
                        }
                    }
                    catch
                    {
                        // Se não conseguir deserializar como JSON, retorna o conteúdo como string
                        return content;
                    }
                }
            }
            catch
            {
                // Ignora erros de leitura do conteúdo
            }
            
            return response.ReasonPhrase ?? "Erro desconhecido";
        }

        // Método para adicionar token de autenticação
        private async Task AddAuthHeaderAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // Ignorar erros de autenticação
            }
        }

        // Clientes
        public async Task<List<Cliente>> GetClientesAsync()
        {
            try
            {
                await AddAuthHeaderAsync();
                var response = await _httpClient.GetAsync($"{_baseUrl}clientes");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta da API: {content}");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    };
                    
                    return JsonSerializer.Deserialize<List<Cliente>>(content, options) ?? new List<Cliente>();
                }
                else
                {
                    throw new Exception($"Erro HTTP: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar clientes: {ex.Message}");
            }
        }

        public async Task<Cliente?> GetClienteAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Cliente>($"{_baseUrl}clientes/{id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar cliente: {ex.Message}");
            }
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}clientes", cliente);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdCliente = await response.Content.ReadFromJsonAsync<Cliente>();
                return createdCliente ?? cliente;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar cliente: {ex.Message}");
            }
        }

        public async Task<Cliente> CreateClienteCompletoAsync(ClienteCompletoDTO clienteCompleto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}clientes/completo", clienteCompleto);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdCliente = await response.Content.ReadFromJsonAsync<Cliente>();
                return createdCliente ?? new Cliente();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar cliente completo: {ex.Message}");
            }
        }

        public async Task<Cliente> UpdateClienteCompletoAsync(ClienteCompletoDTO clienteCompleto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}clientes/completo/{clienteCompleto.IDCliente}", clienteCompleto);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var updatedCliente = await response.Content.ReadFromJsonAsync<Cliente>();
                return updatedCliente ?? new Cliente();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar cliente completo: {ex.Message}");
            }
        }

        public async Task UpdateClienteAsync(Cliente cliente)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}clientes/{cliente.IDCliente}", cliente);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar cliente: {ex.Message}");
            }
        }

        public async Task DeleteClienteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}clientes/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir cliente: {ex.Message}");
            }
        }

        // Piscinas
        public async Task<List<Piscina>> GetPiscinasAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Piscina>>($"{_baseUrl}piscinas") ?? new List<Piscina>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar piscinas: {ex.Message}");
            }
        }

        public async Task<List<Piscina>> GetPiscinasByClienteAsync(int clienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Piscina>>($"{_baseUrl}piscinas/cliente/{clienteId}") ?? new List<Piscina>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar piscinas do cliente: {ex.Message}");
            }
        }

        public async Task<Piscina> CreatePiscinaAsync(Piscina piscina)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}piscinas", piscina);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdPiscina = await response.Content.ReadFromJsonAsync<Piscina>();
                return createdPiscina ?? piscina;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar piscina: {ex.Message}");
            }
        }

        public async Task UpdatePiscinaAsync(Piscina piscina)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}piscinas/{piscina.IDPiscina}", piscina);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar piscina: {ex.Message}");
            }
        }

        public async Task DeletePiscinaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}piscinas/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir piscina: {ex.Message}");
            }
        }

        // Equipamentos
        public async Task<List<Equipamento>> GetEquipamentosAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Equipamento>>($"{_baseUrl}equipamentos") ?? new List<Equipamento>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar equipamentos: {ex.Message}");
            }
        }

        public async Task<List<Equipamento>> GetEquipamentosByClienteAsync(int clienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Equipamento>>($"{_baseUrl}equipamentos/cliente/{clienteId}") ?? new List<Equipamento>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar equipamentos do cliente: {ex.Message}");
            }
        }

        public async Task<Equipamento> CreateEquipamentoAsync(Equipamento equipamento)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}equipamentos", equipamento);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdEquipamento = await response.Content.ReadFromJsonAsync<Equipamento>();
                return createdEquipamento ?? equipamento;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar equipamento: {ex.Message}");
            }
        }

        public async Task UpdateEquipamentoAsync(Equipamento equipamento)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}equipamentos/{equipamento.IDEquipamento}", equipamento);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar equipamento: {ex.Message}");
            }
        }

        public async Task DeleteEquipamentoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}equipamentos/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir equipamento: {ex.Message}");
            }
        }

        // Produtos
        public async Task<List<Produto>> GetProdutosAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Produto>>($"{_baseUrl}produtos") ?? new List<Produto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar produtos: {ex.Message}");
            }
        }

        public async Task<List<Produto>> GetProdutosByClienteAsync(int clienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Produto>>($"{_baseUrl}produtos/cliente/{clienteId}") ?? new List<Produto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar produtos do cliente: {ex.Message}");
            }
        }

        public async Task<Produto> CreateProdutoAsync(Produto produto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}produtos", produto);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdProduto = await response.Content.ReadFromJsonAsync<Produto>();
                return createdProduto ?? produto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar produto: {ex.Message}");
            }
        }

        public async Task UpdateProdutoAsync(Produto produto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}produtos/{produto.IDProduto}", produto);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar produto: {ex.Message}");
            }
        }

        public async Task DeleteProdutoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}produtos/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir produto: {ex.Message}");
            }
        }

        // Estoque
        public async Task<List<EstoqueCliente>> GetEstoqueByClienteAsync(int clienteId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<EstoqueCliente>>($"{_baseUrl}estoque/cliente/{clienteId}") ?? new List<EstoqueCliente>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar estoque do cliente: {ex.Message}");
            }
        }

        public async Task<EstoqueCliente> CreateEstoqueAsync(EstoqueCliente estoque)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}estoque", estoque);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdEstoque = await response.Content.ReadFromJsonAsync<EstoqueCliente>();
                return createdEstoque ?? estoque;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar item do estoque: {ex.Message}");
            }
        }

        public async Task UpdateEstoqueAsync(EstoqueCliente estoque)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}estoque/{estoque.IDEstoque}", estoque);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar item do estoque: {ex.Message}");
            }
        }

        public async Task DeleteEstoqueAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}estoque/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir item do estoque: {ex.Message}");
            }
        }

        // Ordens de Serviço
        public async Task<List<OrdemDeServico>> GetOrdensDeServicoAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<OrdemDeServico>>($"{_baseUrl}ordensdeservico") ?? new List<OrdemDeServico>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar ordens de serviço: {ex.Message}");
            }
        }

        public async Task<List<OrdemDeServico>> GetOrdensDeHojeAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<OrdemDeServico>>($"{_baseUrl}ordensdeservico/hoje") ?? new List<OrdemDeServico>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar ordens de hoje: {ex.Message}");
            }
        }

        public async Task<OrdemDeServico> CreateOrdemDeServicoAsync(OrdemDeServico ordemDeServico)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}ordensdeservico", ordemDeServico);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var createdOrdem = await response.Content.ReadFromJsonAsync<OrdemDeServico>();
                return createdOrdem ?? ordemDeServico;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar ordem de serviço: {ex.Message}");
            }
        }

        public async Task UpdateOrdemDeServicoAsync(OrdemDeServico ordemDeServico)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}ordensdeservico/{ordemDeServico.IDOS}", ordemDeServico);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar ordem de serviço: {ex.Message}");
            }
        }

        public async Task DeleteOrdemDeServicoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}ordensdeservico/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir ordem de serviço: {ex.Message}");
            }
        }

        public async Task<OrdemDeServico?> GetOrdemDeServicoAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<OrdemDeServico>($"{_baseUrl}ordensdeservico/{id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar ordem de serviço: {ex.Message}");
            }
        }

        public async Task<List<OrdemDeServico>> GetOrdensByPiscinaAsync(int piscinaId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<OrdemDeServico>>($"{_baseUrl}ordensdeservico/piscina/{piscinaId}") ?? new List<OrdemDeServico>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar ordens por piscina: {ex.Message}");
            }
        }

        public async Task<List<OrdemDeServico>> GerarOSAutomaticasAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}ordensdeservico/gerar-automaticas", null);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
                
                var osCriadas = await response.Content.ReadFromJsonAsync<List<OrdemDeServico>>();
                return osCriadas ?? new List<OrdemDeServico>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao gerar OS automáticas: {ex.Message}");
            }
        }
    }
} 