// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Moq;
using pmilet.Playback.Core;
using System.Net;
using System.Net.Http;
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackHandlerTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly PlaybackFileStorageService _storageService;
        private readonly HttpClientPlaybackErrorSimulationService _errorSimService;

        public PlaybackHandlerTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "PlaybackHandlerTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
            _storageService = new PlaybackFileStorageService(_tempDirectory);
            _errorSimService = new HttpClientPlaybackErrorSimulationService();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, recursive: true);
        }

        private Mock<IPlaybackContext> CreateContextMock(PlaybackMode mode, string playbackId = "test-id")
        {
            var mock = new Mock<IPlaybackContext>();
            mock.Setup(c => c.PlaybackMode).Returns(mode);
            mock.Setup(c => c.PlaybackId).Returns(playbackId);
            mock.Setup(c => c.IsPlayback).Returns(
                mode == PlaybackMode.Playback || mode == PlaybackMode.PlaybackReal || mode == PlaybackMode.PlaybackChaos);
            mock.Setup(c => c.IsRecord).Returns(mode == PlaybackMode.Record);
            return mock;
        }

        [Fact]
        public async Task SendAsync_NoneMode_CallsRealHandler()
        {
            var contextMock = CreateContextMock(PlaybackMode.None, "test-none");
            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("real response")
            });

            var playbackHandler = new PlaybackHandler(
                fakeHandler,
                contextMock.Object,
                _storageService,
                "testHandler",
                _errorSimService);

            var client = new HttpClient(playbackHandler);
            var response = await client.GetAsync("http://example.com/api/test");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, fakeHandler.CallCount);
        }

        [Fact]
        public async Task SendAsync_RecordMode_SavesRequestAndResponseAndCallsRealHandler()
        {
            var contextMock = CreateContextMock(PlaybackMode.Record, "rec-id-001");
            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("recorded response")
            });

            var playbackHandler = new PlaybackHandler(
                fakeHandler,
                contextMock.Object,
                _storageService,
                "svcHandler",
                _errorSimService);

            var client = new HttpClient(playbackHandler);
            var response = await client.GetAsync("http://example.com/api/record");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, fakeHandler.CallCount);

            // Files should have been written (req and resp)
            var files = Directory.GetFiles(_tempDirectory);
            Assert.NotEmpty(files);
        }

        [Fact]
        public async Task SendAsync_PlaybackMode_ReturnsStoredResponse_WithoutCallingRealHandler()
        {
            var playbackId = "playback-id-001";
            var storedContent = "\"stored response\"";

            // Pre-store the response file
            var respFileId = $"svcHandlerResp1{playbackId}";
            await _storageService.UploadToStorageAsync(respFileId, storedContent);

            var contextMock = CreateContextMock(PlaybackMode.Playback, playbackId);
            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var playbackHandler = new PlaybackHandler(
                fakeHandler,
                contextMock.Object,
                _storageService,
                "svcHandler",
                _errorSimService);

            var client = new HttpClient(playbackHandler);
            var response = await client.GetAsync("http://example.com/api/playback");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, fakeHandler.CallCount); // Real handler not called

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(storedContent, body);
        }

        [Fact]
        public async Task SendAsync_WithThrowExceptionConfig_ThrowsPlaybackFakeException()
        {
            var contextMock = CreateContextMock(PlaybackMode.None, "exc-id");
            await _errorSimService.AddOrUpdate("throwHandler",
                new HttpClientPlaybackErrorSimulationConfig("throwHandler", 0, true));

            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));

            var playbackHandler = new PlaybackHandler(
                fakeHandler,
                contextMock.Object,
                _storageService,
                "throwHandler",
                _errorSimService);

            var client = new HttpClient(playbackHandler);

            await Assert.ThrowsAsync<PlaybackFakeException>(async () =>
                await client.GetAsync("http://example.com/api/test"));
        }

        [Fact]
        public async Task SendAsync_WithRetriesConfig_ThrowsOnFirstCallsThenSucceeds()
        {
            var contextMock = CreateContextMock(PlaybackMode.None, "retry-id");
            await _errorSimService.AddOrUpdate("retryHandler",
                new HttpClientPlaybackErrorSimulationConfig("retryHandler", 2, false));

            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("success")
            });

            var playbackHandler = new PlaybackHandler(
                fakeHandler,
                contextMock.Object,
                _storageService,
                "retryHandler",
                _errorSimService);

            var client = new HttpClient(playbackHandler);

            // First two calls should throw
            await Assert.ThrowsAsync<PlaybackFakeException>(async () =>
                await client.GetAsync("http://example.com/api/test"));

            await Assert.ThrowsAsync<PlaybackFakeException>(async () =>
                await client.GetAsync("http://example.com/api/test"));

            // Third call should succeed
            var response = await client.GetAsync("http://example.com/api/test");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void HttpClientFactory_WithPlaybackContext_ReturnsHttpClient()
        {
            var contextMock = CreateContextMock(PlaybackMode.None);

            var client = HttpClientFactory.WithPlaybackContext(
                contextMock.Object,
                _storageService,
                "testPrefix",
                _errorSimService);

            Assert.NotNull(client);
            Assert.IsType<HttpClient>(client);
        }

        /// <summary>
        /// A fake HTTP message handler for use in unit tests.
        /// </summary>
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public int CallCount { get; private set; }

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                return Task.FromResult(_response);
            }
        }
    }
}
