using System;
using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Models;

namespace MinimalApiCatalogo.Context;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto>? Produtos { get; set; }
    public DbSet<Categoria>? Categorias { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        //Categoria
        mb.Entity<Categoria>().HasKey(c => c.CategoriaId);
        mb.Entity<Categoria>().Property(c => c.Nome)
                              .HasMaxLength(100)
                              .IsRequired();
        mb.Entity<Categoria>().Property(c => c.Descricao)
                              .HasMaxLength(150)
                              .IsRequired();

        //Produto
        mb.Entity<Produto>().HasKey(p => p.ProdutoId);
        mb.Entity<Produto>().Property(p => p.Nome)
                             .HasMaxLength(100)
                             .IsRequired();
        mb.Entity<Produto>().Property(p => p.Descricao)
                              .HasMaxLength(150);
        mb.Entity<Produto>().Property(p => p.Imagem)
                            .HasMaxLength(100);
        mb.Entity<Produto>().Property(p => p.Preco)
                            .HasPrecision(14, 2);

        //Relacionamento
        mb.Entity<Produto>().HasOne<Categoria>(c => c.Categoria)
                            .WithMany(p => p.Produtos)
                            .HasForeignKey(c => c.CategoriaId);
    }
}
