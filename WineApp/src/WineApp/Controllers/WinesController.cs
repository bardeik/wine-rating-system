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
    public async Task<IEnumerable<Wine>> Get() => await _repo.GetAllWinesAsync();

    [HttpGet("{id}", Name = "GetWineByIdRoute")]
    public async Task<ActionResult<Wine>> Get(string id)
    {
        var wine = await _repo.GetWineByIdAsync(id);
        return wine is null ? NotFound() : Ok(wine);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Wine wine)
    {
        var wineId = await _repo.AddWineAsync(wine);
        return CreatedAtRoute("GetWineByIdRoute", new { id = wineId }, wine);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (await _repo.GetWineByIdAsync(id) is null) return NotFound();
        await _repo.DeleteWineAsync(id);
        return NoContent();
    }
}
