// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.HttpPlayback;
using pmilet.HttpPlayback.Core;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pmilet.HttpPlayback
{
    public class PlaybackSwaggerFilter : IOperationFilter
    {
        public virtual void Apply(Operation operation, OperationFilterContext operationFilter)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "X-Playback-Version",
                In = "header",
                Required = false,
                Type = "string",
                Description = "PlayBack version to determine wich version to retrieve"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "X-Playback-RequestContext",
                In = "header",
                Required = false,
                Type = "string",
                Description = "PlayBack context info to be applied to request"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "X-Playback-Mode",
                In = "header",
                Required = false,
                Type = "string",
                Enum = Enum.GetNames(typeof(PlaybackMode)),
                Description = "PlayBack mode to determine how to handle the request"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "X-Playback-Fake",
                In = "header",
                Required = false,
                Type = "string",
                Enum = new List<object>() { "None", "Inbound", "Outbound" },
                Description = "Request to fake incoming requests and Proxy to fake outgoing requests"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "X-Playback-Id",
                In = "header",
                Required = false,
                Type = "string",
                Description = "Playback Identifier to be able to retrieve a request for replaying"
            });

        }
    }
}
