using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task GerarOSAutomaticas_GeraOSComTecnicoEmBranco()
        {
            await CriarTecnicoPadraoAsync();
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente,
                recorrenciaFrequencia: "Diaria",
                recorrenciaIntervalo: 1,
                recorrenciaDataInicio: new DateTime(2024, 1, 1));
            _db.Context.AddRange(cliente, piscina);
            await _db.Context.SaveChangesAsync();

            await Controller.GerarOSAutomaticas(new GerarOSAutomaticasRequest
            {
                DataInicio = new DateTime(2024, 1, 1),
                DataFim = new DateTime(2024, 1, 2)
            });

            var geradas = _db.Context.OrdensDeServico.ToList();
            Assert.NotEmpty(geradas);
            Assert.All(geradas, os =>
            {
                Assert.Null(os.IDUsuario);
                Assert.Equal(default, os.HoraInicio);
                Assert.Equal(default, os.HoraTermino);
            });
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

    // Tratamento de Ocorrências: trava de edição depois de já persistida (ver
    // PutOrdemDeServico), notificação push ao Aprovador na primeira vez que a OS é salva
    // com essa condição, e aprovação posterior pelo químico responsável (AprovarOcorrencia).
    public class OrdensDeServicoControllerOcorrenciaTests : IDisposable
    {
        private readonly SqliteInMemoryContext _db = new();
        private OrdensDeServicoController Controller => new(_db.Context, new FakePushNotificationService());

        private async Task<(Piscina piscina, Usuario tecnico, Usuario aprovador)> CriarCenarioAsync()
        {
            var perfilTecnico = Fabrica.Perfil("Técnico");
            var tecnico = Fabrica.Usuario(perfilTecnico, login: "tecnico1");
            var perfilQuimica = Fabrica.Perfil("Química");
            var aprovador = Fabrica.Usuario(perfilQuimica, login: "quimica1", nome: "Química Um");
            var cliente = Fabrica.Cliente();
            var piscina = Fabrica.Piscina(cliente);
            _db.Context.AddRange(perfilTecnico, tecnico, perfilQuimica, aprovador, cliente, piscina);
            await _db.Context.SaveChangesAsync();
            return (piscina, tecnico, aprovador);
        }

        // Cria (via Post) e desanexa a OS resultante do change tracker — sem isso, o PUT de
        // um teste que reaproveitasse essa OS falharia ("already tracked"), já que o contexto
        // é compartilhado dentro do teste.
        private async Task<int> CriarOSAsync(OrdensDeServicoController controller, OrdemDeServico os)
        {
            var resultado = await controller.PostOrdemDeServico(os);
            var criada = (OrdemDeServico)((CreatedAtActionResult)resultado.Result!).Value!;
            _db.Context.Entry(criada).State = EntityState.Detached;
            return criada.IDOS;
        }

        private async Task<(int idOS, Usuario aprovador, Usuario tecnico)> CriarOSComOcorrenciaAsync()
        {
            var (piscina, tecnico, aprovador) = await CriarCenarioAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Ocorrência";
            os.Aprovador = aprovador.IDUsuario;

            var idOS = await CriarOSAsync(Controller, os);
            return (idOS, aprovador, tecnico);
        }

        // Clona os campos escalares de uma OS já salva para montar o corpo de um PUT, sem
        // reutilizar a instância rastreada (mesmo motivo do Detached acima).
        private static OrdemDeServico ClonarParaEdicao(OrdemDeServico original, Action<OrdemDeServico>? ajustar = null)
        {
            var clone = new OrdemDeServico
            {
                IDOS = original.IDOS,
                IDPiscina = original.IDPiscina,
                IDUsuario = original.IDUsuario,
                DataExecucao = original.DataExecucao,
                Status = original.Status,
                ChecklistConcluido = original.ChecklistConcluido,
                ChecklistItens = original.ChecklistItens,
                Observacoes = original.Observacoes,
                FotosAntes = original.FotosAntes,
                FotosDepois = original.FotosDepois,
                FotosOcorrencias = original.FotosOcorrencias,
                RelatorioGerado = original.RelatorioGerado,
                Aprovador = original.Aprovador,
                OcorrenciaAprovada = original.OcorrenciaAprovada,
                DataAprovacaoOcorrencia = original.DataAprovacaoOcorrencia,
                pH = original.pH,
                Alcalinidade = original.Alcalinidade,
                CloroLivre = original.CloroLivre,
                DurezaCalcica = original.DurezaCalcica,
                HoraInicio = original.HoraInicio,
                HoraTermino = original.HoraTermino
            };
            ajustar?.Invoke(clone);
            return clone;
        }

        private OrdensDeServicoController ControllerAutenticadoComo(int idUsuario)
        {
            var controller = Controller;
            var claims = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()) });
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claims) }
            };
            return controller;
        }

        [Fact]
        public async Task PutOrdemDeServico_QuandoStatusJaEraOcorrencia_RetornaBadRequestSemAlterarNada()
        {
            var (idOS, _, _) = await CriarOSComOcorrenciaAsync();
            var salvaAntes = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);
            var tentativa = ClonarParaEdicao(salvaAntes, o => o.Observacoes = "Tentando editar depois de registrada a ocorrência.");

            var resultado = await Controller.PutOrdemDeServico(idOS, tentativa);

            Assert.IsType<BadRequestObjectResult>(resultado);
            var salvaDepois = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);
            Assert.Null(salvaDepois.Observacoes);
        }

        [Fact]
        public async Task PutOrdemDeServico_ComIdInexistente_RetornaNotFound()
        {
            var (piscina, tecnico, _) = await CriarCenarioAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.IDOS = 999999;

            var resultado = await Controller.PutOrdemDeServico(999999, os);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task PutOrdemDeServico_TransicaoParaOcorrenciaPelaPrimeiraVez_Salva()
        {
            var (piscina, tecnico, aprovador) = await CriarCenarioAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico); // Status = "Em Aberto"
            var idOS = await CriarOSAsync(Controller, os);
            var salva = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);

            var edicao = ClonarParaEdicao(salva, o =>
            {
                o.Status = "Ocorrência";
                o.Aprovador = aprovador.IDUsuario;
                o.Observacoes = "Vazamento identificado na tubulação de retorno.";
            });

            var resultado = await Controller.PutOrdemDeServico(idOS, edicao);

            Assert.IsType<NoContentResult>(resultado);
            var salvaDepois = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);
            Assert.Equal("Ocorrência", salvaDepois.Status);
        }

        [Fact]
        public async Task PostOrdemDeServico_ComOcorrencia_NotificaOAprovador()
        {
            var (piscina, tecnico, aprovador) = await CriarCenarioAsync();
            var push = new FakePushNotificationService();
            var controller = new OrdensDeServicoController(_db.Context, push);
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico);
            os.Status = "Ocorrência";
            os.Aprovador = aprovador.IDUsuario;

            await controller.PostOrdemDeServico(os);

            var chamada = Assert.Single(push.Chamadas);
            Assert.Equal(aprovador.IDUsuario, chamada.IdUsuario);
        }

        [Fact]
        public async Task PutOrdemDeServico_TransicaoParaOcorrencia_NotificaOAprovador()
        {
            var (piscina, tecnico, aprovador) = await CriarCenarioAsync();
            var push = new FakePushNotificationService();
            var controller = new OrdensDeServicoController(_db.Context, push);
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico); // Status = "Em Aberto"
            var idOS = await CriarOSAsync(controller, os);
            Assert.Empty(push.Chamadas); // Criar como "Em Aberto" não notifica ninguém.
            var salva = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);

            var edicao = ClonarParaEdicao(salva, o =>
            {
                o.Status = "Ocorrência";
                o.Aprovador = aprovador.IDUsuario;
                o.Observacoes = "Vazamento identificado.";
            });
            await controller.PutOrdemDeServico(idOS, edicao);

            var chamada = Assert.Single(push.Chamadas);
            Assert.Equal(aprovador.IDUsuario, chamada.IdUsuario);
        }

        [Fact]
        public async Task PutOrdemDeServico_TransicaoParaOutroStatus_NaoNotificaNinguem()
        {
            var (piscina, tecnico, _) = await CriarCenarioAsync();
            var push = new FakePushNotificationService();
            var controller = new OrdensDeServicoController(_db.Context, push);
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico); // Status = "Em Aberto"
            var idOS = await CriarOSAsync(controller, os);
            var salva = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);

            var edicao = ClonarParaEdicao(salva, o => o.Status = "Em Andamento");
            await controller.PutOrdemDeServico(idOS, edicao);

            Assert.Empty(push.Chamadas);
        }

        [Fact]
        public async Task AprovarOcorrencia_ComUsuarioAprovadorCorreto_MarcaComoAprovada()
        {
            var (idOS, aprovador, _) = await CriarOSComOcorrenciaAsync();

            var resultado = await ControllerAutenticadoComo(aprovador.IDUsuario).AprovarOcorrencia(idOS);

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var os = Assert.IsType<OrdemDeServico>(ok.Value);
            Assert.True(os.OcorrenciaAprovada);
            Assert.NotNull(os.DataAprovacaoOcorrencia);
        }

        [Fact]
        public async Task AprovarOcorrencia_ComUsuarioDiferenteDoAprovador_RetornaForbidSemAlterarNada()
        {
            var (idOS, _, tecnico) = await CriarOSComOcorrenciaAsync();

            var resultado = await ControllerAutenticadoComo(tecnico.IDUsuario).AprovarOcorrencia(idOS);

            Assert.IsType<ForbidResult>(resultado);
            var salva = await _db.Context.OrdensDeServico.AsNoTracking().FirstAsync(o => o.IDOS == idOS);
            Assert.False(salva.OcorrenciaAprovada);
        }

        [Fact]
        public async Task AprovarOcorrencia_SemClaimDeUsuario_RetornaUnauthorized()
        {
            var (idOS, _, _) = await CriarOSComOcorrenciaAsync();
            var controller = Controller;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            var resultado = await controller.AprovarOcorrencia(idOS);

            Assert.IsType<UnauthorizedResult>(resultado);
        }

        [Fact]
        public async Task AprovarOcorrencia_QuandoStatusNaoEOcorrencia_RetornaBadRequest()
        {
            var (piscina, tecnico, aprovador) = await CriarCenarioAsync();
            var os = Fabrica.OrdemDeServicoValida(piscina, tecnico); // Status = "Em Aberto"
            var idOS = await CriarOSAsync(Controller, os);

            var resultado = await ControllerAutenticadoComo(aprovador.IDUsuario).AprovarOcorrencia(idOS);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task AprovarOcorrencia_ComIdInexistente_RetornaNotFound()
        {
            var resultado = await ControllerAutenticadoComo(1).AprovarOcorrencia(999999);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task AprovarOcorrencia_QuandoJaAprovada_MantemADataDaPrimeiraAprovacao()
        {
            var (idOS, aprovador, _) = await CriarOSComOcorrenciaAsync();
            var primeiraChamada = await ControllerAutenticadoComo(aprovador.IDUsuario).AprovarOcorrencia(idOS);
            var dataPrimeiraAprovacao = ((OrdemDeServico)((OkObjectResult)primeiraChamada).Value!).DataAprovacaoOcorrencia;

            var segundaChamada = await ControllerAutenticadoComo(aprovador.IDUsuario).AprovarOcorrencia(idOS);

            var ok = Assert.IsType<OkObjectResult>(segundaChamada);
            var os = Assert.IsType<OrdemDeServico>(ok.Value);
            Assert.True(os.OcorrenciaAprovada);
            Assert.Equal(dataPrimeiraAprovacao, os.DataAprovacaoOcorrencia);
        }

        public void Dispose() => _db.Dispose();
    }
}
