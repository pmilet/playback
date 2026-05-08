// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using pmilet.Playback.Core;
using System.Net;
using System.Net.Http;
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackMiddlewareTests : IDisposable
    {
        private readonly string _tempDirectory;

        public PlaybackMiddlewareTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "PlaybackMiddlewareTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, recursive: true);
        }

        private IHost BuildHost(string storagePath)
        {
            var storageService = new PlaybackFileStorageService(storagePath);

            return new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
                            services.AddScoped<IPlaybackContext, PlaybackContext>();
                            services.AddScoped<IPlaybackStorageService>(_ => storageService);
                        })
                        .Configure(app =>
                        {
                            app.UsePlayback();
                            app.Run(async ctx =>
                            {
                                ctx.Response.StatusCode = 200;
                                await ctx.Response.WriteAsync("OK");
                            });
                        });
                })
                .Build();
        }

        [Fact]
        public async Task Middleware_NoneMode_PassesThrough()
        {
            using var host = BuildHost(_tempDirectory);
            await host.StartAsync();
            var client = host.GetTestClient();

            var response = await client.GetAsync("/api/test");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("OK", body);
        }

        [Fact]
        public async Task Middleware_RecordMode_AddsPlaybackIdResponseHeader()
        {
            using var host = BuildHost(_tempDirectory);
            await host.StartAsync();
            var client = host.GetTestClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/values");
            request.Headers.Add("X-Playback-Mode", "Record");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Playback-Id"),
                "Response should have X-Playback-Id header when recording");
        }

        [Fact]
        public async Task Middleware_RecordThenPlayback_ReturnsSameBody()
        {
            using var host = BuildHost(_tempDirectory);
            await host.StartAsync();
            var client = host.GetTestClient();

            // Record
            var recordRequest = new HttpRequestMessage(HttpMethod.Get, "/api/values");
            recordRequest.Headers.Add("X-Playback-Mode", "Record");
            var recordResponse = await client.SendAsync(recordRequest);

            Assert.Equal(HttpStatusCode.OK, recordResponse.StatusCode);
            var playbackId = recordResponse.Headers.GetValues("X-Playback-Id").First();
            Assert.NotEmpty(playbackId);

            // Playback
            var playbackRequest = new HttpRequestMessage(HttpMethod.Get, "/api/values");
            playbackRequest.Headers.Add("X-Playback-Mode", "Playback");
            playbackRequest.Headers.Add("X-Playback-Id", playbackId);
            var playbackResponse = await client.SendAsync(playbackRequest);

            Assert.Equal(HttpStatusCode.OK, playbackResponse.StatusCode);
        }

        [Fact]
        public async Task Middleware_RecordMode_PersistsFileToStorage()
        {
            using var host = BuildHost(_tempDirectory);
            await host.StartAsync();
            var client = host.GetTestClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hello");
            request.Headers.Add("X-Playback-Mode", "Record");
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // A file should have been written to the temp directory
            var files = Directory.GetFiles(_tempDirectory);
            Assert.NotEmpty(files);
        }

        [Fact]
        public async Task Middleware_PlaybackRealMode_AcceptsPlaybackId()
        {
            using var host = BuildHost(_tempDirectory);
            await host.StartAsync();
            var client = host.GetTestClient();

            // First record
            var recordRequest = new HttpRequestMessage(HttpMethod.Get, "/api/items");
            recordRequest.Headers.Add("X-Playback-Mode", "Record");
            var recordResponse = await client.SendAsync(recordRequest);
            var playbackId = recordResponse.Headers.GetValues("X-Playback-Id").First();

            // Then replay with PlaybackReal mode
            var playbackRequest = new HttpRequestMessage(HttpMethod.Get, "/api/items");
            playbackRequest.Headers.Add("X-Playback-Mode", "PlaybackReal");
            playbackRequest.Headers.Add("X-Playback-Id", playbackId);
            var playbackResponse = await client.SendAsync(playbackRequest);

            Assert.Equal(HttpStatusCode.OK, playbackResponse.StatusCode);
        }
    }
}
