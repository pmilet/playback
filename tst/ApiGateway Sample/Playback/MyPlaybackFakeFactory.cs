// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ApiGateway_Sample
{
    public class HelloRequest
    {
        public string Name { get; set; }
    }

    public class MyPlaybackFakeFactory : FakeFactoryBase
    {
        public override bool GenerateFakeResponse(HttpContext context)
        {
            switch (context.Request.Path.Value.ToLower())
            {
                case "/api/hello":
                    if (context.Request.Method == "POST")
                        return GenerateFakeResponse<HelloRequest, string>(context, HelloPost);
                    else if (context.Request.Method == "GET")
                        return GenerateFakeResponse<string, string>(context, HelloGet);
                    break;
                default:
                    return false;
            }
            return false;
        }


        private string HelloGet(string request)
        {
            return "Hello FAKE";
        }

        private string HelloPost(HelloRequest request)
        {
            var name = !string.IsNullOrEmpty(request.Name) ? request.Name : "Whoever";
            return "Hello " + name + " FAKE";
        }

    }
}
