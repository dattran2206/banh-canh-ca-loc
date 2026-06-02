using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class RecipeItem
    {
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        /// <summary>Số lượng nguyên liệu cần cho 1 phần</summary>
        public double Quantity { get; set; }

        /// <summary>Tỷ lệ hao hụt khi chế biến (0.0 - 1.0)</summary>
        public double YieldPercent { get; set; }
    }
}
