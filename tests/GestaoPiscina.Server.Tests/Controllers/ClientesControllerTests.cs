using Microsoft.AspNetCore.Mvc;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // RF01 – Cadastro de Clientes.
    public class ClientesControllerTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private ClientesController Controller => new(_db.Context);

        [Fact]
        public async Task PostCliente_ComNomeUnico_CriaComSucesso()
        {
            var cliente = Fabrica.Cliente("Condomínio Alfa");

            var resultado = await Controller.PostCliente(cliente);

            var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var criado = Assert.IsType<Cliente>(created.Value);
            Assert.True(criado.IDCliente > 0);
        }

        [Fact]
        public async Task PostCliente_ComNomeJaExistente_RetornaConflict()
        {
            _db.Context.Clientes.Add(Fabrica.Cliente("Condomínio Alfa"));
            await _db.Context.SaveChangesAsync();

            var resultado = await Controller.PostCliente(Fabrica.Cliente("Condomínio Alfa"));

            Assert.IsType<ConflictObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostCliente_ComNomeExistenteEmCaixaDiferente_RetornaConflict()
        {
            // O sistema não deve permitir cadastrar "Condomínio Alfa" e "CONDOMÍNIO ALFA"
            // como clientes distintos.
            _db.Context.Clientes.Add(Fabrica.Cliente("Condomínio Alfa"));
            await _db.Context.SaveChangesAsync();

            var resultado = await Controller.PostCliente(Fabrica.Cliente("CONDOMÍNIO ALFA"));

            Assert.IsType<ConflictObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task DeleteCliente_Inexistente_RetornaNotFound()
        {
            var resultado = await Controller.DeleteCliente(999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task DeleteCliente_Existente_RemoveComSucesso()
        {
            var cliente = Fabrica.Cliente();
            _db.Context.Clientes.Add(cliente);
            await _db.Context.SaveChangesAsync();

            var resultado = await Controller.DeleteCliente(cliente.IDCliente);

            Assert.IsType<NoContentResult>(resultado);
            Assert.Null(await _db.Context.Clientes.FindAsync(cliente.IDCliente));
        }

        public void Dispose() => _db.Dispose();
    }
}
