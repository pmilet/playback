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
                Name = "PlayBackMode",
                In = "header",
                Required = false,
                Type = "string",
                Enum = Enum.GetNames(typeof(PlaybackMode)),
                Description = "Indica la forma en la que se tratará la petición"
            });
            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "PlaybackId",
                In = "header",
                Required = false,
                Type = "string",
                Description = "Id que permite reproducir una mensaje previamente grabado"
            });
            
        }
    }
}
