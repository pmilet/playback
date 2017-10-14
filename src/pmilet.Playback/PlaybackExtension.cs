﻿using pmilet.Playback.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace pmilet.Playback
{ 
    public static class PlaybackExtension
    {
        public static void AddIberfabricFakeFactory(this IServiceCollection services, Type fakefactory)
        {
            services.AddScoped(typeof(IFakeFactory),fakefactory);
        }

        public static void AddIberfabricPlayback(this IServiceCollection services)
        {
            services.AddScoped<IPlaybackContext, PlaybackContext>();
            services.AddScoped<IPlaybackStorageService, PlaybackBlobStorageService>();
        }

        public static void AddIberfabricPlayback(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped<IPlaybackContext, PlaybackContext>();
            services.AddScoped<IPlaybackStorageService>(provider => new PlaybackBlobStorageService(configuration));
        }

        public static IApplicationBuilder UseIberfabricPlayback(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PlaybackMiddleware>();
        }
    }
}