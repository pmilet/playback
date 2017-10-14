// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using pmilet.Playback;
using System.Threading.Tasks;
using pmilet.Playback.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        MyServiceProxy _serviceProxy;
        public HelloController(IHttpContextAccessor accessor, IPlaybackContext context, IPlaybackStorageService service, MyServiceProxy serviceProxy)
        {
            context.Read(accessor.HttpContext);
            _serviceProxy = serviceProxy;
            _serviceProxy.SetPlayback(context, service);
        }

        // GET api/values
        [HttpGet]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get()
        {
            var v = await _serviceProxy.Execute(new MyServiceRequest() { Command = "Get" });
            return v.Response;
        }

        // GET api/values/5
        [HttpGet("{name}")]
        [SwaggerOperation("Hello")]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<string> Get(string name)
        {
            var v = await _serviceProxy.Execute(new MyServiceRequest() { Command = $"Get {name}" });
            return v.Response;
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
