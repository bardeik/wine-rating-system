using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using skeleton.Data;
using skeleton.Models;

namespace skeleton.Controllers
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

        // GET api/todos/2
        [HttpGet("{id}")]
        [Route("{id}", Name = "GetWineProducerByIdRoute")]
        public WineProducer Get(int id)
        {
            return Repo.GetWineProducerById(id);
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
            catch (Exception ex)
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
                var idWineProducer = int.Parse(id, CultureInfo.InvariantCulture);
                if (Repo.GetWineProducerById(idWineProducer) == null) return NotFound();
                Repo.DeleteWineProducer(idWineProducer);
                return new StatusCodeResult(200);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
