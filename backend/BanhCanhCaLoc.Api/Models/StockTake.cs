using System;
using System.ComponentModel.DataAnnotations;

namespace BanhCanhCaLoc.Api.Models
{
    public class StockTake
    {
        public Guid Id { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public double SystemQty { get; set; }

        public double ActualQty { get; set; }

        public double Difference { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }
    }
}
