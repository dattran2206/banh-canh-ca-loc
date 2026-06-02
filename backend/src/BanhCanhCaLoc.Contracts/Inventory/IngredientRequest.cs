namespace BanhCanhCaLoc.Contracts.Inventory
{
    public record IngredientRequest(string Name, string Unit, double MinThreshold, double CurrentStock);
}
