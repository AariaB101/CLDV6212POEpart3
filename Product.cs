using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;

namespace FunctionsPOE
{
    public class Product : ITableEntity
    {
        [Key]
        public int ProductId { get; set; }

        [MaxLength(200)]
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }

        // Image stored in Azure Blob Storage
        public string? ImageUrl { get; set; }

        // Table storage keys
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
