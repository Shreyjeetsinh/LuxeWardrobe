using System.ComponentModel.DataAnnotations;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.ViewModels
{
    public class TrackOrderViewModel
    {
        [Required, Display(Name = "Order number")]
        public string OrderNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public Order Order { get; set; }
        public bool SearchPerformed { get; set; }
    }
}
