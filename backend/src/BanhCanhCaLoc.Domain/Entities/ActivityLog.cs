using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class ActivityLog : BaseEntity<Guid>
    {
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string Detail { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }
    }
}
