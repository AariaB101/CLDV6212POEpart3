using System.Net;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using CLDV6212PoePart3.Models;
using Functions;
using FunctionsPOE;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;


namespace QueueFunctions
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly string _storageConnection;
        private readonly TableClient _customer;
        private readonly TableClient _product;
        private readonly TableClient _order;
        private readonly TableClient _userTable;
        private readonly TableClient _login;
        private readonly TableClient _cart;
        private readonly TableClient _orderDetails;
       
        private readonly BlobContainerClient _blobContainerClient;


        public Function1 (ILogger<Function1> logger)
        {
            _logger = logger;

            // ✅ Replace with your real storage account connection string
            _storageConnection = "DefaultEndpointsProtocol=https;AccountName=aaria;AccountKey=8cs2Wf31b6n529u+hjfC5vtVWlPLq7rXPWQHmz/e+vyQs1Djg3wTX2VKOGDRJ5Nhq55GetaEhkcC+AStP1XAZw==;EndpointSuffix=core.windows.net";
            var serviceClient = new TableServiceClient(_storageConnection);

            _customer = serviceClient.GetTableClient("Customer");
            _product = serviceClient.GetTableClient("Product");
            _order = serviceClient.GetTableClient("Orders");
            _userTable = serviceClient.GetTableClient("User");
            _cart = serviceClient.GetTableClient("Cart");
            _orderDetails = serviceClient.GetTableClient("OrderDetails");




            _blobContainerClient = new BlobContainerClient(_storageConnection, "images");
            _blobContainerClient.CreateIfNotExists();
        }


        

        [Function("QueueCustomerSender")]
        public async Task QueueCustomerSender(
            [QueueTrigger("customer-queue", Connection = "connection")] string message)
        {
            _logger.LogInformation("Processing customer queue message.");

            await _customer.CreateIfNotExistsAsync();

            var customer = JsonSerializer.Deserialize<Customer>(message);

            if (customer == null)
            {
                _logger.LogError("Failed to deserialize customer message.");
                return;
            }

            customer.PartitionKey ??= "Customer";
            customer.RowKey ??= Guid.NewGuid().ToString();

            await _customer.AddEntityAsync(customer);

            _logger.LogInformation($"✅ Customer saved: {customer.Name}");
        }

        [Function("QueueProductSender")]
        public async Task QueueProductSender(
            [QueueTrigger("product-queue", Connection = "connection")] string message)
        {
            _logger.LogInformation("Processing product queue message.");

            await _product.CreateIfNotExistsAsync();

            var product = JsonSerializer.Deserialize<Product>(message);

            if (product == null)
            {
                _logger.LogError("Failed to deserialize product message.");
                return;
            }

            product.PartitionKey ??= "Product";
            product.RowKey ??= Guid.NewGuid().ToString();

            await _product.AddEntityAsync(product);

            _logger.LogInformation($"✅ Product saved: {product.ProductName}");
        }

        [Function("QueueOrderSender")]
        public async Task QueueOrderSender(
            [QueueTrigger("order-queue", Connection = "connection")] string message)
        {
            _logger.LogInformation("Processing order queue message.");

            await _order.CreateIfNotExistsAsync();

            var order = JsonSerializer.Deserialize<Order>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (order == null)
            {
                _logger.LogError("Failed to deserialize order message.");
                return;
            }

            order.PartitionKey ??= "Order";
            order.RowKey ??= Guid.NewGuid().ToString();
            order.Timestamp = DateTimeOffset.UtcNow;

            await _order.AddEntityAsync(order);
            _logger.LogInformation($"✅ Order saved for CustomerID {order.UserId} and ProductID {order.ProductId}");
        }

        // ======================================================
        // 🌐 HTTP GET ENDPOINTS
        // ======================================================

        [Function("GetCustomers")]
        public async Task<HttpResponseData> GetCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequestData req)
        {
            try
            {
                var customers = _customer.Query<Customer>().ToList();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(customers);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to query customers.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Failed to retrieve customers.");
                return error;
            }
        }

        [Function("GetProducts")]
        public async Task<HttpResponseData> GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
        {
            try
            {
                var products = _product.Query<Product>().ToList();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(products);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to query products.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Failed to retrieve products.");
                return error;
            }
        }

        [Function("GetOrders")]
        public async Task<HttpResponseData> GetOrders(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")] HttpRequestData req)
        {
            try
            {
                var orders = _order.Query<Order>().ToList();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(orders);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to query orders.");
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Failed to retrieve orders.");
                return error;
            }
        }

        // ======================================================
        // 🟢 HTTP POST ENDPOINTS
        // ======================================================

        [Function("AddCustomer")]
        public async Task<HttpResponseData> AddCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonSerializer.Deserialize<Customer>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (customer == null || string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Email))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid customer data.");
                return bad;
            }

            customer.PartitionKey ??= "Customer";
            customer.RowKey ??= Guid.NewGuid().ToString();

            await _customer.CreateIfNotExistsAsync();
            await _customer.AddEntityAsync(customer);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("✅ Customer added successfully.");
            return response;
        }

        [Function("AddProduct")]
        public async Task<HttpResponseData> AddProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonSerializer.Deserialize<Product>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null || string.IsNullOrEmpty(product.ProductName) || product.Price == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid product data.");
                return bad;
            }

            product.PartitionKey ??= "Product";
            product.RowKey ??= Guid.NewGuid().ToString();

            await _product.CreateIfNotExistsAsync();
            await _product.AddEntityAsync(product);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("✅ Product added successfully.");
            return response;
        }

        [Function("AddOrder")]
        public async Task<HttpResponseData> AddOrder(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<Order>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Validate order data
            if (order == null || string.IsNullOrEmpty(order.UserId) || string.IsNullOrEmpty(order.ProductId))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid order data. UserId and ProductId are required.");
                return bad;
            }

            // Set Partition/Row keys and timestamp
            order.PartitionKey ??= "Order";
            order.RowKey ??= Guid.NewGuid().ToString();
            order.Timestamp = DateTimeOffset.UtcNow;

            await _order.CreateIfNotExistsAsync();
            await _order.AddEntityAsync(order);

            // Also push to order queue
            var queueClient = new QueueClient(_storageConnection, "order-queue");
            await queueClient.CreateIfNotExistsAsync();
            string json = JsonSerializer.Serialize(order);
            await queueClient.SendMessageAsync(json);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("✅ Order added and queued successfully.");
            return response;
        }


        // ======================================================
        // ❌ DELETE ENDPOINTS
        // ======================================================

        [Function("DeleteCustomer")]
        public async Task<HttpResponseData> DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string partitionKey = query["partitionKey"];
            string rowKey = query["rowKey"];

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing partitionKey or rowKey.");
                return bad;
            }

            try
            {
                await _customer.DeleteEntityAsync(partitionKey, rowKey);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("✅ Customer deleted successfully.");
                return response;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Customer not found.");
                return notFound;
            }
        }

        [Function("DeleteProduct")]
        public async Task<HttpResponseData> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string partitionKey = query["partitionKey"];
            string rowKey = query["rowKey"];

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing partitionKey or rowKey.");
                return bad;
            }

            try
            {
                await _product.DeleteEntityAsync(partitionKey, rowKey);
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("✅ Product deleted successfully.");
                return response;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Product not found.");
                return notFound;
            }
        }

        [Function("Login")]
        public async Task<HttpResponseData> Login(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var loginData = JsonSerializer.Deserialize<Login>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginData == null || string.IsNullOrEmpty(loginData.Username) || string.IsNullOrEmpty(loginData.Password))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid login payload.");
                return bad;
            }

            // Ensure the table exists
            await EnsureTableExistsAsync(_userTable);

            CLDV6212PoePart3.Models.User foundUser = null;

            await foreach (var u in _userTable.QueryAsync<CLDV6212PoePart3.Models.User>())
            {
                if (string.Equals(u.Username, loginData.Username, StringComparison.OrdinalIgnoreCase))
                {
                    foundUser = u;
                    break;
                }
            }


            if (foundUser == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid username or password.");
                return bad;
            }

            // Verify the password
            try
            {
                bool valid = BCrypt.Net.BCrypt.Verify(loginData.Password, foundUser.PasswordHash);
                if (!valid)
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteStringAsync("Invalid username or password.");
                    return bad;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password verification failed.");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid username or password.");
                return bad;
            }

            // Return the found user (omit password hash if you like)
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                foundUser.RowKey,
                foundUser.PartitionKey,
                foundUser.Username
                
            });

            return response;
        }

        private async Task EnsureTableExistsAsync(TableClient tableClient)
        {
            if (tableClient != null)
            {
                await tableClient.CreateIfNotExistsAsync();
            }
        }


        private async Task<HttpResponseData> CreateBadRequest(HttpRequestData req, string message)
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync(message);
            return response;
        }


        [Function("AddToCart")]
        public async Task<HttpResponseData> AddToCart([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cart")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var cartItem = JsonSerializer.Deserialize<CartItem>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (cartItem == null || string.IsNullOrEmpty(cartItem.UserId) || string.IsNullOrEmpty(cartItem.ProductId) || cartItem.Quantity <= 0)
                return await CreateBadRequest(req, "Invalid cart item.");

            cartItem.PartitionKey ??= "CartItem";
            cartItem.RowKey ??= Guid.NewGuid().ToString();

            await EnsureTableExistsAsync(_cart);
            await _cart.AddEntityAsync(cartItem);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("Item added to cart.");
            return response;
        }

        [Function("GetCart")]
        public async Task<HttpResponseData> GetCart(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cart/{userId}")] HttpRequestData req,
     string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return await CreateBadRequest(req, "Missing userId.");

            await EnsureTableExistsAsync(_cart);

            // OData filter
            var filter = $"UserId eq '{userId.Replace("'", "''")}'";

            var cartItems = new List<CartItem>();

            await foreach (var item in _cart.QueryAsync<CartItem>(filter))
            {
                cartItems.Add(item);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(cartItems);
            return response;
        }


        [Function("Checkout")]
        public async Task<HttpResponseData> Checkout(
     [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "checkout")] HttpRequestData req)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            // Parse userId from JSON object or plain string
            string userId;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("userId", out var prop))
                {
                    userId = prop.GetString();
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.String)
                {
                    userId = doc.RootElement.GetString();
                }
                else
                {
                    userId = null;
                }
            }
            catch
            {
                userId = body?.Trim('"');
            }

            if (string.IsNullOrEmpty(userId))
                return await CreateBadRequest(req, "Missing userId in checkout payload.");

            await EnsureTableExistsAsync(_cart);

            // Get all cart items for this user
            var filter = $"UserId eq '{userId.Replace("'", "''")}'";
            var cartItems = new List<CartItem>();
            await foreach (var item in _cart.QueryAsync<CartItem>(filter))
                cartItems.Add(item);

            if (!cartItems.Any())
                return await CreateBadRequest(req, "Cart is empty.");

            // Create a new Order entity
            var order = new Order
            {
                PartitionKey = "Order",
                RowKey = Guid.NewGuid().ToString(),
                UserId = userId,
                Timestamp = DateTimeOffset.UtcNow
            };

            await EnsureTableExistsAsync(_order);
            await _order.AddEntityAsync(order);

            // Create order details for each cart item and remove from cart
            await EnsureTableExistsAsync(_orderDetails);
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    PartitionKey = "OrderDetail",
                    RowKey = Guid.NewGuid().ToString(),
                    OrderId = order.RowKey,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };


                await _orderDetails.AddEntityAsync(orderDetail);
                await _cart.DeleteEntityAsync(item.PartitionKey, item.RowKey);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Checkout successful.");
            return response;
        }

    }
}
