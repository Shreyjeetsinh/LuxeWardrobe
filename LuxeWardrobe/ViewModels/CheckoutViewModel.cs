using System.ComponentModel.DataAnnotations;

namespace LuxeWardrobe.ViewModels
{
    public class CheckoutViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full name")]
        public string CustomerName { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required, Phone, StringLength(20)]
        public string Phone { get; set; }

        [Required, StringLength(250)]
        [Display(Name = "Address")]
        public string AddressLine1 { get; set; }

        [StringLength(250)]
        [Display(Name = "Apartment / landmark (optional)")]
        public string AddressLine2 { get; set; }

        [Required, StringLength(80)]
        public string City { get; set; }

        [Required, StringLength(80)]
        public string State { get; set; }

        [Required, RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Enter a valid 6-digit PIN code.")]
        [Display(Name = "PIN code")]
        public string PostalCode { get; set; }

        public CartViewModel Cart { get; set; }
    }
}
