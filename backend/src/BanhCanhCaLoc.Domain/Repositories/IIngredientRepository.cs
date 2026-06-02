using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface IIngredientRepository : IGenericRepository<Ingredient, int>
    {
    }
}
