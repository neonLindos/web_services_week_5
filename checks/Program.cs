using System.Net;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using DemoApi;

var assertions = 0;
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
    var store = new DemoStore();
    var api = new DemoApi.Controllers.TasksController(store);
    var apiItem = System.Text.Json.JsonSerializer.Deserialize<DemoApi.Models.TaskItem>(System.Text.Json.JsonSerializer.Serialize(item))!;
    var created = (CreatedAtActionResult)api.Create(apiItem).Result!;
    Check(created.StatusCode == 201 && created.ActionName == "GetById", "API POST и Location");
    var id = apiItem.Id;
    Check(((OkObjectResult)api.GetById(id).Result!).Value == apiItem, "API GET созданной записи");
    var updated = new DemoApi.Models.TaskItem { Id = id, Title = "Проверка", Description = "Текст", IsCompleted = false };
    Check(api.Update(id + 1, updated) is BadRequestObjectResult, "API несовпадение ID");
    apiItem.IsCompleted = true;
    Check(api.Update(id, apiItem) is NoContentResult, "API PUT");
    Check(((OkObjectResult)api.GetById(id).Result!).Value == apiItem, "API сохранение изменения");
    Check(api.Delete(id) is NoContentResult, "API DELETE");
    Check(api.GetById(id).Result is NotFoundResult && api.Delete(id) is NotFoundResult, "API 404 после удаления");
    Check(api.Update(id, apiItem) is NotFoundResult, "API PUT отсутствующего ID");
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
    var store = new DemoStore();
    var api = new DemoApi.Controllers.ProductsController(store);
    var apiItem = System.Text.Json.JsonSerializer.Deserialize<DemoApi.Models.Product>(System.Text.Json.JsonSerializer.Serialize(item))!;
    var created = (CreatedAtActionResult)api.Create(apiItem).Result!;
    Check(created.StatusCode == 201 && created.ActionName == "GetById", "API POST и Location");
    var id = apiItem.Id;
    Check(((OkObjectResult)api.GetById(id).Result!).Value == apiItem, "API GET созданной записи");
    var updated = new DemoApi.Models.Product { Id = id, Name = "Проверка", Description = "Текст", Price = 12.50m, Quantity = 3 };
    Check(api.Update(id + 1, updated) is BadRequestObjectResult, "API несовпадение ID");
    apiItem.Price = 25m;
    Check(api.Update(id, apiItem) is NoContentResult, "API PUT");
    Check(((OkObjectResult)api.GetById(id).Result!).Value == apiItem, "API сохранение изменения");
    Check(api.Delete(id) is NoContentResult, "API DELETE");
    Check(api.GetById(id).Result is NotFoundResult && api.Delete(id) is NotFoundResult, "API 404 после удаления");
    Check(api.Update(id, apiItem) is NotFoundResult, "API PUT отсутствующего ID");
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
    var store = new DemoStore();
    var api = new DemoApi.Controllers.StudentsController(store);
    var apiItem = System.Text.Json.JsonSerializer.Deserialize<DemoApi.Models.Student>(System.Text.Json.JsonSerializer.Serialize(item))!;
    var created = (CreatedAtActionResult)api.Create(apiItem).Result!;
    Check(created.StatusCode == 201 && created.ActionName == "GetById", "API POST и Location");
    var id = apiItem.Id;
    Check(((OkObjectResult)api.GetById(id).Result!).Value == apiItem, "API GET созданной записи");
    var updated = new DemoApi.Models.Student { Id = id, FullName = "Проверка", Group = "SE-301", Course = 3, Email = "test@example.com" };
    Check(api.Update(id + 1, updated) is BadRequestObjectResult, "API несовпадение ID");
    apiItem.Course = 4;
    Check(api.Update(id, apiItem) is NoContentResult, "API PUT");
    Check(((OkObjectResult)api.GetById(id).Result!).Value == apiItem, "API сохранение изменения");
    Check(api.Delete(id) is NoContentResult, "API DELETE");
    Check(api.GetById(id).Result is NotFoundResult && api.Delete(id) is NotFoundResult, "API 404 после удаления");
    Check(api.Update(id, apiItem) is NotFoundResult, "API PUT отсутствующего ID");
    var invalid = new StudentClient.Models.Student();
    Check(!Validator.TryValidateObject(invalid, new ValidationContext(invalid), new List<ValidationResult>(), true), "Пустые поля отклоняются");
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
