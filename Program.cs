using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApiCatalogo.Context;
using MinimalApiCatalogo.Models;
using MinimalAPICatalogo.Models;
using MinimalAPICatalogo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal API Catalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Beare Scheme.
                                    Enter 'Bearer'[space]. Example: \'Bearer 123456\'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

//Endpoint Login
app.MapPost("/login", [AllowAnonymous] (UserModel userModel, ITokenService tokenService) =>
{
    if (userModel is null)
        return Results.BadRequest("Dados inválidos");

    if (userModel.UserName == "admin" && userModel.Password == "admin")
    {
        var tokenString = tokenService.GerarToken(
            app.Configuration["Jwt:Key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"],
            userModel
        );

        return Results.Ok(new { token = tokenString });
    }

    return Results.BadRequest("Login inválido");
})
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status200OK)
    .WithName("Login")
    .WithTags("Autenticação");

//Endpoints Categoria
app.MapGet("/", () => "Catálogo de Produtos - 2024").ExcludeFromDescription();

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

//Endpoints Produtos
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();