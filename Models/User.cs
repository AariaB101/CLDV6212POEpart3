using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CLDV6212PoePart3.Models
{
    public class User : ITableEntity
    {

        [Key] public int UserId { get; set; }
        // Required for Table Storage
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        [JsonIgnore]
        [NotMapped]
        public ETag ETag { get; set; }

        [JsonIgnore]
        [NotMapped]
        public DateTimeOffset? Timestamp { get; set; }

        // Your custom properties
        public string Username { get; set; }
        public string PasswordHash { get; set; }  // hashed password
        public string Role { get; set; } // "Admin" or "Customer"
    }
}
