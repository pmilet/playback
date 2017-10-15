// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using pmilet.Playback;
using System.Threading.Tasks;

namespace ApiGateway_Sample
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        MyServiceProxy _serviceProxy;
        public HelloController(MyServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;
        }

        // GET api/values
        [HttpGet]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get()
        {
            var v = await _serviceProxy.Execute(new MyServiceRequest() { Input = "Get" });
            return v.Output;
        }

        // GET api/values/5
        [HttpGet("{name}")]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get(string name)
        {
            var v = await _serviceProxy.Execute(new MyServiceRequest() { Input = $"Get {name}" });
            return v.Output;
        }

        // POST api/values
        [HttpPost]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public void Post([FromBody]HelloRequest req)
        {
        }        
    }
}
