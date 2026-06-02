namespace BanhCanhCaLoc.Contracts.Orders
{
    public record CreateOrderItemRequest(int MenuItemId, int Quantity, string? Note);
}
