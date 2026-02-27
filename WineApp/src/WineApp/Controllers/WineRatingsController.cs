using Microsoft.AspNetCore.Mvc;
using WineApp.Data;
using WineApp.Models;

namespace WineApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WineRatingsController : ControllerBase
{
    private readonly IWineRatingRepository _repo;

    public WineRatingsController(IWineRatingRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IEnumerable<WineRating>> Get() => await _repo.GetAllWineRatingsAsync();

    [HttpGet("{id}", Name = "GetWineRatingByIdRoute")]
    public async Task<ActionResult<WineRating>> Get(string id)
    {
        var rating = await _repo.GetWineRatingByIdAsync(id);
        return rating is null ? NotFound() : Ok(rating);
    }

    [HttpPost]
    public async Task<IActionResult> Post(WineRating wineRating)
    {
        var wineRatingId = await _repo.AddWineRatingAsync(wineRating);
        return CreatedAtRoute("GetWineRatingByIdRoute", new { id = wineRatingId }, wineRating);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (await _repo.GetWineRatingByIdAsync(id) is null) return NotFound();
        await _repo.DeleteWineRatingAsync(id);
        return NoContent();
    }
}
