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
    public class WinesController : Controller
    {
        public IWineRepository Repo { get; set; }

        public WinesController([FromServices] IWineRepository repo)
        {
            Repo = repo;
        }

        // GET api/todos
        [HttpGet]
        public IEnumerable<Wine> Get()
        {
            return Repo.GetAllWines();
        }

        // GET api/wines/2
        [HttpGet("{id}")]
        [Route("{id}", Name = "GetWineByIdRoute")]
        public IActionResult Get(int id)
        {
            var wine = Repo.GetWineById(id);
            if (wine == null)
                return NotFound();
            return Ok(wine);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Wine wine)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var wineId = Repo.AddWine(wine);
                var url = Url.RouteUrl("GetWineByIdRoute", new { id = wineId }, Request.Scheme,
                    Request.Host.ToUriComponent());
                return Created(url, wine);

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
            try
            {
                var idWine = int.Parse(id, CultureInfo.InvariantCulture);
                if (Repo.GetWineById(idWine) == null) return NotFound();
                Repo.DeleteWine(idWine);
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
