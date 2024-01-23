using Microsoft.EntityFrameworkCore;
using MinimalApiCatalogo.Context;
using MinimalApiCatalogo.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

var app = builder.Build();

//Endpoints Categoria
app.MapGet("/", () => "Catálogo de Produtos - 2024").ExcludeFromDescription();

app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias.AsNoTracking().ToListAsync());

app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Categorias.FindAsync(id)
        is Categoria categoria
            ? Results.Ok(categoria)
            : Results.NotFound($"Categoria {id} não encontrada...");
});

app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
{
    if (categoria is null)
        return Results.BadRequest("Dados inválidos");

    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
});

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
});

app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) =>
{
    var categoria = await db.Categorias.FindAsync(id);
    if (categoria is null)
        return Results.NotFound($"Categoria {id} não encontrada...");

    db.Categorias.Remove(categoria);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

//Endpoints Produtos
app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos.AsNoTracking().ToListAsync());

app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Produtos.FindAsync(id)
        is Produto produto
            ? Results.Ok(produto)
            : Results.NotFound($"Produto {id} não encontrado...");
});

app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
{
    if (produto is null)
        return Results.BadRequest("Dados inválidos");

    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.ProdutoId}", produto);
});

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
});

app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    if (produto is null)
        return Results.NotFound($"Produto {id} não encontrado...");

    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();