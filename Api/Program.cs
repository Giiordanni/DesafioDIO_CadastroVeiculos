using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Enums;
using minimal_api.Dominio.Interface;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Services;
using minimal_api.Infraestrutura.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var Key = builder.Configuration.GetSection("Jwt")["Key"].ToString();
if(string.IsNullOrEmpty(Key)){
    Key = "123456";
}

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
        ValidateAudience = false,
        ValidateIssuer = false,
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorService, AdministradorServices>();
builder.Services.AddScoped<IVeiculosServices, VeiculosServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option => {
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {seu token JWT}"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new string[]{}
        }
    });
});


builder.Services.AddDbContext<DbContexto>(
    options => {
        options.UseMySql(
            builder.Configuration.GetConnectionString("MySql")?.ToString(),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")?.ToString())
        );
    }
);

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(Key)) return string.Empty;
    
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>(){
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

#region Validacao
ErroDeValidacoes validaDTOAdm(AdministradorDTO administradorDTO)
{
    var validacao = new ErroDeValidacoes
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.Mensagens.Add("O Email é obrigatório");
    }

    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.Mensagens.Add("A Senha é obrigatória");
    }

    if (administradorDTO.Senha.Length < 6 || administradorDTO.Senha.Length > 20)
    {
        validacao.Mensagens.Add("A senha deve ter entre 6 e 20 caracteres");
    }

    if (administradorDTO.Perfil == null)
    {
        validacao.Mensagens.Add("O Perfil é obrigatório");
    }

    return validacao;
}
#endregion

#region Login
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService adm) =>
{
    var administrador = adm.Login(loginDTO);
    if (administrador != null)
    {
        string Token = GerarTokenJwt(administrador);
        return Results.Ok(new AdministradorLogado{
            Email = administrador.Email,
            Perfil = administrador.Perfil,
            Token = Token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administradores");
#endregion

#region Cadastrar
app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) => {

    var validacao = validaDTOAdm(administradorDTO);
    if (validacao.Mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador{
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    administradorService.CadastrarAdm(administrador);
    return Results.Created($"/administradores/{administrador.Id}", administrador);

}).WithTags("Administradores");
#endregion

#region GetAll
app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorServico) => {
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.GetAllAdm(pagina);

    foreach(var adm in administradores)
        {
            adms.Add(new AdministradorModelView{
                Id = adm.Id,
                Email = adm.Email,
                Perfil = Enum.TryParse<Perfil>(adm.Perfil, out var perfil) ? perfil : Perfil.Editor
        });
    }
    return Results.Ok(adms);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "3"}).WithTags("Administradores");
#endregion

#region GetId
app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
{
    var administrador = administradorService.GetById(id);
    if (administrador == null) return Results.NotFound();
    return Results.Ok(administrador);
}).RequireAuthorization().WithTags("Administradores");
#endregion

#endregion

#region Veiuclos

#region Validacao
ErroDeValidacoes ValidaDTO(VeiculoDTO veiculoDTO){
    var validacao = new ErroDeValidacoes{
         Mensagens =  new List<string>()
    };

    if(string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.Mensagens.Add("O Nome é obrigatório");
    }

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.Mensagens.Add("A marca é obrigatório");
    }

    if (veiculoDTO.Ano < 1950)
    {
        validacao.Mensagens.Add("O ano do veículo é  muito antigo. Aceitável somente anos superiores a 1950");
    }

    return validacao;
}
#endregion

#region CadastrarVeiculos
app.MapPost("/cadastrar/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculosServices veiculoService) =>
{

    var validacao = ValidaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0){
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoService.AdicionaVeiculo(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "3"}).WithTags("Veiculos");
#endregion

#region GetAllVeiculos
app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculosServices veiculosServices) =>
{
    var veiculos = veiculosServices.TodosVeiculos(pagina);

    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veiculos");
#endregion

#region GetOneVeiculos
app.MapGet("/veiculo/{id}", ([FromRoute] int id, IVeiculosServices veiculosServices) =>
{
    var veiculo = veiculosServices.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");


#endregion

#region UpdateVeiculos
app.MapPatch("/update/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculosServices veiculosServices) =>
{
    var veiculo = veiculosServices.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    var validacao = ValidaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0){
        return Results.BadRequest(validacao);
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculosServices.AtualizaVeiculo(veiculo);

    return Results.Ok(veiculo);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "3"}).WithTags("Veiculos");
#endregion

#region DeleteVeiculos
app.MapDelete("/deletar/veiculo", ([FromQuery] int id, IVeiculosServices veiculosServices) => {
    var veiculo = veiculosServices.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculosServices.DeletarVeiculo(veiculo);
    return Results.NoContent();
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute {Roles = "3"}).WithTags("Veiculos");
#endregion

#endregion

#region APP
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
