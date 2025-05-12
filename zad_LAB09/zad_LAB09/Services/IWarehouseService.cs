using zad_LAB09.Models;
using zad_LAB09.Models.DTOs;

namespace zad_LAB09.Services;

public interface IWarehouseService
{
    public Task<bool> DoesWarehouseExist(int IdWarehouse);
    public Task<bool> IsOrderFulfilled(int IdOrder);
    
    public Task<int> AddProductToWarehouse(AddProductToWarehouseRequest request);
    public Task<int> AddProductToWarehouseProcedure(AddProductToWarehouseRequest request);
}