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
    public IEnumerable<WineRating> Get() => _repo.GetAllWineRatings();

    [HttpGet("{id}", Name = "GetWineRatingByIdRoute")]
    public ActionResult<WineRating> Get(string id)
    {
        var rating = _repo.GetWineRatingById(id);
        return rating is null ? NotFound() : Ok(rating);
    }

    [HttpPost]
    public IActionResult Post(WineRating wineRating)
    {
        var wineRatingId = _repo.AddWineRating(wineRating);
        return CreatedAtRoute("GetWineRatingByIdRoute", new { id = wineRatingId }, wineRating);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (_repo.GetWineRatingById(id) is null) return NotFound();
        _repo.DeleteWineRating(id);
        return NoContent();
    }
}
