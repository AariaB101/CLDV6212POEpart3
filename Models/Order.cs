using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CLDV6212PoePart3.Models
{
    public class Order : ITableEntity
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        // Required for Table Storage
        public string PartitionKey { get; set; } = "Order"; // can be "Order" or user-specific
        public string RowKey { get; set; } = Guid.NewGuid().ToString(); // unique ID for each order

        [JsonIgnore] public ETag ETag { get; set; }
        [JsonIgnore] public DateTimeOffset? Timestamp { get; set; }

        // User who made the order
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        // Optional: product and quantity for simple order tracking
        [Required(ErrorMessage = "ProductId is required.")]
        public string ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; } = 1;

        // Optional: order status
        public string Status { get; set; } = "Pending";
    }
}
