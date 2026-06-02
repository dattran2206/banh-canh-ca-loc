using System;
using System.ComponentModel.DataAnnotations;
using BanhCanhCaLoc.Domain.Common;

namespace BanhCanhCaLoc.Domain.Entities
{
    public class User : AggregateRoot<Guid>
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        // admin | cashier | waiter | kitchen
        public string Role { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
