using Microsoft.EntityFrameworkCore;
using GestaoPiscina.Server.Models;

namespace GestaoPiscina.Server.Data
{
    public class GestaoPiscinaContext : DbContext
    {
        public GestaoPiscinaContext(DbContextOptions<GestaoPiscinaContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Piscina> Piscinas { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<OrdemDeServico> OrdensDeServico { get; set; }
        public DbSet<EstoqueCliente> EstoqueClientes { get; set; }
        public DbSet<Equipamento> Equipamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Gestor> Gestores { get; set; }
        public DbSet<GestorCliente> GestorClientes { get; set; }
        public DbSet<DosagemProduto> DosagensProdutos { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<ChecklistItem> ChecklistItens { get; set; }
        public DbSet<PushSubscriptionRegistro> PushSubscriptionRegistros { get; set; }
        public DbSet<RotaVisita> RotaVisitas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações das entidades
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.IDCliente);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Endereco).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DiasDeVisita).HasMaxLength(50);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.CNPJ).HasMaxLength(18);
            });

            modelBuilder.Entity<Piscina>(entity =>
            {
                entity.HasKey(e => e.IDPiscina);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.VolumeM3).IsRequired().HasPrecision(10, 2);
                entity.Property(e => e.Localizacao).HasMaxLength(255);
                entity.Property(e => e.Coberta).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Aquecida).IsRequired();
                entity.Property(e => e.RecorrenciaFrequencia).IsRequired().HasMaxLength(20);
                entity.Property(e => e.RecorrenciaDiasSemana).HasMaxLength(50);
                entity.Property(e => e.RecorrenciaTermino).IsRequired().HasMaxLength(15);
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Piscinas)
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(e => e.IDProduto);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Concentracao).HasPrecision(10, 2);
                entity.Property(e => e.Unidade).IsRequired().HasMaxLength(10);
            });

            modelBuilder.Entity<OrdemDeServico>(entity =>
            {
                entity.HasKey(e => e.IDOS);
                entity.Property(e => e.DataExecucao).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.pH).IsRequired().HasPrecision(10, 2);
                entity.Property(e => e.Alcalinidade).IsRequired().HasPrecision(10, 3);
                entity.Property(e => e.CloroLivre).IsRequired().HasPrecision(10, 3);
                entity.Property(e => e.DurezaCalcica).IsRequired().HasPrecision(10, 3);
                entity.Property(e => e.pHDepois).HasPrecision(10, 2);
                entity.Property(e => e.AlcalinidadeDepois).HasPrecision(10, 3);
                entity.Property(e => e.CloroLivreDepois).HasPrecision(10, 3);
                entity.Property(e => e.DurezaCalcicaDepois).HasPrecision(10, 3);
                entity.HasOne(e => e.Piscina)
                    .WithMany(p => p.OrdensDeServico)
                    .HasForeignKey(e => e.IDPiscina)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Tecnico)
                    .WithMany()
                    .HasForeignKey(e => e.IDUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AprovadorUsuario)
                    .WithMany()
                    .HasForeignKey(e => e.Aprovador)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DosagemProduto>(entity =>
            {
                entity.HasKey(e => e.IDDosagem);
                entity.Property(e => e.Quantidade).IsRequired().HasPrecision(10, 2);
                entity.HasOne(e => e.OrdemDeServico)
                    .WithMany(o => o.Dosagens)
                    .HasForeignKey(e => e.IDOS)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Produto)
                    .WithMany()
                    .HasForeignKey(e => e.IDProduto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EstoqueCliente>(entity =>
            {
                entity.HasKey(e => e.IDEstoque);
                entity.Property(e => e.QuantidadeMinima).HasPrecision(10, 2);
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Estoques)
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Produto)
                    .WithMany(p => p.Estoques)
                    .HasForeignKey(e => e.IDProduto)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.HasKey(e => e.IDMovimentacao);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Quantidade).IsRequired().HasPrecision(10, 2);
                entity.Property(e => e.QuantidadeContada).HasPrecision(10, 2);
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.Observacao).HasMaxLength(255);
                entity.HasOne(e => e.Cliente)
                    .WithMany()
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Produto)
                    .WithMany()
                    .HasForeignKey(e => e.IDProduto)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Dosagem)
                    .WithMany()
                    .HasForeignKey(e => e.IDDosagem)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasIndex(e => new { e.IDCliente, e.IDProduto });
            });

            modelBuilder.Entity<Equipamento>(entity =>
            {
                entity.HasKey(e => e.IDEquipamento);
                entity.Property(e => e.NumeroSerie).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descricao).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Equipamentos)
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Perfil>(entity =>
            {
                entity.HasKey(e => e.IDPerfil);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descricao).HasMaxLength(255);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IDUsuario);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Ativo).IsRequired();
                entity.Property(e => e.DataCriacao).IsRequired();
                entity.Property(e => e.FotoUrl).HasMaxLength(500);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Relacionamento com Perfil
                entity.HasOne(e => e.Perfil)
                    .WithMany()
                    .HasForeignKey(e => e.IDPerfil)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Gestor>(entity =>
            {
                entity.HasKey(e => e.IDGestor);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ativo).IsRequired();
                entity.Property(e => e.DataCadastro).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<GestorCliente>(entity =>
            {
                entity.HasKey(e => new { e.IDGestor, e.IDCliente });
                entity.HasOne(e => e.Gestor)
                    .WithMany(g => g.GestorClientes)
                    .HasForeignKey(e => e.IDGestor)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.GestorClientes)
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChecklistItem>(entity =>
            {
                entity.HasKey(e => e.IDChecklistItem);
                entity.Property(e => e.Chave).IsRequired().HasMaxLength(60);
                entity.Property(e => e.Texto).IsRequired().HasMaxLength(255);
                entity.Property(e => e.TiposClienteObrigatorio).HasMaxLength(100);
                entity.Property(e => e.TiposPiscinaObrigatorio).HasMaxLength(100);
                entity.HasIndex(e => e.Chave).IsUnique();
            });

            modelBuilder.Entity<PushSubscriptionRegistro>(entity =>
            {
                entity.HasKey(e => e.IDPushSubscriptionRegistro);
                entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(500);
                entity.Property(e => e.P256dh).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Auth).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Endpoint).IsUnique();
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IDUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RotaVisita>(entity =>
            {
                entity.HasKey(e => e.IDRotaVisita);
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.Ordem).IsRequired();
                entity.Property(e => e.DataConfirmacao).IsRequired();
                // Uma linha por cliente/técnico/dia — "Confirmar atribuição" sempre apaga e
                // recria as linhas do técnico+dia (ver RotasController), então isso só
                // protege contra alguma dupla-inserção acidental.
                entity.HasIndex(e => new { e.IDUsuario, e.Data, e.IDCliente }).IsUnique();
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IDUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Cliente)
                    .WithMany()
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 