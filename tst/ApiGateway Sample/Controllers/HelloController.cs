// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using pmilet.Playback;
using System.Threading.Tasks;
using System.Net;

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

        /// <summary>
        /// GET Hello
        /// </summary>
        /// <returns>Hello</returns>
        // GET api/hello
        [HttpGet]
        [SwaggerOperation("Get Hello")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(string))]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<IActionResult> Get()
        {
            var response = await _serviceProxy.Execute(new MyServiceRequest() { Input = "Get" });
            return new OkObjectResult(response.Output);
        }

        /// <summary>
        /// GET Hello by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>MyService received input: {name}</returns>
        // GET api/hello/5
        [HttpGet("{name}", Name = "GetByName")]
        [SwaggerOperation("Get Hello by name")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(string))]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<IActionResult> GetByName(string name)
        {
            var response = await _serviceProxy.Execute(new MyServiceRequest() { Input = $"Get {name}" });
            return new OkObjectResult(response.Output);
        }

        /// <summary>
        /// POST Hello
        /// </summary>
        /// <param name="request">Name</param>
        /// <returns>Created. Route new hello</returns>
        // POST api/hello
        [HttpPost]
        [SwaggerOperation("Post Hello")]
        [SwaggerResponse((int)HttpStatusCode.Created, typeof(string))]
        [SwaggerOperationFilter(typeof(PlaybackSwaggerFilter))]
        public async Task<IActionResult> Post([FromBody]HelloRequest request)
        {
            var response = await _serviceProxy.Execute(new MyServiceRequest() { Input = $"Post {request.Name}" });
            return new CreatedAtRouteResult("GetByName", new { name = request.Name }, response);
        }        
    }
}
