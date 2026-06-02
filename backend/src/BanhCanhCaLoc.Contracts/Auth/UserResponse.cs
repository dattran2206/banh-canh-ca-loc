using System;

namespace BanhCanhCaLoc.Contracts.Auth
{
    public record UserResponse(Guid Id, string Username, string Role, string FullName, bool IsActive);
}
