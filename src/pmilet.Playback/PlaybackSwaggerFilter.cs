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
        public virtual void Apply(Operation operation, OperationFilterContext operationFilter)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "PlaybackContext",
                In = "header",
                Required = false,
                Type = "string",
                Description = "PlayBack context info to be applied to request"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "PlaybackMode",
                In = "header",
                Required = false,
                Type = "string",
                Enum = Enum.GetNames(typeof(PlaybackMode)),
                Description = "PlayBack mode to determine how to handle the request"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "PlaybackId",
                In = "header",
                Required = false,
                Type = "string",
                Description = "Playback Identifier to be able to retrieve a request for replaying"
            });

        }
    }
}
