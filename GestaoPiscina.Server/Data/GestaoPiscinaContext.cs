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
        public DbSet<EstoqueCliente> EstoquesCliente { get; set; }
        public DbSet<Equipamento> Equipamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações das entidades
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.IDCliente);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Endereco).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DiasDeVisita).HasMaxLength(50);
                entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<Piscina>(entity =>
            {
                entity.HasKey(e => e.IDPiscina);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
                entity.Property(e => e.VolumeLitros).IsRequired().HasPrecision(10, 2);
                entity.Property(e => e.Localizacao).HasMaxLength(255);
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Piscinas)
                    .HasForeignKey(e => e.IDCliente)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(e => e.IDProduto);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Concentracao).HasMaxLength(50);
                entity.Property(e => e.Unidade).IsRequired().HasMaxLength(10);
            });

            modelBuilder.Entity<OrdemDeServico>(entity =>
            {
                entity.HasKey(e => e.IDOS);
                entity.Property(e => e.DataExecucao).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Piscina)
                    .WithMany(p => p.OrdensDeServico)
                    .HasForeignKey(e => e.IDPiscina)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EstoqueCliente>(entity =>
            {
                entity.HasKey(e => e.IDEstoque);
                entity.Property(e => e.QuantidadeAtual).IsRequired().HasPrecision(10, 2);
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

            modelBuilder.Entity<Equipamento>(entity =>
            {
                entity.HasKey(e => e.IDEquipamento);
                entity.Property(e => e.NumeroSerie).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
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
                entity.HasIndex(e => e.Login).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Relacionamento com Perfil
                entity.HasOne(e => e.Perfil)
                    .WithMany()
                    .HasForeignKey(e => e.IDPerfil)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
} 