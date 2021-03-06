﻿// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.IO;

namespace pmilet.Playback
{
    public static class PlaybackExtension
    {
        public enum PlaybackStorageType { Blob, File }

        public static bool IsPlayback(this IPlaybackContext playbackContext)
        {
            return IsPlayback(playbackContext.PlaybackMode);
        }

        public static bool IsPlayback(this PlaybackMode playbackMode)
        {
            return (playbackMode == PlaybackMode.Playback || playbackMode == PlaybackMode.PlaybackReal || playbackMode == PlaybackMode.PlaybackChaos);
        }

        private static string AssemblyLoadDirectory
        {
            get
            {
                var codeBase = Assembly.GetCallingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static void AddPlayback(this IServiceCollection services, IConfiguration configuration,
            IPlaybackStorageService playbackStorageService = null)
        {
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPlaybackContext, PlaybackContext>();
            if (playbackStorageService == null)
            {
                if (configuration.GetSection("PlaybackStorage")?.GetValue<string>("ConnectionString")?.ToLower() == "local")
                {
                    string name = configuration.GetSection("PlaybackStorage").GetValue<string>("ContainerName");
                    playbackStorageService = new PlaybackFileStorageService($"{AssemblyLoadDirectory}\\{name}\\");
                }
                else
                {
                    playbackStorageService = new PlaybackBlobStorageService(configuration);
                }
            }
            services.AddScoped<IPlaybackStorageService>(provider => playbackStorageService);
        }

        public static IApplicationBuilder UsePlayback(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PlaybackMiddleware>();
        }

        public static IApplicationBuilder UsePlayback(this IApplicationBuilder builder, PlaybackMode defaultMode)
        {
            PlaybackContext.DefaultPlaybackMode = defaultMode;
            return builder.UseMiddleware<PlaybackMiddleware>();
        }
    }
}