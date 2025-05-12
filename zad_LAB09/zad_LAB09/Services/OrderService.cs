using Microsoft.Data.SqlClient;

using zad_LAB09.Models;
using zad_LAB09.Models.DTOs;

namespace zad_LAB09.Services;

public class OrderService : IOrderService
{
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";

    public async Task<Order?> GetMatchingOrder(AddProductToWarehouseRequest request)
    {
        string query = @"
                        SELECT TOP 1 IdOrder, IdProduct, Amount, CreatedAt, FulfilledAt 
                        FROM [Order]
                        WHERE IdProduct = @IdProduct
                            AND Amount = @Amount
                            AND CreatedAt < @CreatedAt
                            AND FulfilledAt IS NULL";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
        cmd.Parameters.AddWithValue("@Amount", request.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
        
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new Order
            {
                IdOrder = reader.GetInt32(reader.GetOrdinal("IdOrder")),
                IdProduct = reader.GetInt32(reader.GetOrdinal("IdProduct")),
                Amount = reader.GetInt32(reader.GetOrdinal("Amount")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                FulfilledAt = reader.IsDBNull(reader.GetOrdinal("FulfilledAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("FulfilledAt"))
            };
        }
        return null;
    }
}