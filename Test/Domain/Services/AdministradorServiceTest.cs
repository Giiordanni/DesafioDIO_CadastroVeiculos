using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Services;
using minimal_api.Infraestrutura.Db;

namespace Test.Domain.Services
{
    [TestClass]
    public class AdministradorServiceTest
    {

        private DbContexto CriarContextoDeTeste()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", "..", "Api"));

            var builder = new ConfigurationBuilder()
                .SetBasePath(path ?? Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            return new DbContexto(configuration);
        }

        [TestMethod]
        public void TestandoSalvarAdministrador()
        {
            // Arrange - variáveis
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var administradorServico = new AdministradorServices(context);

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "3";

            // Act
            administradorServico.CadastrarAdm(adm);

            // Assert
            Assert.AreEqual(1, administradorServico.GetAllAdm(1).Count());

        }

        [TestMethod]
        public void TestandoBuscaPorId()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

            var adm = new Administrador();
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "3";

            var administradorServico = new AdministradorServices(context);

            // Act
            administradorServico.CadastrarAdm(adm);
            var admDoBanco = administradorServico.GetById(adm.Id);

            // Assert
            Assert.AreEqual(1, admDoBanco?.Id);
        }
    }
}