using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class MenuItem : AggregateRoot<int>
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
