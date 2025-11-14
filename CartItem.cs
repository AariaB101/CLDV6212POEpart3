using Azure;
using Azure.Data.Tables;

namespace CLDV6212PoePart3.Models
{
    public class CartItem : ITableEntity
    {
        // Required for Table Storage
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Your cart properties
        public string UserId { get; set; }      // User identifier (string)
        public string ProductId { get; set; }   // Product identifier (string)
        public int Quantity { get; set; } = 1;
    }
}
