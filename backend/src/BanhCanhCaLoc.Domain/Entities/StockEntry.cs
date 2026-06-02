using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class StockEntry : BaseEntity<Guid>
    {
        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public double Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }
    }
}
