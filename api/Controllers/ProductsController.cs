using Microsoft.AspNetCore.Mvc;
using DemoApi.Models;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(DemoStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Product>> GetAll()
    {
        lock (store.Sync) return Ok(store.Products.ToList());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        lock (store.Sync)
        {
            var item = store.Products.FirstOrDefault(x => x.Id == id);
            return item is null ? NotFound() : Ok(item);
        }
    }

    [HttpPost]
    public ActionResult<Product> Create(Product item)
    {
        lock (store.Sync)
        {
            item.Id = store.Products.Count == 0 ? 1 : store.Products.Max(x => x.Id) + 1;
            store.Products.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Product item)
    {
        if (id != item.Id) return BadRequest("ID маршрута и тела запроса различаются.");
        lock (store.Sync)
        {
            var index = store.Products.FindIndex(x => x.Id == id);
            if (index < 0) return NotFound();
            store.Products[index] = item;
            return NoContent();
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        lock (store.Sync)
        {
            var item = store.Products.FirstOrDefault(x => x.Id == id);
            if (item is null) return NotFound();
            store.Products.Remove(item);
            return NoContent();
        }
    }
}
