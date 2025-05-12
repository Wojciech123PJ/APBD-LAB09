using Microsoft.Data.SqlClient;

namespace zad_LAB09.Services;

public class ProductService : IProductService
{
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=yourStrong(!)Password; Initial Catalog=master; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
   
    
    public async Task<bool> DoesProductExist(int IdProduct)
    {
        string query = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
        
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(query, conn);
        
        cmd.Parameters.AddWithValue("@IdProduct", IdProduct);
        
        await conn.OpenAsync();
        
        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }
}