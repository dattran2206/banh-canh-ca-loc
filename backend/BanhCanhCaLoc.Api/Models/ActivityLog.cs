using System;
using System.ComponentModel.DataAnnotations;

namespace BanhCanhCaLoc.Api.Models
{
    public class ActivityLog
    {
        public Guid Id { get; set; }

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
