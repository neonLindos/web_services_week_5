using Microsoft.AspNetCore.Mvc;
using DemoApi.Models;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(DemoStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Student>> GetAll()
    {
        lock (store.Sync) return Ok(store.Students.ToList());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Student> GetById(int id)
    {
        lock (store.Sync)
        {
            var item = store.Students.FirstOrDefault(x => x.Id == id);
            return item is null ? NotFound() : Ok(item);
        }
    }

    [HttpPost]
    public ActionResult<Student> Create(Student item)
    {
        lock (store.Sync)
        {
            item.Id = store.Students.Count == 0 ? 1 : store.Students.Max(x => x.Id) + 1;
            store.Students.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Student item)
    {
        if (id != item.Id) return BadRequest("ID маршрута и тела запроса различаются.");
        lock (store.Sync)
        {
            var index = store.Students.FindIndex(x => x.Id == id);
            if (index < 0) return NotFound();
            store.Students[index] = item;
            return NoContent();
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        lock (store.Sync)
        {
            var item = store.Students.FirstOrDefault(x => x.Id == id);
            if (item is null) return NotFound();
            store.Students.Remove(item);
            return NoContent();
        }
    }
}
