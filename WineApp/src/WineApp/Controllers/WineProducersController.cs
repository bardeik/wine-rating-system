using Microsoft.AspNetCore.Mvc;
using WineApp.Data;
using WineApp.Models;

namespace WineApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WineProducersController : ControllerBase
{
    private readonly IWineProducerRepository _repo;

    public WineProducersController(IWineProducerRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IEnumerable<WineProducer>> Get() => await _repo.GetAllWineProducersAsync();

    [HttpGet("{id}", Name = "GetWineProducerByIdRoute")]
    public async Task<ActionResult<WineProducer>> Get(string id)
    {
        var producer = await _repo.GetWineProducerByIdAsync(id);
        return producer is null ? NotFound() : Ok(producer);
    }

    [HttpPost]
    public async Task<IActionResult> Post(WineProducer wineProducer)
    {
        var wineProducerId = await _repo.AddWineProducerAsync(wineProducer);
        return CreatedAtRoute("GetWineProducerByIdRoute", new { id = wineProducerId }, wineProducer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (await _repo.GetWineProducerByIdAsync(id) is null) return NotFound();
        await _repo.DeleteWineProducerAsync(id);
        return NoContent();
    }
}
