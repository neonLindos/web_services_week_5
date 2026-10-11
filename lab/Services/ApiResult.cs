namespace WebServiceClients;

public record ApiResult<T>(T? Data, string? Error = null)
{
    public bool Success => Error is null;
}
