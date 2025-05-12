using zad_LAB09.Models;
using zad_LAB09.Models.DTOs;

namespace zad_LAB09.Services;

public interface IOrderService
{
    public Task<Order?> GetMatchingOrder(AddProductToWarehouseRequest request);
}