using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;

namespace FunctionsPOE
{
    public class Customer : ITableEntity
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, MaxLength(200)]
        public string? Name { get; set; }
        public string? Email { get; set; }

        // Table storage keys
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
