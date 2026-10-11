using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

var assertions = 0;
var Web = new JsonSerializerOptions(JsonSerializerDefaults.Web);
void Check(bool condition, string message)
{
    assertions++;
    if (!condition) throw new Exception(message);
}

// TaskManagerClient: реальные сервисы, контролируемые HTTP-ответы, без слушающего порта.
{
    using var handler = new StubHandler();
    using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
    var factory = new StubFactory(client, "TaskApi");
    var service = new TaskManagerClient.Services.TaskApiService(factory);
    var item = new TaskManagerClient.Models.TaskItem { Id = 7, Title = "Проверка", Description = "Текст", IsCompleted = false };
    handler.Body = System.Text.Json.JsonSerializer.Serialize(new[] { item });
    var all = await service.GetAllAsync();
    Check(all.Success && all.Data!.Count == 1 && all.Data[0].Id == 7, "Task: GET списка");
    Check(handler.Method == HttpMethod.Get && handler.Path == "/api/tasks", "GET маршрут");
    handler.Body = System.Text.Json.JsonSerializer.Serialize(item);
    Check((await service.GetByIdAsync(7)).Data!.Id == 7, "GET по ID");
    Check(handler.Path == "/api/tasks/7", "GET ID маршрут");
    handler.Status = HttpStatusCode.Created;
    Check((await service.CreateAsync(item)).Success && handler.Method == HttpMethod.Post, "POST 201");
    var sent = System.Text.Json.JsonSerializer.Deserialize<TaskManagerClient.Models.TaskItem>(handler.RequestBody!, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
    Check(sent!.Id == 7, "JSON тела запроса");
    handler.Status = HttpStatusCode.NoContent;
    handler.Body = "";
    Check((await service.UpdateAsync(item)).Success && handler.Method == HttpMethod.Put && handler.Path!.EndsWith("/7"), "PUT 204");
    Check((await service.DeleteAsync(7)).Success && handler.Method == HttpMethod.Delete && handler.RequestBody is null, "DELETE 204");
    foreach (var status in new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden })
    {
        handler.Status = status;
        Check(!(await service.GetAllAsync()).Success, "Ошибка GET списка");
        Check(!(await service.GetByIdAsync(7)).Success, "Ошибка GET ID");
        Check(!(await service.CreateAsync(item)).Success, "Ошибка POST");
        Check(!(await service.UpdateAsync(item)).Success, "Ошибка PUT");
        Check(!(await service.DeleteAsync(7)).Success, "Ошибка DELETE");
    }
    handler.Status = HttpStatusCode.OK;
    foreach (var body in new[] { "not-json", "null", "", "{}" })
    {
        handler.Body = body;
        Check(!(await service.GetAllAsync()).Success, "Некорректный/пустой JSON");
    }
    foreach (var error in new Exception[] { new HttpRequestException(), new TaskCanceledException() })
    {
        handler.Failure = error;
        Check(!(await service.GetAllAsync()).Success, "Сбой сети GET");
        Check(!(await service.CreateAsync(item)).Success, "Сбой сети POST");
        Check(!(await service.UpdateAsync(item)).Success, "Сбой сети PUT");
        Check(!(await service.DeleteAsync(7)).Success, "Сбой сети DELETE");
    }
    Check(factory.Calls == 1, "Клиент получается из фабрики в конструкторе");
    await CheckEmbeddedApiAsync("tasks", n => TaskManagerClient.Api.DemoApi.Create(new Uri("http://localhost:5100"), n), JsonNode.Parse(JsonSerializer.Serialize(item, Web))!.AsObject(), "title");
    var invalid = new TaskManagerClient.Models.TaskItem();
    Check(!Validator.TryValidateObject(invalid, new ValidationContext(invalid), new List<ValidationResult>(), true), "Пустые поля отклоняются");
}

// ProductClient: реальные сервисы, контролируемые HTTP-ответы, без слушающего порта.
{
    using var handler = new StubHandler();
    using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
    var factory = new StubFactory(client, "ProductApi");
    var service = new ProductClient.Services.ProductApiService(factory);
    var item = new ProductClient.Models.Product { Id = 7, Name = "Проверка", Description = "Текст", Price = 12.50m, Quantity = 3 };
    handler.Body = System.Text.Json.JsonSerializer.Serialize(new[] { item });
    var all = await service.GetAllAsync();
    Check(all.Success && all.Data!.Count == 1 && all.Data[0].Id == 7, "Product: GET списка");
    Check(handler.Method == HttpMethod.Get && handler.Path == "/api/products", "GET маршрут");
    handler.Body = System.Text.Json.JsonSerializer.Serialize(item);
    Check((await service.GetByIdAsync(7)).Data!.Id == 7, "GET по ID");
    Check(handler.Path == "/api/products/7", "GET ID маршрут");
    handler.Status = HttpStatusCode.Created;
    Check((await service.CreateAsync(item)).Success && handler.Method == HttpMethod.Post, "POST 201");
    var sent = System.Text.Json.JsonSerializer.Deserialize<ProductClient.Models.Product>(handler.RequestBody!, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
    Check(sent!.Id == 7, "JSON тела запроса");
    handler.Status = HttpStatusCode.NoContent;
    handler.Body = "";
    Check((await service.UpdateAsync(item)).Success && handler.Method == HttpMethod.Put && handler.Path!.EndsWith("/7"), "PUT 204");
    Check((await service.DeleteAsync(7)).Success && handler.Method == HttpMethod.Delete && handler.RequestBody is null, "DELETE 204");
    foreach (var status in new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden })
    {
        handler.Status = status;
        Check(!(await service.GetAllAsync()).Success, "Ошибка GET списка");
        Check(!(await service.GetByIdAsync(7)).Success, "Ошибка GET ID");
        Check(!(await service.CreateAsync(item)).Success, "Ошибка POST");
        Check(!(await service.UpdateAsync(item)).Success, "Ошибка PUT");
        Check(!(await service.DeleteAsync(7)).Success, "Ошибка DELETE");
    }
    handler.Status = HttpStatusCode.OK;
    foreach (var body in new[] { "not-json", "null", "", "{}" })
    {
        handler.Body = body;
        Check(!(await service.GetAllAsync()).Success, "Некорректный/пустой JSON");
    }
    foreach (var error in new Exception[] { new HttpRequestException(), new TaskCanceledException() })
    {
        handler.Failure = error;
        Check(!(await service.GetAllAsync()).Success, "Сбой сети GET");
        Check(!(await service.CreateAsync(item)).Success, "Сбой сети POST");
        Check(!(await service.UpdateAsync(item)).Success, "Сбой сети PUT");
        Check(!(await service.DeleteAsync(7)).Success, "Сбой сети DELETE");
    }
    Check(factory.Calls == 1, "Клиент получается из фабрики в конструкторе");
    await CheckEmbeddedApiAsync("products", n => ProductClient.Api.DemoApi.Create(new Uri("http://localhost:5100"), n), JsonNode.Parse(JsonSerializer.Serialize(item, Web))!.AsObject(), "name");
    var invalid = new ProductClient.Models.Product();
    Check(!Validator.TryValidateObject(invalid, new ValidationContext(invalid), new List<ValidationResult>(), true), "Пустые поля отклоняются");
}

// StudentClient: реальные сервисы, контролируемые HTTP-ответы, без слушающего порта.
{
    using var handler = new StubHandler();
    using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
    var factory = new StubFactory(client, "StudentApi");
    var service = new StudentClient.Services.StudentApiService(factory);
    var item = new StudentClient.Models.Student { Id = 7, FullName = "Проверка", Group = "SE-301", Course = 3, Email = "test@example.com" };
    handler.Body = System.Text.Json.JsonSerializer.Serialize(new[] { item });
    var all = await service.GetAllAsync();
    Check(all.Success && all.Data!.Count == 1 && all.Data[0].Id == 7, "Student: GET списка");
    Check(handler.Method == HttpMethod.Get && handler.Path == "/api/students", "GET маршрут");
    handler.Body = System.Text.Json.JsonSerializer.Serialize(item);
    Check((await service.GetByIdAsync(7)).Data!.Id == 7, "GET по ID");
    Check(handler.Path == "/api/students/7", "GET ID маршрут");
    handler.Status = HttpStatusCode.Created;
    Check((await service.CreateAsync(item)).Success && handler.Method == HttpMethod.Post, "POST 201");
    var sent = System.Text.Json.JsonSerializer.Deserialize<StudentClient.Models.Student>(handler.RequestBody!, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
    Check(sent!.Id == 7, "JSON тела запроса");
    handler.Status = HttpStatusCode.NoContent;
    handler.Body = "";
    Check((await service.UpdateAsync(item)).Success && handler.Method == HttpMethod.Put && handler.Path!.EndsWith("/7"), "PUT 204");
    Check((await service.DeleteAsync(7)).Success && handler.Method == HttpMethod.Delete && handler.RequestBody is null, "DELETE 204");
    foreach (var status in new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden })
    {
        handler.Status = status;
        Check(!(await service.GetAllAsync()).Success, "Ошибка GET списка");
        Check(!(await service.GetByIdAsync(7)).Success, "Ошибка GET ID");
        Check(!(await service.CreateAsync(item)).Success, "Ошибка POST");
        Check(!(await service.UpdateAsync(item)).Success, "Ошибка PUT");
        Check(!(await service.DeleteAsync(7)).Success, "Ошибка DELETE");
    }
    handler.Status = HttpStatusCode.OK;
    foreach (var body in new[] { "not-json", "null", "", "{}" })
    {
        handler.Body = body;
        Check(!(await service.GetAllAsync()).Success, "Некорректный/пустой JSON");
    }
    foreach (var error in new Exception[] { new HttpRequestException(), new TaskCanceledException() })
    {
        handler.Failure = error;
        Check(!(await service.GetAllAsync()).Success, "Сбой сети GET");
        Check(!(await service.CreateAsync(item)).Success, "Сбой сети POST");
        Check(!(await service.UpdateAsync(item)).Success, "Сбой сети PUT");
        Check(!(await service.DeleteAsync(7)).Success, "Сбой сети DELETE");
    }
    Check(factory.Calls == 1, "Клиент получается из фабрики в конструкторе");
    await CheckEmbeddedApiAsync("students", n => StudentClient.Api.DemoApi.Create(new Uri("http://localhost:5100"), n), JsonNode.Parse(JsonSerializer.Serialize(item, Web))!.AsObject(), "fullName");
    var invalid = new StudentClient.Models.Student();
    Check(!Validator.TryValidateObject(invalid, new ValidationContext(invalid), new List<ValidationResult>(), true), "Пустые поля отклоняются");
}

// Встроенный API работы: настоящие маршруты и проверки на тестовом сервере в памяти, без порта.
async Task CheckEmbeddedApiAsync(string route, Func<Action<WebApplicationBuilder>?, WebApplication> create, JsonObject valid, string field)
{
    // Адрес клиента из ASPNETCORE_URLS не должен занять сервер API
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://localhost:5101");
    await using (var real = create(null))
        Check(real.Urls.SequenceEqual(new[] { "http://localhost:5100" }), $"{route}: API слушает только адрес из Api:BaseAddress");
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", null);

    await using var app = create(b => { b.WebHost.UseTestServer(); b.Logging.ClearProviders(); });
    await app.StartAsync();
    using var http = app.GetTestClient();
    var path = $"/api/{route}";

    var list = await http.GetFromJsonAsync<JsonArray>(path);
    Check(list!.Count == 3, $"{route}: три начальные записи");
    var first = list[0]!["id"]!.GetValue<int>();
    Check((await http.GetAsync($"{path}/{first}")).StatusCode == HttpStatusCode.OK, $"{route}: GET по ID");
    Check((await http.GetAsync($"{path}/999999")).StatusCode == HttpStatusCode.NotFound, $"{route}: GET 404");

    var created = await http.PostAsJsonAsync(path, valid);
    Check(created.StatusCode == HttpStatusCode.Created, $"{route}: POST 201");
    var item = (await created.Content.ReadFromJsonAsync<JsonObject>())!;
    var id = item["id"]!.GetValue<int>();
    Check(id == 4, $"{route}: Id назначает API, а не клиент");
    Check(created.Headers.Location!.OriginalString == $"{path}/{id}", $"{route}: заголовок Location");

    Check((await http.PostAsJsonAsync(path, new JsonObject())).StatusCode == HttpStatusCode.BadRequest, $"{route}: POST пустых данных 400");
    Check((await http.PostAsync(path, new StringContent("не json", System.Text.Encoding.UTF8, "application/json"))).StatusCode == HttpStatusCode.BadRequest, $"{route}: POST неверного JSON 400");

    item[field] = "Изменено";
    Check((await http.PutAsJsonAsync($"{path}/{id + 1}", item)).StatusCode == HttpStatusCode.BadRequest, $"{route}: PUT несовпадение ID 400");
    Check((await http.PutAsJsonAsync($"{path}/{id}", item)).StatusCode == HttpStatusCode.NoContent, $"{route}: PUT 204");
    Check((await http.GetFromJsonAsync<JsonObject>($"{path}/{id}"))![field]!.GetValue<string>() == "Изменено", $"{route}: PUT сохранил изменение");
    var missing = item.DeepClone().AsObject(); missing["id"] = 999999;
    Check((await http.PutAsJsonAsync($"{path}/999999", missing)).StatusCode == HttpStatusCode.NotFound, $"{route}: PUT отсутствующего ID 404");
    var invalid = item.DeepClone().AsObject(); invalid[field] = "";
    Check((await http.PutAsJsonAsync($"{path}/{id}", invalid)).StatusCode == HttpStatusCode.BadRequest, $"{route}: PUT пустого поля 400");

    Check((await http.DeleteAsync($"{path}/{id}")).StatusCode == HttpStatusCode.NoContent, $"{route}: DELETE 204");
    Check((await http.GetAsync($"{path}/{id}")).StatusCode == HttpStatusCode.NotFound, $"{route}: запись исчезла");
    Check((await http.DeleteAsync($"{path}/{id}")).StatusCode == HttpStatusCode.NotFound, $"{route}: DELETE повторно 404");
    await app.StopAsync();
}

Console.WriteLine($"Проверки пройдены: {assertions}. Веб-серверы не запускались.");

sealed class StubFactory(HttpClient client, string expectedName) : IHttpClientFactory
{
    public int Calls { get; private set; }
    public HttpClient CreateClient(string name)
    {
        if (name != expectedName) throw new Exception("Неверное имя клиента");
        Calls++;
        return client;
    }
}

sealed class StubHandler : HttpMessageHandler
{
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public string Body { get; set; } = "[]";
    public Exception? Failure { get; set; }
    public HttpMethod? Method { get; private set; }
    public string? Path { get; private set; }
    public string? RequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Method = request.Method;
        Path = request.RequestUri!.AbsolutePath;
        RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        if (Failure is not null) throw Failure;
        return new HttpResponseMessage(Status) { Content = new StringContent(Body, System.Text.Encoding.UTF8, "application/json") };
    }
}
