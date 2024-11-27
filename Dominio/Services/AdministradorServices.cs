using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Interface;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Dominio.Services
{
    public class AdministradorServices : IAdministradorService
    {

        private readonly DbContexto _contexto;
        public AdministradorServices(DbContexto db) {
            _contexto = db;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(adm => adm.Email == loginDTO.Email && adm.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }
    }
}