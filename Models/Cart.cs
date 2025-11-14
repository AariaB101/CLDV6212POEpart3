using System;
using System.Collections.Generic;

namespace CLDV6212PoePart3.Models
{
    public class Cart
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }   // Links cart to the customer
        public Customer Customer { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsOrderPlaced { get; set; } = false; // True when order is confirmed
        public string Status { get; set; } = "Pending";  // "Pending", "Processed"
    }
}
