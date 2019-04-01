using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using pmilet.Playback;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestWebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        readonly IServiceProxy  _serviceProxy;
        public ValuesController( IServiceProxy remoteServiceProxy )
        {
            _serviceProxy = remoteServiceProxy;
        }
       
        [HttpGet]
        [SwaggerOperation("Get")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get()
        {
            var v = await _serviceProxy.Execute(new MyServiceRequest() { Input = "Get" });
            return v.Output;
        }

        [HttpGet("{input}")]
        [SwaggerOperation("Get")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get(string input)
        {
            var v = await _serviceProxy.Execute(new MyServiceRequest() { Input = $"GET { input }" });
            return v.Output;
        }

    }
}
