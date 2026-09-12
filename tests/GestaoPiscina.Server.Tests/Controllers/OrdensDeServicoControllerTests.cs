using Microsoft.AspNetCore.Mvc;
using GestaoPiscina.Server.Controllers;
using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Tests.TestSupport;
using Xunit;

namespace GestaoPiscina.Server.Tests.Controllers
{
    // RN01 – Finalização de Ordem de Serviço (regras validadas em
    // ValidarRegrasDeNegocioAsync, exercitadas via PostOrdemDeServico/PutOrdemDeServico).
    public class OrdensDeServicoControllerValidacaoTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private OrdensDeServicoController Controller => new(_db.Context, new FakePushNotificationService());

        private async Task<(Piscina piscina, Usuario tecnico)> CriarPiscinaETecnicoAsync()
        {
            var perfil = Fabrica.Perfil("Técnico");
            var tecnico = Fabrica.Usuario(perfil, login: "tecnico1");
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente);
            _db.Context.AddRange(perfil, tecnico, cliente, piscina);
            await _db.Context.SaveChangesAsync();
            return (piscina, tecnico);
        }

        [Fact]
        public async Task PostOrdemDeServico_ComDadosValidos_CriaComSucesso()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Finalizada_SemChecklistConcluido_RetornaBadRequest()
        {
            // RN01: "A OS só pode ser finalizada se todos os itens do checklist
            // obrigatório forem concluídos e validados pelo técnico."
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Finalizada";
            os.ChecklistConcluido = false;

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Finalizada_ComChecklistConcluido_CriaComSucesso()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Finalizada";
            os.ChecklistConcluido = true;

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Ocorrencia_SemAprovador_RetornaBadRequest()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Ocorrência";
            os.Aprovador = null;

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Ocorrencia_ComAprovadorValido_CriaComSucesso()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var perfilAprovador = Fabrica.Perfil("Supervisor");
            var aprovador = Fabrica.Usuario(perfilAprovador, login: "supervisor1");
            _db.Context.AddRange(perfilAprovador, aprovador);
            await _db.Context.SaveChangesAsync();

            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Ocorrência";
            os.Aprovador = aprovador.IDUsuario;

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Cancelada_SemObservacoes_RetornaBadRequest()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Cancelada";
            os.Observacoes = null;

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Cancelada_ComObservacoes_CriaComSucesso()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Cancelada";
            os.Observacoes = "Cliente pediu para cancelar.";

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_Finalizada_ComTerminoAntesDoInicio_RetornaBadRequest()
        {
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Finalizada";
            os.ChecklistConcluido = true;
            os.HoraInicio = new DateTime(2024, 1, 10, 10, 0, 0);
            os.HoraTermino = new DateTime(2024, 1, 10, 9, 0, 0);

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_EmAndamento_ComTerminoAntesDoInicio_NaoValidaEsseCampo()
        {
            // Fora de Finalizada/Ocorrência, HoraTermino pode carregar um valor antigo
            // (ex.: meia-noite, herdado da criação da OS) e não deve travar o salvamento —
            // é só preenchido de verdade ao finalizar.
            var (piscina, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Em Andamento";
            os.HoraInicio = new DateTime(2024, 1, 10, 10, 0, 0);
            os.HoraTermino = new DateTime(2024, 1, 10, 0, 0, 0);

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<CreatedAtActionResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_ComPiscinaInvalida_RetornaBadRequest()
        {
            var (_, tecnico) = await CriarPiscinaETecnicoAsync();
            var os = new OrdemDeServico
            {
                IDPiscina = 999999,
                IDUsuario = tecnico.IDUsuario,
                DataExecucao = new DateTime(2024, 1, 10),
                Status = "Em Aberto",
                HoraInicio = new DateTime(2024, 1, 10, 8, 0, 0),
                HoraTermino = new DateTime(2024, 1, 10, 9, 0, 0)
            };

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task PostOrdemDeServico_ComTecnicoInvalido_RetornaBadRequest()
        {
            var (piscina, _) = await CriarPiscinaETecnicoAsync();
            var os = new OrdemDeServico
            {
                IDPiscina = piscina.IDPiscina,
                IDUsuario = 999999,
                DataExecucao = new DateTime(2024, 1, 10),
                Status = "Em Aberto",
                HoraInicio = new DateTime(2024, 1, 10, 8, 0, 0),
                HoraTermino = new DateTime(2024, 1, 10, 9, 0, 0)
            };

            var resultado = await Controller.PostOrdemDeServico(os);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        public void Dispose() => _db.Dispose();
    }

    // RF04 – Ordem de Serviço: geração automática conforme a recorrência
    // configurada em cada piscina (GerarOSAutomaticas).
    public class OrdensDeServicoControllerGeracaoAutomaticaTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private OrdensDeServicoController Controller => new(_db.Context, new FakePushNotificationService());

        private async Task<Usuario> CriarTecnicoPadraoAsync()
        {
            var perfil = Fabrica.Perfil("Técnico");
            var tecnico = Fabrica.Usuario(perfil, login: "tecnico1");
            _db.Context.AddRange(perfil, tecnico);
            await _db.Context.SaveChangesAsync();
            return tecnico;
        }

        [Fact]
        public async Task GerarOSAutomaticas_SemTecnicoCadastrado_RetornaBadRequest()
        {
            var resultado = await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 1)
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task GerarOSAutomaticas_ComDataFimAntesDaDataInicio_RetornaBadRequest()
        {
            await CriarTecnicoPadraoAsync();

            var resultado = await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 10),
                DataFim = new DateTime(2024, 1, 1)
            });

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task GerarOSAutomaticas_PiscinaSemRecorrencia_NaoGeraNenhumaOS()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente, recorrenciaFrequencia: "Nenhuma");
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 10)
            });

            Assert.Empty(_db.Context.OrdensDeServico);
        }

        [Fact]
        public async Task GerarOSAutomaticas_RecorrenciaDiaria_GeraNosDiasCorretosConformeOIntervalo()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            // A cada 2 dias, a partir de 2024-01-01.
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Diaria",
                recorrenciaIntervalo: 2,
                recorrenciaDataInicio: new DateTime(2024, 1, 1));
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 6)
            });

            var datasGeradas = _db.Context.OrdensDeServico.Select(o => o.DataExecucao.Date).OrderBy(d => d).ToList();
            Assert.Equal(new[]
            {
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 3),
                new DateTime(2024, 1, 5)
            }, datasGeradas);
        }

        [Fact]
        public async Task GerarOSAutomaticas_RecorrenciaSemanal_RespeitaOsDiasDaSemanaConfigurados()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            // 2024-01-01 é uma segunda-feira ("Seg").
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Semanal",
                recorrenciaIntervalo: 1,
                recorrenciaDiasSemana: "Seg",
                recorrenciaDataInicio: new DateTime(2024, 1, 1));
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 15)
            });

            var datasGeradas = _db.Context.OrdensDeServico.Select(o => o.DataExecucao.Date).OrderBy(d => d).ToList();
            Assert.Equal(new[]
            {
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 8),
                new DateTime(2024, 1, 15)
            }, datasGeradas);
        }

        [Fact]
        public async Task GerarOSAutomaticas_RecorrenciaMensal_GeraNoMesmoDiaDeCadaMes()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Mensal",
                recorrenciaIntervalo: 1,
                recorrenciaDataInicio: new DateTime(2024, 1, 15));
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 15),
                DataFim = new DateTime(2024, 3, 15)
            });

            var datasGeradas = _db.Context.OrdensDeServico.Select(o => o.DataExecucao.Date).OrderBy(d => d).ToList();
            Assert.Equal(new[]
            {
                new DateTime(2024, 1, 15),
                new DateTime(2024, 2, 15),
                new DateTime(2024, 3, 15)
            }, datasGeradas);
        }

        [Fact]
        public async Task GerarOSAutomaticas_NaoDuplicaOSParaUmaDataQueJaTemOS()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Diaria",
                recorrenciaIntervalo: 1,
                recorrenciaDataInicio: new DateTime(2024, 1, 1));
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            var tecnico = _db.Context.Usuarios.First();
            _db.Context.OrdensDeServico.Add(new OrdemDeServico
            {
                IDPiscina = piscina.IDPiscina,
                IDUsuario = tecnico.IDUsuario,
                DataExecucao = new DateTime(2024, 1, 1),
                Status = "Em Aberto",
                HoraInicio = new DateTime(2024, 1, 1),
                HoraTermino = new DateTime(2024, 1, 1)
            });
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 3)
            });

            // 1 preexistente + 2 novas (dias 2 e 3) = 3 no total, nunca 4.
            Assert.Equal(3, _db.Context.OrdensDeServico.Count());
        }

        [Fact]
        public async Task GerarOSAutomaticas_RespeitaOLimiteDeOcorrenciasConfigurado()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Diaria",
                recorrenciaIntervalo: 1,
                recorrenciaDataInicio: new DateTime(2024, 1, 1),
                recorrenciaTermino: "Ocorrencias",
                recorrenciaOcorrencias: 2);
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 10)
            });

            Assert.Equal(2, _db.Context.OrdensDeServico.Count());
        }

        [Fact]
        public async Task GerarOSAutomaticas_RecorrenciaComTerminoPorData_NaoGeraAposADataFim()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Diaria",
                recorrenciaIntervalo: 1,
                recorrenciaDataInicio: new DateTime(2024, 1, 1),
                recorrenciaTermino: "Data",
                recorrenciaDataFim: new DateTime(2024, 1, 3));
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 10)
            });

            var datasGeradas = _db.Context.OrdensDeServico.Select(o => o.DataExecucao.Date).OrderBy(d => d).ToList();
            Assert.Equal(new[]
            {
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 2),
                new DateTime(2024, 1, 3)
            }, datasGeradas);
        }

        public void Dispose() => _db.Dispose();
    }
}
