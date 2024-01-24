
using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Context;
using MinimalApiCatalogo.Models;

namespace MinimalAPICatalogo.ApiEndpoints;
public static class CategoriasEndpoints
{
  public static void MapCategoriasEndpoints(this WebApplication app)
  {
    app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.AsNoTracking().ToListAsync())
        .WithTags("Categorias")
        .RequireAuthorization();

    app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) =>
    {
      return await db.Categorias.FindAsync(id)
      is Categoria categoria
          ? Results.Ok(categoria)
          : Results.NotFound($"Categoria {id} não encontrada...");
    })
        .WithTags("Categorias")
        .RequireAuthorization();

    app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
    {
      if (categoria is null)
        return Results.BadRequest("Dados inválidos");

      db.Categorias.Add(categoria);
      await db.SaveChangesAsync();

      return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
    })
        .WithTags("Categorias")
        .RequireAuthorization();

    app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContext db) =>
    {
      if (id != categoria.CategoriaId)
        return Results.BadRequest("Dados inválidos");

      var categoriaDB = await db.Categorias.FindAsync(id);
      if (categoriaDB is null)
        return Results.NotFound($"Categoria {id} não encontrada...");

      categoriaDB.Nome = categoria.Nome;
      categoriaDB.Descricao = categoria.Descricao;

      await db.SaveChangesAsync();
      return Results.Ok(categoriaDB);
    })
        .WithTags("Categorias")
        .RequireAuthorization();

    app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) =>
    {
      var categoria = await db.Categorias.FindAsync(id);
      if (categoria is null)
        return Results.NotFound($"Categoria {id} não encontrada...");

      db.Categorias.Remove(categoria);
      await db.SaveChangesAsync();

      return Results.NoContent();
    })
        .WithTags("Categorias")
        .RequireAuthorization();
  }
}