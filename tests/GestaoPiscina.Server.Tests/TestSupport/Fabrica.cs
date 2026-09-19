using GestaoPiscina.Server.Models;
using GestaoPiscina.Server.Services;

namespace GestaoPiscina.Server.Tests.TestSupport
{
    // Constrói entidades válidas com valores padrão razoáveis, para os testes
    // só precisarem sobrescrever o que é relevante para o cenário.
    internal static class Fabrica
    {
        public static Perfil Perfil(string nome = "Técnico", bool podeGerenciarOrdensServico = true, bool exigeEndereco = false) => new()
        {
            Nome = nome,
            PodeGerenciarUsuarios = nome == "Administrador",
            PodeGerenciarClientes = nome == "Administrador",
            PodeGerenciarPiscinas = nome == "Administrador",
            PodeGerenciarProdutos = nome == "Administrador",
            PodeGerenciarEstoque = true,
            PodeGerenciarOrdensServico = podeGerenciarOrdensServico,
            PodeGerenciarEquipamentos = nome == "Administrador",
            PodeVisualizarRelatorios = true,
            PodeConfigurarSistema = nome == "Administrador",
            ExigeEndereco = exigeEndereco
        };

        public static Usuario Usuario(
            Perfil perfil,
            string login = "tecnico1",
            string senha = "Senha@123",
            bool ativo = true,
            string nome = "Técnico Um") => new()
        {
            Nome = nome,
            Email = $"{login}@teste.com",
            Login = login,
            SenhaHash = PasswordHasher.Hash(senha),
            Ativo = ativo,
            Perfil = perfil
        };

        public static Cliente Cliente(string nome = "Cliente Teste", string tipo = "Uso Coletivo") => new()
        {
            Nome = nome,
            Tipo = tipo,
            Endereco = "Rua Teste, 123"
        };

        public static Piscina Piscina(
            Cliente cliente,
            decimal volumeM3 = 10m,
            string recorrenciaFrequencia = "Nenhuma",
            int recorrenciaIntervalo = 1,
            string? recorrenciaDiasSemana = null,
            DateTime? recorrenciaDataInicio = null,
            string recorrenciaTermino = "Nunca",
            DateTime? recorrenciaDataFim = null,
            int? recorrenciaOcorrencias = null) => new()
        {
            Cliente = cliente,
            Tipo = "adulto",
            VolumeM3 = volumeM3,
            Coberta = "Não",
            Aquecida = false,
            RecorrenciaFrequencia = recorrenciaFrequencia,
            RecorrenciaIntervalo = recorrenciaIntervalo,
            RecorrenciaDiasSemana = recorrenciaDiasSemana,
            RecorrenciaDataInicio = recorrenciaDataInicio,
            RecorrenciaTermino = recorrenciaTermino,
            RecorrenciaDataFim = recorrenciaDataFim,
            RecorrenciaOcorrencias = recorrenciaOcorrencias
        };

        public static Produto Produto(string nome = "Cloro", string unidade = "kg") => new()
        {
            Nome = nome,
            Unidade = unidade
        };

        public static EstoqueCliente Estoque(Cliente cliente, Produto produto, decimal? quantidadeMinima = null) => new()
        {
            Cliente = cliente,
            Produto = produto,
            QuantidadeMinima = quantidadeMinima
        };

        // OS válida "pronta para finalizar": checklist concluído, horários coerentes,
        // parâmetros de água preenchidos — usada como base pelos testes de
        // ValidarRegrasDeNegocioAsync, que então quebram um campo por vez.
        public static OrdemDeServico OrdemDeServicoValida(Piscina piscina, Usuario tecnico, DateTime? data = null)
        {
            var dataExecucao = data ?? new DateTime(2024, 1, 10);
            return new OrdemDeServico
            {
                // FK escalares diretas — não usar as propriedades de navegação aqui:
                // ValidarRegrasDeNegocioAsync lê OrdemDeServico.IDPiscina/IDUsuario antes
                // de o EF anexar a entidade ao change tracker, então o "fixup" automático
                // via navegação ainda não teria acontecido nesse ponto.
                IDPiscina = piscina.IDPiscina,
                IDUsuario = tecnico.IDUsuario,
                DataExecucao = dataExecucao,
                Status = "Em Aberto",
                ChecklistConcluido = false,
                pH = 7.2m,
                Alcalinidade = 100m,
                CloroLivre = 2m,
                DurezaCalcica = 200m,
                pHDepois = 7.4m,
                AlcalinidadeDepois = 100m,
                CloroLivreDepois = 2m,
                DurezaCalcicaDepois = 200m,
                FotosAntes = "http://exemplo.com/antes.jpg",
                FotosDepois = "http://exemplo.com/depois.jpg",
                HoraInicio = dataExecucao.AddHours(8),
                HoraTermino = dataExecucao.AddHours(9)
            };
        }
    }
}
