using System;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class Shift : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalBills { get; set; }
    }
}
