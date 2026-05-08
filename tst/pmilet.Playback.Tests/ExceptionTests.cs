// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackFakeExceptionTests
    {
        [Fact]
        public void PlaybackFakeException_DefaultConstructor_CanBeCreated()
        {
            var ex = new PlaybackFakeException();
            Assert.NotNull(ex);
        }

        [Fact]
        public void PlaybackFakeException_MessageConstructor_SetsMessage()
        {
            var ex = new PlaybackFakeException("test message");
            Assert.Equal("test message", ex.Message);
        }

        [Fact]
        public void PlaybackFakeException_WithInnerException_SetsInnerException()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new PlaybackFakeException("outer", inner);

            Assert.Equal("outer", ex.Message);
            Assert.Equal(inner, ex.InnerException);
        }

        [Fact]
        public void PlaybackFakeException_IsException()
        {
            var ex = new PlaybackFakeException("test");
            Assert.IsAssignableFrom<Exception>(ex);
        }
    }

    public class PlaybackStorageExceptionTests
    {
        [Fact]
        public void PlaybackStorageException_SetsPlaybackIdAndMessage()
        {
            var ex = new PlaybackStorageException("my-playback-id", "storage error");

            Assert.Equal("my-playback-id", ex.PlaybackId);
            Assert.Equal("storage error", ex.Message);
        }

        [Fact]
        public void PlaybackStorageException_SetsPlaybackIdMessageAndInnerException()
        {
            var inner = new IOException("disk error");
            var ex = new PlaybackStorageException("my-id", "upload failed", inner);

            Assert.Equal("my-id", ex.PlaybackId);
            Assert.Equal("upload failed", ex.Message);
            Assert.Equal(inner, ex.InnerException);
        }

        [Fact]
        public void PlaybackStorageException_IsPlaybackFakeException()
        {
            var ex = new PlaybackStorageException("id", "msg");
            Assert.IsAssignableFrom<PlaybackFakeException>(ex);
        }
    }
}
