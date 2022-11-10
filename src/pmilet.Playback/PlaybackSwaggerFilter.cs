// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using pmilet.Playback;
using pmilet.Playback.Core;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public class PlaybackSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Playback-Version",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "PlayBack version to determine wich version to retrieve"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Playback-RequestContext",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "PlayBack context info to be applied to request"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Playback-Mode",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Enum = new List<IOpenApiAny> { new OpenApiString("None"), new OpenApiString("Record") , new OpenApiString("Playback") , new OpenApiString("PlaybackReal") , new OpenApiString("PlaybackChaos") } },
                Description = "PlayBack mode to determine how to handle the request"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Playback-Id",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "Playback Identifier to be able to retrieve a request for replaying"
            });

        }

    }
}
