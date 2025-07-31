using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Data;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Models.DTOs;

namespace GestaoPiscina.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly GestaoPiscinaContext _context;

        public ClientesController(GestaoPiscinaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes
                .Include(c => c.Piscinas)
                .Include(c => c.Estoques)
                .Include(c => c.Equipamentos)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Piscinas)
                .Include(c => c.Estoques)
                .Include(c => c.Equipamentos)
                .FirstOrDefaultAsync(c => c.IDCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente;
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            // Verifica se já existe cliente com o mesmo nome (ignorando maiúsculas/minúsculas)
            if (await _context.Clientes.AnyAsync(c => c.Nome.ToLower() == cliente.Nome.ToLower()))
            {
                return Conflict(new { message = "Já existe um cliente com este nome." });
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.IDCliente }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.IDCliente)
            {
                return BadRequest();
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IDCliente == id);
        }

        [HttpPost("completo")]
        public async Task<ActionResult<Cliente>> PostClienteCompleto(ClienteCompletoDTO clienteDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Validações iniciais
                if (clienteDTO == null)
                {
                    return BadRequest(new { message = "Dados do cliente não fornecidos." });
                }

                // Verifica se já existe cliente com o mesmo nome (ignorando maiúsculas/minúsculas)
                if (await _context.Clientes.AnyAsync(c => c.Nome.ToLower() == clienteDTO.Nome.ToLower()))
                {
                    return Conflict(new { message = "Já existe um cliente com este nome." });
                }

                // 1. Criar/Atualizar o Cliente
                Cliente cliente;
                if (clienteDTO.IDCliente > 0)
                {
                    // Atualizar cliente existente
                    var clienteExistente = await _context.Clientes.FindAsync(clienteDTO.IDCliente);
                    if (clienteExistente == null)
                    {
                        return NotFound(new { message = "Cliente não encontrado." });
                    }
                    cliente = clienteExistente;
                    
                    cliente.Nome = clienteDTO.Nome;
                    cliente.Tipo = clienteDTO.Tipo;
                    cliente.Telefone = clienteDTO.Telefone;
                    cliente.Email = clienteDTO.Email;
                    cliente.Endereco = clienteDTO.Endereco;
                    cliente.DiasDeVisita = clienteDTO.DiasDeVisita;
                    cliente.Observacoes = clienteDTO.Observacoes;
                    
                    _context.Entry(cliente).State = EntityState.Modified;
                }
                else
                {
                    // Criar novo cliente
                    cliente = new Cliente
                    {
                        Nome = clienteDTO.Nome,
                        Tipo = clienteDTO.Tipo,
                        Telefone = clienteDTO.Telefone,
                        Email = clienteDTO.Email,
                        Endereco = clienteDTO.Endereco,
                        DiasDeVisita = clienteDTO.DiasDeVisita,
                        Observacoes = clienteDTO.Observacoes
                    };
                    
                    _context.Clientes.Add(cliente);
                }

                await _context.SaveChangesAsync();

                // Verificar se o cliente foi salvo corretamente
                if (cliente.IDCliente <= 0)
                {
                    return BadRequest(new { message = "Erro ao salvar cliente após commit." });
                }

                // 2. Processar Piscinas
                foreach (var piscinaDTO in clienteDTO.Piscinas)
                {
                    if (piscinaDTO.IDPiscina > 0)
                    {
                        // Atualizar piscina existente
                        var piscina = await _context.Piscinas.FindAsync(piscinaDTO.IDPiscina);
                        if (piscina != null)
                        {
                            piscina.Tipo = piscinaDTO.Tipo;
                            piscina.VolumeLitros = piscinaDTO.VolumeLitros;
                            piscina.Localizacao = piscinaDTO.Localizacao;
                            _context.Entry(piscina).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Criar nova piscina
                        var piscina = new Piscina
                        {
                            IDCliente = cliente.IDCliente,
                            Tipo = piscinaDTO.Tipo,
                            VolumeLitros = piscinaDTO.VolumeLitros,
                            Localizacao = piscinaDTO.Localizacao
                        };
                        _context.Piscinas.Add(piscina);
                    }
                }

                // 3. Processar Equipamentos
                foreach (var equipamentoDTO in clienteDTO.Equipamentos)
                {
                    if (equipamentoDTO.IDEquipamento > 0)
                    {
                        // Atualizar equipamento existente
                        var equipamento = await _context.Equipamentos.FindAsync(equipamentoDTO.IDEquipamento);
                        if (equipamento != null)
                        {
                            equipamento.Tipo = equipamentoDTO.Tipo;
                            equipamento.NumeroSerie = equipamentoDTO.NumeroSerie;
                            equipamento.UltimaCalibragem = equipamentoDTO.UltimaCalibragem;
                            equipamento.Observacao = equipamentoDTO.Observacao;
                            _context.Entry(equipamento).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Criar novo equipamento
                        var equipamento = new Equipamento
                        {
                            IDCliente = cliente.IDCliente,
                            Tipo = equipamentoDTO.Tipo,
                            NumeroSerie = equipamentoDTO.NumeroSerie,
                            UltimaCalibragem = equipamentoDTO.UltimaCalibragem,
                            Observacao = equipamentoDTO.Observacao
                        };
                        _context.Equipamentos.Add(equipamento);
                    }
                }

                // 4. Processar Estoque do Cliente (produtos específicos do cliente)
                foreach (var estoqueDTO in clienteDTO.Estoques)
                {
                    // Validações para o estoque
                    if (string.IsNullOrWhiteSpace(estoqueDTO.NomeProduto))
                    {
                        return BadRequest(new { message = "Nome do produto é obrigatório." });
                    }

                    if (string.IsNullOrWhiteSpace(estoqueDTO.UnidadeProduto))
                    {
                        return BadRequest(new { message = "Unidade do produto é obrigatória." });
                    }

                    if (estoqueDTO.QuantidadeAtual < 0)
                    {
                        return BadRequest(new { message = "Quantidade atual deve ser maior ou igual a zero." });
                    }

                    if (estoqueDTO.QuantidadeMinima.HasValue && estoqueDTO.QuantidadeMinima.Value < 0)
                    {
                        return BadRequest(new { message = "Quantidade mínima deve ser maior ou igual a zero." });
                    }

                    // Primeiro, verificar se o produto já existe ou criar um novo
                    Produto produto;
                    if (estoqueDTO.IDProduto > 0)
                    {
                        // Produto existente
                        produto = await _context.Produtos.FindAsync(estoqueDTO.IDProduto);
                        if (produto == null)
                        {
                            return BadRequest(new { message = $"Produto com ID {estoqueDTO.IDProduto} não encontrado." });
                        }
                    }
                    else
                    {
                        // Verificar se já existe um produto com o mesmo nome
                        produto = await _context.Produtos
                            .FirstOrDefaultAsync(p => p.Nome.ToLower() == estoqueDTO.NomeProduto.ToLower());
                        
                        if (produto == null)
                        {
                            // Criar novo produto
                            produto = new Produto
                            {
                                Nome = estoqueDTO.NomeProduto.Trim(),
                                Concentracao = !string.IsNullOrWhiteSpace(estoqueDTO.ConcentracaoProduto) 
                                    ? estoqueDTO.ConcentracaoProduto.Trim() 
                                    : null,
                                Unidade = estoqueDTO.UnidadeProduto.Trim()
                            };
                            _context.Produtos.Add(produto);
                            await _context.SaveChangesAsync(); // Salvar para obter o ID
                        }
                        else
                        {
                            // Produto já existe, usar o existente
                            // Atualizar dados se necessário
                            if (!string.IsNullOrWhiteSpace(estoqueDTO.ConcentracaoProduto) && 
                                string.IsNullOrWhiteSpace(produto.Concentracao))
                            {
                                produto.Concentracao = estoqueDTO.ConcentracaoProduto.Trim();
                                _context.Entry(produto).State = EntityState.Modified;
                            }
                        }
                    }

                    // Verificar se o produto foi processado corretamente
                    if (produto.IDProduto <= 0)
                    {
                        return BadRequest(new { message = "Erro ao processar produto." });
                    }

                    // Agora processar o estoque
                    if (estoqueDTO.IDEstoque > 0)
                    {
                        // Atualizar estoque existente
                        var estoque = await _context.EstoqueClientes.FindAsync(estoqueDTO.IDEstoque);
                        if (estoque != null)
                        {
                            estoque.IDProduto = produto.IDProduto;
                            estoque.QuantidadeAtual = estoqueDTO.QuantidadeAtual;
                            estoque.QuantidadeMinima = estoqueDTO.QuantidadeMinima;
                            _context.Entry(estoque).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Verificar se já existe estoque para este produto e cliente
                        var estoqueExistente = await _context.EstoqueClientes
                            .FirstOrDefaultAsync(e => e.IDCliente == cliente.IDCliente && e.IDProduto == produto.IDProduto);
                        
                        if (estoqueExistente != null)
                        {
                            // Atualizar estoque existente
                            estoqueExistente.QuantidadeAtual = estoqueDTO.QuantidadeAtual;
                            estoqueExistente.QuantidadeMinima = estoqueDTO.QuantidadeMinima;
                            _context.Entry(estoqueExistente).State = EntityState.Modified;
                        }
                        else
                        {
                            // Criar novo estoque
                            var estoque = new EstoqueCliente
                            {
                                IDCliente = cliente.IDCliente,
                                IDProduto = produto.IDProduto,
                                QuantidadeAtual = estoqueDTO.QuantidadeAtual,
                                QuantidadeMinima = estoqueDTO.QuantidadeMinima
                            };
                            _context.EstoqueClientes.Add(estoque);
                        }
                    }

                    // Verificar se o estoque foi processado corretamente
                    if (estoqueDTO.QuantidadeAtual < 0)
                    {
                        return BadRequest(new { message = "Quantidade atual deve ser maior ou igual a zero." });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Verificar se o cliente foi salvo corretamente
                if (cliente.IDCliente <= 0)
                {
                    return BadRequest(new { message = "Erro ao salvar cliente após commit." });
                }

                // Retornar o cliente com todos os dados relacionados
                var clienteCompleto = await _context.Clientes
                    .Include(c => c.Piscinas)
                    .Include(c => c.Equipamentos)
                    .Include(c => c.Estoques)
                        .ThenInclude(e => e.Produto)
                    .FirstOrDefaultAsync(c => c.IDCliente == cliente.IDCliente);

                if (clienteCompleto == null)
                {
                    return BadRequest(new { message = "Erro ao carregar cliente após salvamento." });
                }

                return CreatedAtAction(nameof(GetCliente), new { id = cliente.IDCliente }, clienteCompleto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                // Log detalhado do erro para debug
                var errorMessage = $"Erro ao processar transação: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                
                return BadRequest(new { message = errorMessage });
            }
        }

        [HttpPut("completo/{id}")]
        public async Task<ActionResult<Cliente>> PutClienteCompleto(int id, ClienteCompletoDTO clienteDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Validações iniciais
                if (clienteDTO == null)
                {
                    return BadRequest(new { message = "Dados do cliente não fornecidos." });
                }

                if (id != clienteDTO.IDCliente)
                {
                    return BadRequest(new { message = "ID do cliente não confere." });
                }

                // Verificar se o cliente existe
                var clienteExistente = await _context.Clientes.FindAsync(id);
                if (clienteExistente == null)
                {
                    return NotFound(new { message = "Cliente não encontrado." });
                }

                // Verificar se já existe outro cliente com o mesmo nome (ignorando o atual)
                if (await _context.Clientes.AnyAsync(c => c.Nome.ToLower() == clienteDTO.Nome.ToLower() && c.IDCliente != id))
                {
                    return Conflict(new { message = "Já existe um cliente com este nome." });
                }

                // 1. Atualizar o Cliente
                clienteExistente.Nome = clienteDTO.Nome;
                clienteExistente.Tipo = clienteDTO.Tipo;
                clienteExistente.Telefone = clienteDTO.Telefone;
                clienteExistente.Email = clienteDTO.Email;
                clienteExistente.Endereco = clienteDTO.Endereco;
                clienteExistente.DiasDeVisita = clienteDTO.DiasDeVisita;
                clienteExistente.Observacoes = clienteDTO.Observacoes;
                
                _context.Entry(clienteExistente).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                // 2. Processar Piscinas
                foreach (var piscinaDTO in clienteDTO.Piscinas)
                {
                    if (piscinaDTO.IDPiscina > 0)
                    {
                        // Atualizar piscina existente
                        var piscina = await _context.Piscinas.FindAsync(piscinaDTO.IDPiscina);
                        if (piscina != null)
                        {
                            piscina.Tipo = piscinaDTO.Tipo;
                            piscina.VolumeLitros = piscinaDTO.VolumeLitros;
                            piscina.Localizacao = piscinaDTO.Localizacao;
                            _context.Entry(piscina).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Criar nova piscina
                        var piscina = new Piscina
                        {
                            IDCliente = clienteExistente.IDCliente,
                            Tipo = piscinaDTO.Tipo,
                            VolumeLitros = piscinaDTO.VolumeLitros,
                            Localizacao = piscinaDTO.Localizacao
                        };
                        _context.Piscinas.Add(piscina);
                    }
                }

                // 3. Processar Equipamentos
                foreach (var equipamentoDTO in clienteDTO.Equipamentos)
                {
                    if (equipamentoDTO.IDEquipamento > 0)
                    {
                        // Atualizar equipamento existente
                        var equipamento = await _context.Equipamentos.FindAsync(equipamentoDTO.IDEquipamento);
                        if (equipamento != null)
                        {
                            equipamento.Tipo = equipamentoDTO.Tipo;
                            equipamento.NumeroSerie = equipamentoDTO.NumeroSerie;
                            equipamento.UltimaCalibragem = equipamentoDTO.UltimaCalibragem;
                            equipamento.Observacao = equipamentoDTO.Observacao;
                            _context.Entry(equipamento).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Criar novo equipamento
                        var equipamento = new Equipamento
                        {
                            IDCliente = clienteExistente.IDCliente,
                            Tipo = equipamentoDTO.Tipo,
                            NumeroSerie = equipamentoDTO.NumeroSerie,
                            UltimaCalibragem = equipamentoDTO.UltimaCalibragem,
                            Observacao = equipamentoDTO.Observacao
                        };
                        _context.Equipamentos.Add(equipamento);
                    }
                }

                // 4. Processar Estoque do Cliente
                foreach (var estoqueDTO in clienteDTO.Estoques)
                {
                    // Validações para o estoque
                    if (string.IsNullOrWhiteSpace(estoqueDTO.NomeProduto))
                    {
                        return BadRequest(new { message = "Nome do produto é obrigatório." });
                    }

                    if (string.IsNullOrWhiteSpace(estoqueDTO.UnidadeProduto))
                    {
                        return BadRequest(new { message = "Unidade do produto é obrigatória." });
                    }

                    if (estoqueDTO.QuantidadeAtual < 0)
                    {
                        return BadRequest(new { message = "Quantidade atual deve ser maior ou igual a zero." });
                    }

                    if (estoqueDTO.QuantidadeMinima.HasValue && estoqueDTO.QuantidadeMinima.Value < 0)
                    {
                        return BadRequest(new { message = "Quantidade mínima deve ser maior ou igual a zero." });
                    }

                    // Primeiro, verificar se o produto já existe ou criar um novo
                    Produto produto;
                    if (estoqueDTO.IDProduto > 0)
                    {
                        // Produto existente
                        produto = await _context.Produtos.FindAsync(estoqueDTO.IDProduto);
                        if (produto == null)
                        {
                            return BadRequest(new { message = $"Produto com ID {estoqueDTO.IDProduto} não encontrado." });
                        }
                    }
                    else
                    {
                        // Verificar se já existe um produto com o mesmo nome
                        produto = await _context.Produtos
                            .FirstOrDefaultAsync(p => p.Nome.ToLower() == estoqueDTO.NomeProduto.ToLower());
                        
                        if (produto == null)
                        {
                            // Criar novo produto
                            produto = new Produto
                            {
                                Nome = estoqueDTO.NomeProduto.Trim(),
                                Concentracao = !string.IsNullOrWhiteSpace(estoqueDTO.ConcentracaoProduto) 
                                    ? estoqueDTO.ConcentracaoProduto.Trim() 
                                    : null,
                                Unidade = estoqueDTO.UnidadeProduto.Trim()
                            };
                            _context.Produtos.Add(produto);
                            await _context.SaveChangesAsync(); // Salvar para obter o ID
                        }
                        else
                        {
                            // Produto já existe, usar o existente
                            // Atualizar dados se necessário
                            if (!string.IsNullOrWhiteSpace(estoqueDTO.ConcentracaoProduto) && 
                                string.IsNullOrWhiteSpace(produto.Concentracao))
                            {
                                produto.Concentracao = estoqueDTO.ConcentracaoProduto.Trim();
                                _context.Entry(produto).State = EntityState.Modified;
                            }
                        }
                    }

                    // Verificar se o produto foi processado corretamente
                    if (produto.IDProduto <= 0)
                    {
                        return BadRequest(new { message = "Erro ao processar produto." });
                    }

                    // Agora processar o estoque
                    if (estoqueDTO.IDEstoque > 0)
                    {
                        // Atualizar estoque existente
                        var estoque = await _context.EstoqueClientes.FindAsync(estoqueDTO.IDEstoque);
                        if (estoque != null)
                        {
                            estoque.IDProduto = produto.IDProduto;
                            estoque.QuantidadeAtual = estoqueDTO.QuantidadeAtual;
                            estoque.QuantidadeMinima = estoqueDTO.QuantidadeMinima;
                            _context.Entry(estoque).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Verificar se já existe estoque para este produto e cliente
                        var estoqueExistente = await _context.EstoqueClientes
                            .FirstOrDefaultAsync(e => e.IDCliente == clienteExistente.IDCliente && e.IDProduto == produto.IDProduto);
                        
                        if (estoqueExistente != null)
                        {
                            // Atualizar estoque existente
                            estoqueExistente.QuantidadeAtual = estoqueDTO.QuantidadeAtual;
                            estoqueExistente.QuantidadeMinima = estoqueDTO.QuantidadeMinima;
                            _context.Entry(estoqueExistente).State = EntityState.Modified;
                        }
                        else
                        {
                            // Criar novo estoque
                            var estoque = new EstoqueCliente
                            {
                                IDCliente = clienteExistente.IDCliente,
                                IDProduto = produto.IDProduto,
                                QuantidadeAtual = estoqueDTO.QuantidadeAtual,
                                QuantidadeMinima = estoqueDTO.QuantidadeMinima
                            };
                            _context.EstoqueClientes.Add(estoque);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Retornar o cliente com todos os dados relacionados
                var clienteCompleto = await _context.Clientes
                    .Include(c => c.Piscinas)
                    .Include(c => c.Equipamentos)
                    .Include(c => c.Estoques)
                        .ThenInclude(e => e.Produto)
                    .FirstOrDefaultAsync(c => c.IDCliente == clienteExistente.IDCliente);

                if (clienteCompleto == null)
                {
                    return BadRequest(new { message = "Erro ao carregar cliente após salvamento." });
                }

                return Ok(clienteCompleto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                // Log detalhado do erro para debug
                var errorMessage = $"Erro ao processar transação: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                
                return BadRequest(new { message = errorMessage });
            }
        }
    }
} 