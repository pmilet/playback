using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using pmilet.Playback;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public string Get(int id)
        {
            return $"value {id}";
        }

        // POST api/values
        [HttpPost]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public void Delete(int id)
        {
        }
    }
}
