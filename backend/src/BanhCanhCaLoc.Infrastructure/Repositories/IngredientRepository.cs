using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Domain.Entities;
using BanhCanhCaLoc.Infrastructure.Persistence;

namespace BanhCanhCaLoc.Infrastructure.Repositories
{
    public class IngredientRepository : GenericRepository<Ingredient, int>, IIngredientRepository
    {
        public IngredientRepository(BanhCanhCaLocDbContext context) : base(context)
        {
        }
    }
}
