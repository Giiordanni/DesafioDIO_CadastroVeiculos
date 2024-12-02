using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entities;
using minimal_api.Dominio.Interface;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Dominio.Services
{
    public class VeiculosServices : IVeiculosServices
    {

        private readonly DbContexto _contexto;
        public VeiculosServices(DbContexto db) {
            _contexto = db;
        }

        public void AdicionaVeiculo(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public void AtualizaVeiculo(Veiculo veiculo)
        {
           _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void DeletarVeiculo(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo> TodosVeiculos(int? pagina = 1, string nome = null, string marca = null)
        {
            var query = _contexto.Veiculos.AsQueryable();
            if(!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), 
                $"%{nome.ToLower()}%"));
            }

            int itensPorPagina = 10;

            if(pagina != null){
                 query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }

            return query.ToList();
        }
    }
}