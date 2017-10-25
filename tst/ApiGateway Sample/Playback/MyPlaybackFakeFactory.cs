// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace ApiGateway_Sample
{
    public class HelloRequest
    {
        public string Name { get; set; }
    }

    public class MyPlaybackFakeFactory : FakeFactoryBase
    {
        public override void GenerateFakeResponse(HttpContext context)
        {
            var userRegEx = new Regex("^/api/hello/.*$");
            switch (context.Request.Path.Value.ToLower())
            {
                case "/api/hello":
                    if (context.Request.Method == "GET")
                        GenerateFakeResponse(context, HelloGet);
                    else if (context.Request.Method == "POST")
                        GenerateFakeResponse<HelloRequest, string>(context, HelloPost);
                    break;
                case var path when (userRegEx.IsMatch(path)):
                    if (context.Request.Method == "GET")
                        GenerateFakeResponse(context, HelloGetByName);
                    break;
                default:
                    throw new NotImplementedException("fake method not found");
            }
        }

        private string HelloGet(string path)
        {
            return $"Hello Get FAKE";
        }

        private string HelloGetByName(string path)
        {
            var namePathParam = path.Split('/').LastOrDefault();
            var name = !string.IsNullOrEmpty(namePathParam) ? namePathParam : "Whoever";
            return $"Hello {name} GetByName FAKE";
        }

        private string HelloPost(string path, HelloRequest request)
        {
            var name = !string.IsNullOrEmpty(request.Name) ? request.Name : "Whoever";
            return "Hello " + name + " Post FAKE";
        }

    }
}
