using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Interface;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Services;
using minimal_api.Infraestrutura.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorService, AdministradorServices>();
builder.Services.AddScoped<IVeiculosServices, VeiculosServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(
    options => {
        options.UseMySql(
            builder.Configuration.GetConnectionString("mysql")?.ToString(),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql")?.ToString())
        );
    }
);

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home()));
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService adm) =>
{
    if (adm.Login(loginDTO) != null)
    {
        return Results.Ok("Login feito com sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
});
#endregion

#region Veiuclos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculosServices veiculoService) =>
{
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoService.AdicionaVeiculo(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
});
#endregion

#region APP
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
