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
    public IEnumerable<WineProducer> Get() => _repo.GetAllWineProducers();

    [HttpGet("{id}", Name = "GetWineProducerByIdRoute")]
    public ActionResult<WineProducer> Get(string id)
    {
        var producer = _repo.GetWineProducerById(id);
        return producer is null ? NotFound() : Ok(producer);
    }

    [HttpPost]
    public IActionResult Post(WineProducer wineProducer)
    {
        var wineProducerId = _repo.AddWineProducer(wineProducer);
        return CreatedAtRoute("GetWineProducerByIdRoute", new { id = wineProducerId }, wineProducer);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (_repo.GetWineProducerById(id) is null) return NotFound();
        _repo.DeleteWineProducer(id);
        return NoContent();
    }
}
