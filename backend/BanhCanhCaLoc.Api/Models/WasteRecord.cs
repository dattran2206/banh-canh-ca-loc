using System;
using System.ComponentModel.DataAnnotations;

namespace BanhCanhCaLoc.Api.Models
{
    public class WasteRecord
    {
        public Guid Id { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public double Quantity { get; set; }

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
