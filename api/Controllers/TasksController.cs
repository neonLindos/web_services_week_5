using Microsoft.AspNetCore.Mvc;
using DemoApi.Models;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController(DemoStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<TaskItem>> GetAll()
    {
        lock (store.Sync) return Ok(store.Tasks.ToList());
    }

    [HttpGet("{id:int}")]
    public ActionResult<TaskItem> GetById(int id)
    {
        lock (store.Sync)
        {
            var item = store.Tasks.FirstOrDefault(x => x.Id == id);
            return item is null ? NotFound() : Ok(item);
        }
    }

    [HttpPost]
    public ActionResult<TaskItem> Create(TaskItem item)
    {
        lock (store.Sync)
        {
            item.Id = store.Tasks.Count == 0 ? 1 : store.Tasks.Max(x => x.Id) + 1;
            store.Tasks.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, TaskItem item)
    {
        if (id != item.Id) return BadRequest("ID маршрута и тела запроса различаются.");
        lock (store.Sync)
        {
            var index = store.Tasks.FindIndex(x => x.Id == id);
            if (index < 0) return NotFound();
            store.Tasks[index] = item;
            return NoContent();
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        lock (store.Sync)
        {
            var item = store.Tasks.FirstOrDefault(x => x.Id == id);
            if (item is null) return NotFound();
            store.Tasks.Remove(item);
            return NoContent();
        }
    }
}
