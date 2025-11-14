using Azure.Data.Tables;
using Azure;

public class CartItemEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    public string UserId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
}
