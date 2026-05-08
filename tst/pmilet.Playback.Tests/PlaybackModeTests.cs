// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackModeTests
    {
        [Theory]
        [InlineData(PlaybackMode.None, 0)]
        [InlineData(PlaybackMode.Record, 1)]
        [InlineData(PlaybackMode.Playback, 2)]
        [InlineData(PlaybackMode.PlaybackReal, 3)]
        [InlineData(PlaybackMode.PlaybackChaos, 4)]
        public void PlaybackMode_HasExpectedValues(PlaybackMode mode, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)mode);
        }

        [Fact]
        public void PlaybackMode_CanBeParsedFromString()
        {
            Assert.True(System.Enum.TryParse<PlaybackMode>("Record", out var mode));
            Assert.Equal(PlaybackMode.Record, mode);
        }

        [Fact]
        public void PlaybackMode_CanBeParsedFromString_Playback()
        {
            Assert.True(System.Enum.TryParse<PlaybackMode>("Playback", out var mode));
            Assert.Equal(PlaybackMode.Playback, mode);
        }

        [Fact]
        public void PlaybackMode_CanBeParsedFromString_PlaybackReal()
        {
            Assert.True(System.Enum.TryParse<PlaybackMode>("PlaybackReal", out var mode));
            Assert.Equal(PlaybackMode.PlaybackReal, mode);
        }

        [Fact]
        public void PlaybackMode_CanBeParsedFromString_PlaybackChaos()
        {
            Assert.True(System.Enum.TryParse<PlaybackMode>("PlaybackChaos", out var mode));
            Assert.Equal(PlaybackMode.PlaybackChaos, mode);
        }

        [Fact]
        public void PlaybackMode_CanBeParsedFromString_None()
        {
            Assert.True(System.Enum.TryParse<PlaybackMode>("None", out var mode));
            Assert.Equal(PlaybackMode.None, mode);
        }

        [Fact]
        public void PlaybackMode_InvalidString_ReturnsFalse()
        {
            Assert.False(System.Enum.TryParse<PlaybackMode>("InvalidMode", out _));
        }
    }
}
