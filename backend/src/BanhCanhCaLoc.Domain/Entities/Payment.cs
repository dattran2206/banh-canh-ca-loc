using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class Payment : BaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime PaidAt { get; set; }

        [Required]
        [MaxLength(50)]
        // cash | banking | card
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
