using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pmilet.Playback;
using pmilet.Playback.Core;
using TestWebApi.Controllers;

namespace TestWebApi
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddPlayback(Configuration);

            services.AddScoped<Service, PlaybackService>();
            services.AddScoped<IService, PlaybackService>();
            services.AddScoped<IHttpClientPlaybackErrorSimulationService, HttpClientPlaybackErrorSimulationService>();

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<PlaybackSwaggerFilter>();
            });

            services.AddControllers();

            //don't forget to return the service provider
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
          
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage( );
            }

            app.UseSwagger();
            app.UseSwaggerUI(c=> { c.SwaggerEndpoint("v1/swagger.json", "API"); });

            app.UsePlayback();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
