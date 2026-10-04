using System.Collections.Generic;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.ViewModels
{
    public class CustomerAccountViewModel
    {
        public CustomerAccountViewModel()
        {
            Orders = new List<Order>();
        }

        public CustomerProfileViewModel Profile { get; set; }
        public List<Order> Orders { get; set; }
    }
}
