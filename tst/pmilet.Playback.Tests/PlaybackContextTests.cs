// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using Moq;
using pmilet.Playback.Core;
using Xunit;

namespace pmilet.Playback.Tests
{
    /// <summary>
    /// Tests for PlaybackContext header parsing and mode resolution.
    /// Uses a default (null context) scenario and verifies static defaults.
    /// </summary>
    public class PlaybackContextTests
    {
        private static IPlaybackStorageService CreateStorageMock()
        {
            var mock = new Mock<IPlaybackStorageService>();
            return mock.Object;
        }

        private static IHttpContextAccessor CreateAccessor(HttpContext? context = null)
        {
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(a => a.HttpContext).Returns(context);
            return mock.Object;
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenAccessorIsNull()
        {
            var storageService = CreateStorageMock();
            Assert.Throws<ArgumentNullException>(() => new PlaybackContext(null!, storageService));
        }

        [Fact]
        public void Constructor_WithNullHttpContext_DoesNotThrow()
        {
            // Accessor with null HttpContext should still work (context not yet available)
            var accessor = CreateAccessor(null);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);
            Assert.NotNull(context);
        }

        [Fact]
        public void PlaybackMode_DefaultsToNone_WhenNoHeaderPresent()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.Equal(PlaybackMode.None, context.PlaybackMode);
        }

        [Theory]
        [InlineData("Record", PlaybackMode.Record)]
        [InlineData("Playback", PlaybackMode.Playback)]
        [InlineData("PlaybackReal", PlaybackMode.PlaybackReal)]
        [InlineData("PlaybackChaos", PlaybackMode.PlaybackChaos)]
        [InlineData("None", PlaybackMode.None)]
        public void PlaybackMode_ParsedFromXPlaybackModeHeader(string headerValue, PlaybackMode expectedMode)
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Playback-Mode"] = headerValue;

            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.Equal(expectedMode, context.PlaybackMode);
        }

        [Fact]
        public void PlaybackId_SetFromXPlaybackIdHeader()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Playback-Mode"] = "Playback";
            httpContext.Request.Headers["X-Playback-Id"] = "my-playback-id";

            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.Equal("my-playback-id", context.PlaybackId);
        }

        [Fact]
        public void IsPlayback_ReturnsFalse_WhenModeIsNone()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.False(context.IsPlayback);
        }

        [Fact]
        public void IsPlayback_ReturnsFalse_WhenModeIsRecord()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Playback-Mode"] = "Record";

            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.False(context.IsPlayback);
        }

        [Theory]
        [InlineData("Playback")]
        [InlineData("PlaybackReal")]
        [InlineData("PlaybackChaos")]
        public void IsPlayback_ReturnsTrue_WhenModeIsAPlaybackVariant(string mode)
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Playback-Mode"] = mode;

            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.True(context.IsPlayback);
        }

        [Fact]
        public void IsRecord_ReturnsTrue_WhenModeIsRecord()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Playback-Mode"] = "Record";

            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.True(context.IsRecord);
        }

        [Fact]
        public void IsRecord_ReturnsFalse_WhenModeIsNotRecord()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Playback-Mode"] = "Playback";

            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.False(context.IsRecord);
        }

        [Fact]
        public void ChangePlaybackMode_UpdatesDefaultPlaybackMode()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.Record);
            Assert.Equal(PlaybackMode.Record, PlaybackContext.DefaultPlaybackMode);

            // Reset
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);
        }

        [Fact]
        public void ChangePlaybackRequestContext_UpdatesDefaultRequestContext()
        {
            PlaybackContext.ChangePlaybackRequestContext("TestContext");
            Assert.Equal("TestContext", PlaybackContext.DefaultPlaybackRequestContext);

            // Reset
            PlaybackContext.ChangePlaybackRequestContext(string.Empty);
        }

        [Fact]
        public void ChangePlaybackFake_UpdatesDefaultPlaybackFake()
        {
            PlaybackContext.ChangePlaybackFake("FakeScenario");
            Assert.Equal("FakeScenario", PlaybackContext.DefaultPlaybackFake);

            // Reset
            PlaybackContext.ChangePlaybackFake(null);
            Assert.Equal(string.Empty, PlaybackContext.DefaultPlaybackFake);
        }

        [Fact]
        public void PlaybackStorageService_IsSetCorrectly()
        {
            PlaybackContext.ChangePlaybackMode(PlaybackMode.None);

            var httpContext = new DefaultHttpContext();
            var accessor = CreateAccessor(httpContext);
            var storageService = CreateStorageMock();

            var context = new PlaybackContext(accessor, storageService);

            Assert.Same(storageService, context.PlaybackStorageService);
        }
    }
}
