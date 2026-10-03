using ProductClient.Models;
using WebServiceClients;

namespace ProductClient.Services;

public interface IProductApiService
{
    Task<ApiResult<List<Product>>> GetAllAsync();
    Task<ApiResult<Product>> GetByIdAsync(int id);
    Task<ApiResult<bool>> CreateAsync(Product item);
    Task<ApiResult<bool>> UpdateAsync(Product item);
    Task<ApiResult<bool>> DeleteAsync(int id);
}
