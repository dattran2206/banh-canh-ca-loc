using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    /// <summary>
    /// Aggregate Root đại diện cho một đơn order.
    /// Vòng đời trạng thái: confirmed → preparing → ready → paid
    /// </summary>
    public class Order : AggregateRoot<Guid>
    {
        public int TableId { get; set; }
        public Table? Table { get; set; }

        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        // confirmed | preparing | ready | paid
        public string Status { get; set; } = "confirmed";

        public DateTime CreatedAt { get; set; }

        public Guid? ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public System.Collections.Generic.List<OrderItem> Items { get; set; } = new();
    }
}
