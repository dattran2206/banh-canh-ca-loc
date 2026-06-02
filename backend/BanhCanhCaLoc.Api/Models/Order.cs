using System;
using System.ComponentModel.DataAnnotations;

namespace BanhCanhCaLoc.Api.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public int TableId { get; set; }
        public Table? Table { get; set; }

        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty; // pending, confirmed, preparing, ready, paid

        public DateTime CreatedAt { get; set; }

        public Guid? ShiftId { get; set; }
        public Shift? Shift { get; set; }
    }
}
