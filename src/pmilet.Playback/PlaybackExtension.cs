// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace pmilet.Playback
{
    public static class PlaybackExtension
    {
        public enum PlaybackStorageType { Blob, File }
        public static void AddFakeFactory<T>(this IServiceCollection services)
        {
            services.AddScoped(typeof(IFakeFactory), typeof(T));
        }

        public static void AddPlayback(this IServiceCollection services, IConfigurationRoot configuration, PlaybackStorageType type = PlaybackStorageType.Blob)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPlaybackContext, PlaybackContext>();
            switch (type)
            {
                case PlaybackStorageType.File:
                    services.AddScoped<IPlaybackStorageService>(provider => new PlaybackFileStorageService());
                    break;
                case PlaybackStorageType.Blob:
                default:
                    services.AddScoped<IPlaybackStorageService>(provider => new PlaybackBlobStorageService(configuration));
                    break;
            }
        } 

        public static IApplicationBuilder UsePlayback(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PlaybackMiddleware>();
        }
    }
}