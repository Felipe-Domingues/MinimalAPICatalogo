
using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Context;
using MinimalApiCatalogo.Models;

namespace MinimalAPICatalogo.ApiEndpoints;
public static class ProdutosEndpoints
{
  public static void MapProdutosEndpoints(this WebApplication app)
  {
    app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos.AsNoTracking().ToListAsync())
      .WithTags("Produtos")
      .RequireAuthorization();

    app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) =>
    {
      return await db.Produtos.FindAsync(id)
      is Produto produto
          ? Results.Ok(produto)
          : Results.NotFound($"Produto {id} não encontrado...");
    })
        .WithTags("Produtos")
        .RequireAuthorization();

    app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
    {
      if (produto is null)
        return Results.BadRequest("Dados inválidos");

      db.Produtos.Add(produto);
      await db.SaveChangesAsync();

      return Results.Created($"/produtos/{produto.ProdutoId}", produto);
    })
        .WithTags("Produtos")
        .RequireAuthorization();

    app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbContext db) =>
    {
      if (id != produto.ProdutoId)
        return Results.BadRequest("Dados inválidos");

      var produtoDB = await db.Produtos.FindAsync(id);
      if (produtoDB is null)
        return Results.NotFound($"Produto {id} não encontrado...");

      produtoDB.Nome = produto.Nome;
      produtoDB.Descricao = produto.Descricao;
      produtoDB.Preco = produto.Preco;
      produtoDB.Imagem = produto.Imagem;
      produtoDB.DataCompra = produto.DataCompra;
      produtoDB.Estoque = produto.Estoque;
      produtoDB.CategoriaId = produto.CategoriaId;

      await db.SaveChangesAsync();
      return Results.Ok(produtoDB);
    })
        .WithTags("Produtos")
        .RequireAuthorization();

    app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) =>
    {
      var produto = await db.Produtos.FindAsync(id);
      if (produto is null)
        return Results.NotFound($"Produto {id} não encontrado...");

      db.Produtos.Remove(produto);
      await db.SaveChangesAsync();

      return Results.NoContent();
    })
        .WithTags("Produtos")
        .RequireAuthorization();
  }
}
