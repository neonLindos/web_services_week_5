using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using TaskManagerClient.Models;

namespace TaskManagerClient.Api;

// Учебный внешний Web API: отдельный HTTP-сервер на адресе Api:BaseAddress
// (http://localhost:5100) внутри процесса клиента, поэтому хватает одного dotnet run.
// Данные хранятся в памяти и сбрасываются при перезапуске.
public static class DemoApi
{
    // Поднимает API, если по этому адресу ещё ничего не слушает, и останавливает его вместе с клиентом
    public static void StartIfNeeded(WebApplication client)
    {
        if (!client.Environment.IsDevelopment()) return;

        if (!Uri.TryCreate(client.Configuration["Api:BaseAddress"], UriKind.Absolute, out var address)
            || !address.IsLoopback)
            return;

        if (IsListening(address)) return;   // внешний API уже запущен — используем его

        var api = Create(address);
        api.Start();
        client.Lifetime.ApplicationStopping.Register(() => api.StopAsync().GetAwaiter().GetResult());
    }

    // Собирает API; customize нужен проверкам, чтобы подменить сервер на тестовый
    public static WebApplication Create(Uri address, Action<WebApplicationBuilder>? customize = null)
    {
        var builder = WebApplication.CreateBuilder();
        customize?.Invoke(builder);
        var app = builder.Build();

        // Адрес клиента (ASPNETCORE_URLS) не должен занять сервер API
        app.Urls.Clear();
        app.Urls.Add(address.GetLeftPart(UriPartial.Authority));

        var items = new List<TaskItem>
        {
            new() { Id = 1, Title = "Изучить HttpClient", Description = "Отправить GET-запрос", IsCompleted = true },
            new() { Id = 2, Title = "Реализовать Web API", Description = "Добавить CRUD" },
            new() { Id = 3, Title = "Изучить PATCH", Description = "Разобрать частичное обновление" }
        };
        var sync = new object();
        var api = app.MapGroup("/api/tasks");

        // GET /api/tasks
        api.MapGet("", () =>
        {
            lock (sync) return Results.Ok(items.ToList());
        });

        // GET /api/tasks/{id}
        api.MapGet("{id:int}", (int id) =>
        {
            lock (sync)
            {
                var item = items.FirstOrDefault(x => x.Id == id);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
        });

        // POST /api/tasks: Id назначает API
        api.MapPost("", (TaskItem item) =>
        {
            if (!IsValid(item)) return Results.BadRequest("Данные не прошли проверку.");
            lock (sync)
            {
                item.Id = items.Count == 0 ? 1 : items.Max(x => x.Id) + 1;
                items.Add(item);
                return Results.Created($"/api/tasks/{item.Id}", item);
            }
        });

        // PUT /api/tasks/{id}
        api.MapPut("{id:int}", (int id, TaskItem item) =>
        {
            if (id != item.Id) return Results.BadRequest("ID маршрута и тела запроса различаются.");
            if (!IsValid(item)) return Results.BadRequest("Данные не прошли проверку.");
            lock (sync)
            {
                var index = items.FindIndex(x => x.Id == id);
                if (index < 0) return Results.NotFound();
                items[index] = item;
                return Results.NoContent();
            }
        });

        // DELETE /api/tasks/{id}
        api.MapDelete("{id:int}", (int id) =>
        {
            lock (sync)
            {
                var item = items.FirstOrDefault(x => x.Id == id);
                if (item is null) return Results.NotFound();
                items.Remove(item);
                return Results.NoContent();
            }
        });

        return app;
    }

    private static bool IsValid(object item) =>
        Validator.TryValidateObject(item, new ValidationContext(item), new List<ValidationResult>(), true);

    private static bool IsListening(Uri address)
    {
        try
        {
            using var tcp = new TcpClient();
            return tcp.ConnectAsync(address.Host, address.Port).Wait(300) && tcp.Connected;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
