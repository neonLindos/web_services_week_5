using ProductClient.Models;
using WebServiceClients;

namespace ProductClient.Services;

public class ProductApiService : ApiService<Product>, IProductApiService
{
    public ProductApiService(IHttpClientFactory factory)
        : base(factory.CreateClient("ProductApi"), "api/products", "Товар не найден.") { }

    public Task<ApiResult<bool>> UpdateAsync(Product item) => UpdateAsync(item.Id, item);
}
