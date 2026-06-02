namespace BanhCanhCaLoc.Contracts.Inventory
{
    public record StockEntryRequest(int IngredientId, double Quantity, decimal UnitPrice, string? Note);
}
