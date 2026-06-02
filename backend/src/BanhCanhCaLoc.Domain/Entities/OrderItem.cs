using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class OrderItem : BaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int Quantity { get; set; }

        [MaxLength(250)]
        public string? Note { get; set; }
    }
}
