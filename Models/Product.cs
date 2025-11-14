using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CLDV6212PoePart3.Models
{
    public class Product : ITableEntity
    {
        [Key] public int ProductId { get; set; }


        [MaxLength(200)]
        public String? ProductName { get; set; }
        public String? Description { get; set; }
        public double? Price { get; set; }

        // Blob URL (image hosted in Azure Blob Storage)
        public string? ImageUrl { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }

        [JsonIgnore]
        [NotMapped]
        public ETag ETag { get; set; }

        [JsonIgnore]
        [NotMapped]
        public DateTimeOffset? Timestamp { get; set; }
    }
}

