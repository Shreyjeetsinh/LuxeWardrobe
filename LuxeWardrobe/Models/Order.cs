using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeWardrobe.Models
{
    public class Order
    {
        public Order()
        {
            Items = new HashSet<OrderItem>();
        }

        public int Id { get; set; }

        public int? CustomerId { get; set; }

        [Required, StringLength(30)]
        [Index(IsUnique = true)]
        public string OrderNumber { get; set; }

        [Required, StringLength(100)]
        public string CustomerName { get; set; }

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required, Phone, StringLength(20)]
        public string Phone { get; set; }

        [Required, StringLength(250)]
        public string AddressLine1 { get; set; }

        [StringLength(250)]
        public string AddressLine2 { get; set; }

        [Required, StringLength(80)]
        public string City { get; set; }

        [Required, StringLength(80)]
        public string State { get; set; }

        [Required, StringLength(12)]
        public string PostalCode { get; set; }

        [Required, StringLength(30)]
        public string Status { get; set; }

        [StringLength(30)]
        public string PromoCode { get; set; }

        [Column(TypeName = "money")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "money")]
        public decimal Discount { get; set; }

        [Column(TypeName = "money")]
        public decimal DeliveryFee { get; set; }

        [Column(TypeName = "money")]
        public decimal Total { get; set; }

        public DateTime OrderedAtUtc { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; }
    }
}
