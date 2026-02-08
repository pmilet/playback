// Copyright (c) 2017 Pierre Milet. All rights reserved.
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
    /// <summary>
    /// Extension methods for configuring playback functionality.
    /// </summary>
    public static class PlaybackExtension
    {
        /// <summary>
        /// Storage type options for playback messages.
        /// </summary>
        public enum PlaybackStorageType { Blob, File }

        /// <summary>
        /// Determines if the playback context is in a playback mode.
        /// </summary>
        /// <param name="playbackContext">The playback context to check.</param>
        /// <returns>True if in playback mode; otherwise, false.</returns>
        public static bool IsPlayback(this IPlaybackContext playbackContext)
        {
            return IsPlayback(playbackContext.PlaybackMode);
        }

        /// <summary>
        /// Determines if the specified playback mode is a playback mode.
        /// </summary>
        /// <param name="playbackMode">The playback mode to check.</param>
        /// <returns>True if it's a playback mode; otherwise, false.</returns>
        public static bool IsPlayback(this PlaybackMode playbackMode)
        {
            return (playbackMode == PlaybackMode.Playback || playbackMode == PlaybackMode.PlaybackReal || playbackMode == PlaybackMode.PlaybackChaos);
        }

        private static string AssemblyLoadDirectory
        {
            get
            {
                var location = Assembly.GetCallingAssembly().Location;
                return Path.GetDirectoryName(location) ?? Directory.GetCurrentDirectory();
            }
        }

        /// <summary>
        /// Adds playback services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="playbackStorageService">Optional custom storage service. If null, creates based on configuration.</param>
        public static void AddPlayback(this IServiceCollection services, IConfiguration configuration,
            IPlaybackStorageService? playbackStorageService = null)
        {
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPlaybackContext, PlaybackContext>();
            if (playbackStorageService == null)
            {
                if (configuration.GetSection("PlaybackStorage")?.GetValue<string>("ConnectionString")?.ToLower() == "local")
                {
                    string name = configuration.GetSection("PlaybackStorage").GetValue<string>("ContainerName") ?? "PlaybackFiles";
                    playbackStorageService = new PlaybackFileStorageService(Path.Combine(AssemblyLoadDirectory, name));
                }
                else
                {
                    playbackStorageService = new PlaybackBlobStorageService(configuration);
                }
            }
            services.AddScoped<IPlaybackStorageService>(provider => playbackStorageService);
        }

        /// <summary>
        /// Adds the playback middleware to the application pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UsePlayback(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PlaybackMiddleware>();
        }

        /// <summary>
        /// Adds the playback middleware to the application pipeline with a default mode.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="defaultMode">The default playback mode to use.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UsePlayback(this IApplicationBuilder builder, PlaybackMode defaultMode)
        {
            PlaybackContext.DefaultPlaybackMode = defaultMode;
            return builder.UseMiddleware<PlaybackMiddleware>();
        }
    }
}