using MinimalAPICatalogo.ApiEndpoints;
using MinimalAPICatalogo.AppServicesExtension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddApiSwagger();
builder.AddPersistence();
builder.Services.AddCors();
builder.AddAuthenticationJwt();

var app = builder.Build();

app.MapAutenticacaoEndpoints();
app.MapCategoriasEndpoints();
app.MapProdutosEndpoints();


// Configure the HTTP request pipeline.
var enviroment = app.Environment;
app.UseExceptionHandling(enviroment)
    .UseSwaggerMiddleware()
    .UseAppCors();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();