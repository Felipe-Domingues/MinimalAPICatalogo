using Microsoft.AspNetCore.Authorization;
using MinimalAPICatalogo.Models;
using MinimalAPICatalogo.Services;

namespace MinimalAPICatalogo.ApiEndpoints;
public static class AutenticacaoEndpoints
{
  public static void MapAutenticacaoEndpoints(this WebApplication app)
  {
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
  }
}
