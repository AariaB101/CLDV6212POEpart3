using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace CLDV6212PoePart3.Models
{
    public class Customer : ITableEntity
    {
        [Key] public int CustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public String? Name { get; set; }
        public String? Email { get; set; }
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