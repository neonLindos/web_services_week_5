using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WebServiceClients;

public abstract class ApiService<T>(HttpClient client, string route, string notFound)
{
    public Task<ApiResult<List<T>>> GetAllAsync() => ReadAsync<List<T>>(route);

    public Task<ApiResult<T>> GetByIdAsync(int id) => ReadAsync<T>($"{route}/{id}");

    public Task<ApiResult<bool>> CreateAsync(T item) =>
        WriteAsync(HttpMethod.Post, route, item);

    public Task<ApiResult<bool>> UpdateAsync(int id, T item) =>
        WriteAsync(HttpMethod.Put, $"{route}/{id}", item);

    public Task<ApiResult<bool>> DeleteAsync(int id) =>
        WriteAsync(HttpMethod.Delete, $"{route}/{id}", default);

    private Task<ApiResult<TResult>> ReadAsync<TResult>(string path) => GuardAsync(async () =>
    {
        using var response = await client.GetAsync(path);
        if (!response.IsSuccessStatusCode) return new ApiResult<TResult>(default, Error(response));
        var data = await response.Content.ReadFromJsonAsync<TResult>();
        return data is null
            ? new ApiResult<TResult>(default, "API вернул пустой ответ.")
            : new ApiResult<TResult>(data);
    });

    private Task<ApiResult<bool>> WriteAsync(HttpMethod method, string path, T? item) => GuardAsync(async () =>
    {
        using var request = new HttpRequestMessage(method, path);
        if (method != HttpMethod.Delete) request.Content = JsonContent.Create(item);
        using var response = await client.SendAsync(request);
        return response.IsSuccessStatusCode
            ? new ApiResult<bool>(true)
            : new ApiResult<bool>(false, Error(response));
    });

    private string Error(HttpResponseMessage response) => response.StatusCode switch
    {
        HttpStatusCode.BadRequest => "API отклонил данные (400). Проверьте заполнение полей.",
        HttpStatusCode.NotFound => notFound,
        HttpStatusCode.InternalServerError => "Ошибка сервера API (500). Повторите попытку позже.",
        _ => $"API вернул ошибку {(int)response.StatusCode}. Повторите попытку позже."
    };

    private static async Task<ApiResult<TResult>> GuardAsync<TResult>(Func<Task<ApiResult<TResult>>> operation)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException)
        {
            return new(default, "Не удалось связаться с API. Проверьте адрес и запущен ли сервер.");
        }
        catch (OperationCanceledException)
        {
            return new(default, "API не ответил вовремя. Повторите попытку.");
        }
        catch (JsonException)
        {
            return new(default, "API вернул некорректный JSON или данные другого формата.");
        }
        catch (NotSupportedException)
        {
            return new(default, "Формат ответа API не поддерживается.");
        }
    }
}
