﻿// Copyright (c) 2017 Pierre Milet. All rights reserved.
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

        public static void AddPlayback(this IServiceCollection services, IConfigurationRoot configuration, 
            IPlaybackStorageService playbackStorageService = null, IFakeFactory fakeFactory = null)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPlaybackContext, PlaybackContext>();
            if( playbackStorageService == null )
            {
                playbackStorageService = new PlaybackBlobStorageService(configuration);
            }
            services.AddScoped<IPlaybackStorageService>(provider => playbackStorageService);
            if (fakeFactory == null)
            {
                fakeFactory = new DefaultFakeFactory();
            }
            services.AddScoped(typeof(IFakeFactory), fakeFactory.GetType());
        }

        public static IApplicationBuilder UsePlayback(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PlaybackMiddleware>();
        }
    }
}