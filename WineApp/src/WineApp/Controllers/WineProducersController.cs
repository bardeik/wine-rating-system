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
    public class WineProducersController : Controller
    {
        public IWineProducerRepository Repo { get; set; }

        public WineProducersController([FromServices] IWineProducerRepository repo)
        {
            Repo = repo;
        }

        // GET api/todos
        [HttpGet]
        public IEnumerable<WineProducer> Get()
        {
            return Repo.GetAllWineProducers();
        }

        // GET api/wineproducers/2
        [HttpGet("{id}")]
        [Route("{id}", Name = "GetWineProducerByIdRoute")]
        public IActionResult Get(string id)
        {
            var producer = Repo.GetWineProducerById(id);
            if (producer == null)
                return NotFound();
            return Ok(producer);
        }

        [HttpPost]
        public IActionResult Post([FromBody] WineProducer wineProducer)
        {
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                var wineProducerId = Repo.AddWineProducer(wineProducer);
                var url = Url.RouteUrl("GetWineProducerByIdRoute", new { id = wineProducerId }, Request.Scheme,
                    Request.Host.ToUriComponent());
                return Created(url, wineProducer);

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
            if (Repo.GetWineProducerById(id) == null) return NotFound();
            Repo.DeleteWineProducer(id);
            return new StatusCodeResult(200);
        }
    }
}
