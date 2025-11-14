using Azure;
using Azure.Data.Tables;
using CLDV6212PoePart3.Models;

namespace CLDV6212PoePart3.Services
{
    public class TableStorageService
    {
        public readonly TableClient _productTableClient;
        public readonly TableClient _customerTableClient;
        public readonly TableClient _orderTableClient;

        public TableStorageService(string connectionString)
        {
            _productTableClient = new TableClient(connectionString, "products");
            _customerTableClient = new TableClient(connectionString, "customer");
            _orderTableClient = new TableClient(connectionString, "orders");
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey))
                product.PartitionKey = "Product";

            if (string.IsNullOrEmpty(product.RowKey))
                product.RowKey = Guid.NewGuid().ToString();

            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();

            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey))
                customer.PartitionKey = "Customer";

            if (string.IsNullOrEmpty(customer.RowKey))
                customer.RowKey = Guid.NewGuid().ToString();

            try
            {
                await _customerTableClient.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task AddOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.PartitionKey))
                order.PartitionKey = "Order";

            if (string.IsNullOrEmpty(order.RowKey))
                order.RowKey = Guid.NewGuid().ToString();

            try
            {
                await _orderTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table Storage", ex);
            }
        }

        public async Task<List<Order>> GetAllOrderAsync()
        {
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var products = await GetAllProductsAsync();
            return products.FirstOrDefault(p => p.ProductId == productId);
        }


    }
}
