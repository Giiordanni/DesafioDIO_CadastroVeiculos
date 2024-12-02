using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entities;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class AdministradorTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange - variáveis
            var adm = new Administrador();

            // Act - Setar
            adm.Id = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "123456";
            adm.Perfil = "3";


            // Assert -  validação
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual( "teste@teste.com", adm.Email);
            Assert.AreEqual("123456", adm.Senha);
            Assert.AreEqual("3", adm.Perfil);

        }
    }
}