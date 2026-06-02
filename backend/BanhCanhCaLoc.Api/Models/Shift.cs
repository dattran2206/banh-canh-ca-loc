using System;

namespace BanhCanhCaLoc.Api.Models
{
    public class Shift
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalBills { get; set; }
    }
}
