using System;
using System.ComponentModel.DataAnnotations;

namespace BanhCanhCaLoc.Api.Models
{
    public class StockEntry
    {
        public Guid Id { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public double Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }
    }
}
