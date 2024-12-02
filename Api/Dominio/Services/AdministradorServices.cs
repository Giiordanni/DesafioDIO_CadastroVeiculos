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

        public void CadastrarAdm(Administrador adm)
        {
            _contexto.Administradores.Add(adm);
            _contexto.SaveChanges();
        }

        public List<Administrador> GetAllAdm(int? pagina)
        {
            var query = _contexto.Administradores.AsQueryable();

            int itensPorPagina = 10;

            if(pagina != null){
                 query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();
        }

        public Administrador GetById(int Id)
        {
           return _contexto.Administradores.Find(Id);
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(adm => adm.Email == loginDTO.Email && adm.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }
    }
}