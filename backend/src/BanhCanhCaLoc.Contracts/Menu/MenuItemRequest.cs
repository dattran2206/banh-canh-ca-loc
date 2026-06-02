namespace BanhCanhCaLoc.Contracts.Menu
{
    public record MenuItemRequest(string Name, int CategoryId, decimal Price, string? Description, bool IsAvailable);
}
