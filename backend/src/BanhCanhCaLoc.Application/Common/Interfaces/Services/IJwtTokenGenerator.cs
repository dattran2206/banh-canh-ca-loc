using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Common.Interfaces.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
