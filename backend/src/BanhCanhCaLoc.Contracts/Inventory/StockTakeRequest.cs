namespace BanhCanhCaLoc.Contracts.Inventory
{
    public record StockTakeRequest(int IngredientId, double ActualQty, string? Note);
}
