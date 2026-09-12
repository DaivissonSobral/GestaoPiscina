using System.Net.Http.Json;
using GestaoPiscina.Client.Models;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;

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

        // Métodos auxiliares para tratamento de erros.
        // Nunca repassa o corpo bruto da resposta ao usuário: se o servidor devolver algo
        // que não seja o formato esperado (ex.: uma página de exceção com stack trace),
        // cai numa mensagem genérica em vez de vazar detalhes internos.
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
                        if (errorResponse != null)
                        {
                            if (errorResponse.TryGetValue("message", out var mensagem) && mensagem is not null)
                            {
                                return mensagem.ToString() ?? "Erro desconhecido";
                            }

                            if (errorResponse.TryGetValue("errors", out var errosObj)
                                && errosObj is JsonElement errosElement
                                && errosElement.ValueKind == JsonValueKind.Object)
                            {
                                var primeiraMensagem = errosElement.EnumerateObject()
                                    .SelectMany(campo => campo.Value.EnumerateArray())
                                    .Select(valor => valor.GetString())
                                    .FirstOrDefault(m => !string.IsNullOrEmpty(m));

                                if (!string.IsNullOrEmpty(primeiraMensagem))
                                {
                                    return primeiraMensagem!;
                                }
                            }

                            if (errorResponse.TryGetValue("title", out var titulo) && titulo is not null)
                            {
                                return titulo.ToString() ?? "Erro desconhecido";
                            }
                        }
                    }
                    catch
                    {
                        // Conteúdo não é um JSON no formato esperado — ignora e cai no fallback genérico abaixo.
                    }
                }
            }
            catch
            {
                // Ignora erros de leitura do conteúdo
            }

            return $"Erro ao processar a solicitação (HTTP {(int)response.StatusCode}).";
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

        // Uploads
        private class UploadFotoResponse
        {
            public string Url { get; set; } = string.Empty;
        }

        public async Task<string> UploadFotoAsync(IBrowserFile arquivo)
        {
            try
            {
                const long tamanhoMaximo = 10 * 1024 * 1024; // 10MB, deve bater com o limite do UploadsController

                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(arquivo.OpenReadStream(tamanhoMaximo));
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(arquivo.ContentType);
                content.Add(streamContent, "arquivo", arquivo.Name);

                var response = await _httpClient.PostAsync($"{_baseUrl}uploads/foto", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }

                var resultado = await response.Content.ReadFromJsonAsync<UploadFotoResponse>();
                if (string.IsNullOrEmpty(resultado?.Url))
                {
                    throw new Exception("Resposta inválida do servidor ao enviar a foto.");
                }

                return resultado.Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar foto: {ex.Message}");
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

        // Itens de Checklist
        public async Task<List<ChecklistItemDefinicao>> GetChecklistItensAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ChecklistItemDefinicao>>($"{_baseUrl}checklistitens") ?? new List<ChecklistItemDefinicao>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar itens de checklist: {ex.Message}");
            }
        }

        public async Task<ChecklistItemDefinicao> CreateChecklistItemAsync(ChecklistItemDefinicao item)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}checklistitens", item);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }

                var criado = await response.Content.ReadFromJsonAsync<ChecklistItemDefinicao>();
                return criado ?? item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar item de checklist: {ex.Message}");
            }
        }

        public async Task UpdateChecklistItemAsync(ChecklistItemDefinicao item)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}checklistitens/{item.IDChecklistItem}", item);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar item de checklist: {ex.Message}");
            }
        }

        public async Task DeleteChecklistItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}checklistitens/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir item de checklist: {ex.Message}");
            }
        }

        // Estoque
        public async Task<List<EstoqueCliente>> GetEstoqueAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<EstoqueCliente>>($"{_baseUrl}estoque") ?? new List<EstoqueCliente>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar estoque: {ex.Message}");
            }
        }

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

        // Movimentações de estoque (Entrada, Ajuste, Inventário — Saída é automática via dosagem na OS)
        public async Task<List<MovimentacaoEstoque>> GetMovimentacoesEstoqueAsync(int clienteId, int produtoId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MovimentacaoEstoque>>($"{_baseUrl}movimentacoesestoque/cliente/{clienteId}/produto/{produtoId}") ?? new List<MovimentacaoEstoque>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar histórico de movimentações: {ex.Message}");
            }
        }

        public async Task<MovimentacaoEstoque> CreateMovimentacaoEstoqueAsync(MovimentacaoEstoque movimentacao)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}movimentacoesestoque", movimentacao);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }

                var criada = await response.Content.ReadFromJsonAsync<MovimentacaoEstoque>();
                return criada ?? movimentacao;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao registrar movimentação de estoque: {ex.Message}");
            }
        }

        public async Task DeleteMovimentacaoEstoqueAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}movimentacoesestoque/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir movimentação de estoque: {ex.Message}");
            }
        }

        // Dosagens de produtos (aplicadas numa OS)
        public async Task<List<DosagemProduto>> GetDosagensPorOSAsync(int osId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<DosagemProduto>>($"{_baseUrl}dosagens/os/{osId}") ?? new List<DosagemProduto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar dosagens da OS: {ex.Message}");
            }
        }

        public async Task<DosagemProduto> CreateDosagemAsync(DosagemProduto dosagem)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}dosagens", dosagem);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }

                var criada = await response.Content.ReadFromJsonAsync<DosagemProduto>();
                return criada ?? dosagem;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao registrar dosagem: {ex.Message}");
            }
        }

        public async Task UpdateDosagemAsync(DosagemProduto dosagem)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}dosagens/{dosagem.IDDosagem}", dosagem);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar dosagem: {ex.Message}");
            }
        }

        public async Task DeleteDosagemAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}dosagens/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir dosagem: {ex.Message}");
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

        public async Task<List<OrdemDeServico>> GerarOSAutomaticasAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}ordensdeservico/gerar-automaticas", new { DataInicio = dataInicio, DataFim = dataFim });

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

        // Gestores
        public async Task<List<Gestor>> GetGestoresAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Gestor>>($"{_baseUrl}gestores") ?? new List<Gestor>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar gestores: {ex.Message}");
            }
        }

        public async Task<Gestor?> GetGestorAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Gestor>($"{_baseUrl}gestores/{id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar gestor: {ex.Message}");
            }
        }

        public async Task<Gestor> CreateGestorAsync(Gestor gestor)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}gestores", gestor);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }

                var createdGestor = await response.Content.ReadFromJsonAsync<Gestor>();
                return createdGestor ?? gestor;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar gestor: {ex.Message}");
            }
        }

        public async Task UpdateGestorAsync(Gestor gestor)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}gestores/{gestor.IDGestor}", gestor);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar gestor: {ex.Message}");
            }
        }

        public async Task DeleteGestorAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}gestores/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir gestor: {ex.Message}");
            }
        }

        public async Task<List<Cliente>> GetClientesDoGestorAsync(int gestorId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Cliente>>($"{_baseUrl}gestores/{gestorId}/clientes") ?? new List<Cliente>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar clientes do gestor: {ex.Message}");
            }
        }

        public async Task VincularGestorClienteAsync(int gestorId, int clienteId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}gestores/{gestorId}/clientes/{clienteId}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao vincular cliente ao gestor: {ex.Message}");
            }
        }

        public async Task DesvincularGestorClienteAsync(int gestorId, int clienteId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}gestores/{gestorId}/clientes/{clienteId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao desvincular cliente do gestor: {ex.Message}");
            }
        }

        // Usuários (somente leitura - usado para seletores de técnico/aprovador)
        public async Task<List<Usuario>> GetUsuariosAsync(string? perfil = null)
        {
            try
            {
                var url = string.IsNullOrWhiteSpace(perfil)
                    ? $"{_baseUrl}usuarios"
                    : $"{_baseUrl}usuarios?perfil={Uri.EscapeDataString(perfil)}";
                return await _httpClient.GetFromJsonAsync<List<Usuario>>(url) ?? new List<Usuario>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar usuários: {ex.Message}");
            }
        }

        // Administração de usuários
        public async Task<List<UsuarioAdmin>> GetUsuariosAdminAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<UsuarioAdmin>>($"{_baseUrl}usuarios/admin") ?? new List<UsuarioAdmin>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar usuários: {ex.Message}");
            }
        }

        public async Task<UsuarioAdmin> CreateUsuarioAsync(UsuarioAdmin usuario)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}usuarios", usuario);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }

                var criado = await response.Content.ReadFromJsonAsync<UsuarioAdmin>();
                return criado ?? usuario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar usuário: {ex.Message}");
            }
        }

        public async Task UpdateUsuarioAsync(UsuarioAdmin usuario)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}usuarios/{usuario.IDUsuario}", usuario);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar usuário: {ex.Message}");
            }
        }

        public async Task ResetarSenhaUsuarioAsync(int idUsuario, string novaSenha)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}usuarios/{idUsuario}/resetar-senha", new { NovaSenha = novaSenha });

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await GetErrorMessageAsync(response);
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao resetar senha: {ex.Message}");
            }
        }

        public async Task<List<Perfil>> GetPerfisAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Perfil>>($"{_baseUrl}perfis") ?? new List<Perfil>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar perfis: {ex.Message}");
            }
        }
    }
} 