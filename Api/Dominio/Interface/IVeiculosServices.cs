using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entities;

namespace minimal_api.Dominio.Interface
{
    public interface IVeiculosServices
    {
        List<Veiculo> TodosVeiculos(int? pagina = 1, string? nome = null, string?marca = null);
        Veiculo? BuscaPorId(int id);
        void AdicionaVeiculo(Veiculo veiculo);
       void AtualizaVeiculo(Veiculo veiculo);
        void DeletarVeiculo(Veiculo veiculo);
    }
}