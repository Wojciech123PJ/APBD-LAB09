using System.Data;
using Microsoft.Data.SqlClient;
using zad_LAB09.Models;
using zad_LAB09.Models.DTOs;

namespace zad_LAB09.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    
    public WarehouseService(IOrderService orderService, IProductService productService)
    {
        _productService = productService;
        _orderService = orderService;
    }
    public async Task<bool> DoesWarehouseExist(int IdWarehouse)
    {
        string query = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@IdWarehouse", IdWarehouse);
        
        await conn.OpenAsync();
        
        // var result = (int?)await cmd.ExecuteScalarAsync();
        // return result > 0;
        
        
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }
    
    public async Task<bool> IsOrderFulfilled(int IdOrder)
    {
        var query = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@IdOrder", IdOrder);
        await conn.OpenAsync();
        
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<int> AddProductToWarehouse(AddProductToWarehouseRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0");
        
        if (!await DoesWarehouseExist(request.IdWarehouse))
            throw new InvalidOperationException($"Warehouse with ID {request.IdWarehouse} doesn't exist.");
        
        if (!await _productService.DoesProductExist(request.IdProduct))
            throw new InvalidOperationException($"Product with ID {request.IdProduct} doesn't exist.");
        
        var order = await _orderService.GetMatchingOrder(request);
        
        if (order == null)
            throw new InvalidOperationException("No matching order was found");
        
        if (await IsOrderFulfilled(order.IdOrder))
            throw new ArgumentException("Order is already fulfilled");
        
        
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // Update FullfilledAt 
            string updateQuery = @"UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = @IdOrder";
            await using (var cmd = new SqlCommand(updateQuery, conn, (SqlTransaction)transaction))
            {
                cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdOrder", order.IdOrder);
                await cmd.ExecuteNonQueryAsync();
            }

            
            // Get price
            decimal price;
            string getPrice = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            await using (var cmd = new SqlCommand(getPrice, conn, (SqlTransaction)transaction))
            {
                cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                price = (decimal)(await cmd.ExecuteScalarAsync() ?? throw new Exception("Product not found."));
            }
            
            
            // Insert into Product_Warehouse
            string insertQuery = @"
                    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    OUTPUT INSERTED.IdProductWarehouse
                    VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
            await using (var cmd = new SqlCommand(insertQuery, conn, (SqlTransaction)transaction))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                cmd.Parameters.AddWithValue("@IdOrder", order.IdOrder);
                cmd.Parameters.AddWithValue("@Amount", request.Amount);
                cmd.Parameters.AddWithValue("@Price", price * request.Amount);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                
                var resultId = (int)await cmd.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return resultId;
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddProductToWarehouseProcedure(AddProductToWarehouseRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0");
        

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        try
        {
            var resultId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(resultId);
        }
        catch (SqlException ex)
        {
            throw new ArgumentException($"Database procedure error: {ex.Message}");
        }
    }
}