using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class WasteRecord : BaseEntity<Guid>
    {
        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        public double Quantity { get; set; }

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
