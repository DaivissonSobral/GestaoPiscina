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

                // 4. Processar Produtos (globais)
                foreach (var produtoDTO in clienteDTO.Produtos)
                {
                    if (produtoDTO.IDProduto > 0)
                    {
                        // Atualizar produto existente
                        var produto = await _context.Produtos.FindAsync(produtoDTO.IDProduto);
                        if (produto != null)
                        {
                            produto.Nome = produtoDTO.Nome;
                            produto.Concentracao = produtoDTO.Concentracao;
                            produto.Unidade = produtoDTO.Unidade;
                            _context.Entry(produto).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Criar novo produto
                        var produto = new Produto
                        {
                            Nome = produtoDTO.Nome,
                            Concentracao = produtoDTO.Concentracao,
                            Unidade = produtoDTO.Unidade
                        };
                        _context.Produtos.Add(produto);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Retornar o cliente com todos os dados relacionados
                var clienteCompleto = await _context.Clientes
                    .Include(c => c.Piscinas)
                    .Include(c => c.Equipamentos)
                    .FirstOrDefaultAsync(c => c.IDCliente == cliente.IDCliente);

                return CreatedAtAction(nameof(GetCliente), new { id = cliente.IDCliente }, clienteCompleto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = $"Erro ao processar transação: {ex.Message}" });
            }
        }
    }
} 