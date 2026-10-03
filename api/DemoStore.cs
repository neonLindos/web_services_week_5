using DemoApi.Models;

namespace DemoApi;

// Общее хранилище между запросами; блокировка защищает выдачу ID и изменения.
public class DemoStore
{
    public object Sync { get; } = new();

    public List<Product> Products { get; } =
    [
        new() { Id = 1, Name = "Notebook", Description = "Ноутбук", Price = 350000, Quantity = 5 },
        new() { Id = 2, Name = "Mouse", Description = "Мышь", Price = 8000, Quantity = 15 },
        new() { Id = 3, Name = "Keyboard", Description = "Клавиатура", Price = 15000, Quantity = 10 }
    ];

    public List<Student> Students { get; } =
    [
        new() { Id = 1, FullName = "Иван Иванов", Group = "SE-301", Course = 3, Email = "ivan@example.com" },
        new() { Id = 2, FullName = "Анна Петрова", Group = "SE-302", Course = 3, Email = "anna@example.com" },
        new() { Id = 3, FullName = "Максим Ким", Group = "SE-301", Course = 3, Email = "maxim@example.com" }
    ];

    public List<TaskItem> Tasks { get; } =
    [
        new() { Id = 1, Title = "Изучить HttpClient", Description = "Отправить GET-запрос", IsCompleted = true },
        new() { Id = 2, Title = "Реализовать Web API", Description = "Добавить CRUD" },
        new() { Id = 3, Title = "Изучить PATCH", Description = "Разобрать частичное обновление" }
    ];
}
