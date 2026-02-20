// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using System.IO;
using System.Text.Json;
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackFileStorageServiceTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly PlaybackFileStorageService _service;

        public PlaybackFileStorageServiceTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "PlaybackTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
            _service = new PlaybackFileStorageService(_tempDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, recursive: true);
        }

        [Fact]
        public async Task UploadAndDownload_RoundTrip_WithPathAndQueryAndBody()
        {
            var id = "test_upload_download";
            var path = "/api/values";
            var queryString = "?foo=bar";
            var body = "Hello World";

            await _service.UploadToStorageAsync(id, path, queryString, body);
            var msg = await _service.DownloadFromStorageAsync(id);

            Assert.Equal(path, msg.Path);
            Assert.Equal(queryString, msg.QueryString);
            Assert.Equal(body, msg.BodyString);
        }

        [Fact]
        public async Task UploadAndDownload_RoundTrip_WithContentStringOverload()
        {
            var id = "test_content_overload";
            var content = "simple content string";

            await _service.UploadToStorageAsync(id, content);
            var msg = await _service.DownloadFromStorageAsync(id);

            Assert.Equal(content, msg.BodyString);
        }

        [Fact]
        public async Task UploadAndDownload_RoundTrip_WithObjectOverload()
        {
            var id = "test_object_overload";
            var obj = new { Name = "Alice", Age = 30 };

            await _service.UploadToStorageAsync(id, (object)obj);
            var result = await _service.ReplayFromStorageAsync<dynamic>(id);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UploadToStorageAsync_ThrowsPlaybackStorageException_WhenPlaybackIdIsEmpty()
        {
            await Assert.ThrowsAsync<PlaybackStorageException>(async () =>
                await _service.UploadToStorageAsync(string.Empty, "/path", "", "body"));
        }

        [Fact]
        public async Task DownloadFromStorageAsync_ThrowsPlaybackStorageException_WhenFileDoesNotExist()
        {
            await Assert.ThrowsAsync<PlaybackStorageException>(async () =>
                await _service.DownloadFromStorageAsync("nonexistent_id"));
        }

        [Fact]
        public async Task UploadAndReplay_DeserializesObjectCorrectly()
        {
            var id = "test_replay";
            var original = new TestRecord { Name = "Bob", Value = 42 };

            await _service.UploadToStorageAsync(id, (object)original);
            var replayed = await _service.ReplayFromStorageAsync<TestRecord>(id);

            Assert.Equal("Bob", replayed.Name);
            Assert.Equal(42, replayed.Value);
        }

        [Fact]
        public async Task UploadAndReplay_WithPlaybackMode_Playback_ReturnsBodyString()
        {
            var id = "test_replay_mode";
            var content = "replay content";

            await _service.UploadToStorageAsync(id, content);
            var result = await _service.ReplayFromStorageAsync(PlaybackMode.Playback, id);

            Assert.Equal(content, result);
        }

        [Fact]
        public async Task ElapsedTime_IsPreservedInResponseTime()
        {
            var id = "test_elapsed";
            await _service.UploadToStorageAsync(id, "/path", "", "body", elapsedTime: 750);
            var msg = await _service.DownloadFromStorageAsync(id);

            Assert.Equal(750, msg.ResponseTime);
        }

        [Fact]
        public void Constructor_CreatesStorageDirectory()
        {
            var storagePath = Path.Combine(_tempDirectory, "subdir_" + Guid.NewGuid().ToString("N"), "storage");
            var parentDir = Path.GetDirectoryName(storagePath)!;
            Assert.False(Directory.Exists(parentDir));

            _ = new PlaybackFileStorageService(storagePath);

            Assert.True(Directory.Exists(parentDir));
        }

        private class TestRecord
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }
    }
}
