using System.ComponentModel.DataAnnotations;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.ViewModels
{
    public class TrackOrderViewModel
    {
        [Required(ErrorMessage = "Order number is required.")]
        [Display(Name = "Order number")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        public Order Order { get; set; }
        public bool SearchPerformed { get; set; }
    }
}