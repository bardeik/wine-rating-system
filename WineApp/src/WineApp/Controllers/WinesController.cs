using Microsoft.AspNetCore.Mvc;
using WineApp.Data;
using WineApp.Models;

namespace WineApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WinesController : ControllerBase
{
    private readonly IWineRepository _repo;

    public WinesController(IWineRepository repo) => _repo = repo;

    [HttpGet]
    public IEnumerable<Wine> Get() => _repo.GetAllWines();

    [HttpGet("{id}", Name = "GetWineByIdRoute")]
    public ActionResult<Wine> Get(string id)
    {
        var wine = _repo.GetWineById(id);
        return wine is null ? NotFound() : Ok(wine);
    }

    [HttpPost]
    public IActionResult Post(Wine wine)
    {
        var wineId = _repo.AddWine(wine);
        return CreatedAtRoute("GetWineByIdRoute", new { id = wineId }, wine);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (_repo.GetWineById(id) is null) return NotFound();
        _repo.DeleteWine(id);
        return NoContent();
    }
}
