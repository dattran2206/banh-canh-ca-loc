using BanhCanhCaLoc.Domain.Entities;

namespace BanhCanhCaLoc.Application.Features.Auth.Common
{
    public record AuthResult(User User, string Token);
}
