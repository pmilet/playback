// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using Moq;
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackExtensionTests
    {
        [Theory]
        [InlineData(PlaybackMode.Playback, true)]
        [InlineData(PlaybackMode.PlaybackReal, true)]
        [InlineData(PlaybackMode.PlaybackChaos, true)]
        [InlineData(PlaybackMode.Record, false)]
        [InlineData(PlaybackMode.None, false)]
        public void IsPlayback_PlaybackMode_ReturnsExpectedResult(PlaybackMode mode, bool expected)
        {
            Assert.Equal(expected, mode.IsPlayback());
        }

        [Theory]
        [InlineData(PlaybackMode.Playback, true)]
        [InlineData(PlaybackMode.PlaybackReal, true)]
        [InlineData(PlaybackMode.PlaybackChaos, true)]
        [InlineData(PlaybackMode.Record, false)]
        [InlineData(PlaybackMode.None, false)]
        public void IsPlayback_IPlaybackContext_ReturnsExpectedResult(PlaybackMode mode, bool expected)
        {
            var mockContext = new Mock<IPlaybackContext>();
            mockContext.Setup(c => c.PlaybackMode).Returns(mode);
            mockContext.Setup(c => c.IsPlayback).Returns(
                mode == PlaybackMode.Playback || mode == PlaybackMode.PlaybackReal || mode == PlaybackMode.PlaybackChaos);

            Assert.Equal(expected, mockContext.Object.IsPlayback());
        }
    }
}
