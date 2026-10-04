using System.ComponentModel.DataAnnotations;

namespace LuxeWardrobe.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
