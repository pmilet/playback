using pmilet.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ApiGateway
{
    public class HelloRequest
    {
        public string Nombre { get; set; }
    }

    public class MyFakeFactory : FakeFactoryBase
    {
        private string HelloGet(string request)
        {
            return "Hello FAKE";
        }

        private string HelloPost(HelloRequest request)
        {
            var name = !string.IsNullOrEmpty(request.Nombre) ? request.Nombre : "Whoever";
            return "Hello " + name + " FAKE";
        }

        public override void GenerateFakeResponse(HttpContext context)
        {
            switch (context.Request.Path.Value.ToLower())
            {

                case "/api/values":
                    if (context.Request.Method == "POST")
                        GenerateFakeResponse<HelloRequest, string>(context, HelloPost);
                    else if (context.Request.Method == "GET")
                        GenerateFakeResponse<string, string>(context, HelloGet);
                    break;
                default:
                    break;
            }
        }
    }
}
