namespace zad_LAB09.Services;

public interface IProductService
{
    public Task<bool> DoesProductExist(int IdProduct);
}