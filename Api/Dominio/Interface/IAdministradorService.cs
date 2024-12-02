using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entities;

namespace minimal_api.Dominio.Interface
{
    public interface IAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);

        void CadastrarAdm(Administrador adm);

        List<Administrador> GetAllAdm(int? pagina);

        Administrador GetById(int Id);
    }
}