using Azure;
using Azure.Data.Tables;


namespace CLDV6212PoePart3.Models
{
    public class User : ITableEntity
    {
        // Required for Table Storage
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Your custom properties
        public string Username { get; set; }
        public string PasswordHash { get; set; }  // hashed password
        public string Role { get; set; } // "Admin" or "Customer"
    }
}
