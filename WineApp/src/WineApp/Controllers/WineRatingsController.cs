using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WineApp.Data;
using WineApp.Models;

namespace WineApp.Controllers
{

    [Route("api/[controller]")]
    public class WineRatingsController : Controller
    {
        public IWineRatingRepository Repo { get; set; }

        public WineRatingsController([FromServices] IWineRatingRepository repo)
        {
            Repo = repo;
        }

        // GET api/todos
        [HttpGet]
        public IEnumerable<WineRating> Get()
        {
            return Repo.GetAllWineRatings();
        }

        // GET api/wineratings/2
        [HttpGet("{id}")]
        [Route("{id}", Name = "GetWineRatingByIdRoute")]
        public IActionResult Get(string id)
        {
            var rating = Repo.GetWineRatingById(id);
            if (rating == null)
                return NotFound();
            return Ok(rating);
        }

        [HttpPost]
        public IActionResult Post([FromBody] WineRating wineRating)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var wineRatingId = Repo.AddWineRating(wineRating);
                var url = Url.RouteUrl("GetWineRatingByIdRoute", new { id = wineRatingId }, Request.Scheme,
                    Request.Host.ToUriComponent());
                return Created(url, wineRating);

            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete(string id)
        {
            if (Repo.GetWineRatingById(id) == null) return NotFound();
            Repo.DeleteWineRating(id);
            return new StatusCodeResult(200);
        }
    }   
}
