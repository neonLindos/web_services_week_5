using Microsoft.AspNetCore.Mvc;
using TaskManagerClient.Models;
using TaskManagerClient.Services;

namespace TaskManagerClient.Controllers;

public class TaskController(ITaskApiService api) : Controller
{
    public async Task<IActionResult> Index()
    {
        var result = await api.GetAllAsync();
        ViewBag.Error = result.Error;
        return View(result.Data ?? new List<TaskItem>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await api.GetByIdAsync(id);
        if (!result.Success) return ApiError(result.Error);
        return View(result.Data);
    }

    public IActionResult Create() => View("Edit", new TaskItem());

    [HttpPost]
    public async Task<IActionResult> Create(TaskItem item)
    {
        item.Id = 0;
        if (!ModelState.IsValid) return View("Edit", item);
        var result = await api.CreateAsync(item);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error!);
            return View("Edit", item);
        }
        TempData["Success"] = "Запись добавлена.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await api.GetByIdAsync(id);
        if (!result.Success) return ApiError(result.Error);
        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, TaskItem item)
    {
        if (id != item.Id) return BadRequest();
        if (!ModelState.IsValid) return View(item);
        var result = await api.UpdateAsync(item);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error!);
            return View(item);
        }
        TempData["Success"] = "Изменения сохранены.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var result = await api.GetByIdAsync(id);
        if (!result.Success) return ApiError(result.Error);
        return View(result.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await api.DeleteAsync(id);
        if (!result.Success) return ApiError(result.Error);
        TempData["Success"] = "Запись удалена.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Error() => ApiError("Произошла ошибка приложения. Повторите попытку.");

    private IActionResult ApiError(string? message)
    {
        ViewBag.Error = message;
        return View("Error");
    }
}
