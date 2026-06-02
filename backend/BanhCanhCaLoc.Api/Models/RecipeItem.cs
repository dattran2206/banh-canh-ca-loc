namespace BanhCanhCaLoc.Api.Models
{
    public class RecipeItem
    {
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public double Quantity { get; set; }

        public double YieldPercent { get; set; }
    }
}
