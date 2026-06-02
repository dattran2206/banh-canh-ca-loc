using System;
using System.Threading;
using System.Threading.Tasks;
using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Domain.Repositories
{
    public interface IUserRepository : IGenericRepository<User, Guid>
    {
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
