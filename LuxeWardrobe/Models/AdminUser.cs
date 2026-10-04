using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeWardrobe.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        [Index(IsUnique = true)]
        public string Username { get; set; }

        [Required, StringLength(256)]
        public string PasswordHash { get; set; }

        [Required, StringLength(128)]
        public string PasswordSalt { get; set; }
    }
}
