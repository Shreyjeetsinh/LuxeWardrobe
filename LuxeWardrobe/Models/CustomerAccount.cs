using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeWardrobe.Models
{
    public class CustomerAccount
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, StringLength(150)]
        [Index(IsUnique = true)]
        public string Email { get; set; }

        [Required, StringLength(256)]
        public string PasswordHash { get; set; }

        [Required, StringLength(128)]
        public string PasswordSalt { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
