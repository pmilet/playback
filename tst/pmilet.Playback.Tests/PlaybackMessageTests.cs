// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using System.IO;
using System.Text;
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackMessageTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var msg = new PlaybackMessage("/api/test", "?foo=bar", "body content", "application/json", 123);

            Assert.Equal("/api/test", msg.Path);
            Assert.Equal("?foo=bar", msg.QueryString);
            Assert.Equal("body content", msg.BodyString);
            Assert.Equal("application/json", msg.ContentType);
        }

        [Fact]
        public void Constructor_StoresElapsedTimeAsResponseTimeMetadata()
        {
            var msg = new PlaybackMessage("/api/test", "", "body", "text", 500);

            Assert.Equal(500, msg.ResponseTime);
        }

        [Fact]
        public void ResponseTime_ReturnsZero_WhenMetadataMissing()
        {
            var msg = new PlaybackMessage("/api/test", "", "body", "text", 0);
            msg.Metadata.Remove("responseTime");

            Assert.Equal(0, msg.ResponseTime);
        }

        [Fact]
        public void ResponseTime_ReturnsZero_WhenMetadataValueIsInvalid()
        {
            var msg = new PlaybackMessage("/api/test", "", "body", "text", 0);
            msg.Metadata["responseTime"] = "not-a-number";

            Assert.Equal(0, msg.ResponseTime);
        }

        [Fact]
        public void GetBodyStream_ReturnsStreamWithBodyContent()
        {
            var body = "hello world";
            var msg = new PlaybackMessage("/api/test", "", body, "text/plain", 0);

            using var stream = msg.GetBodyStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();

            Assert.Equal(body, content);
        }

        [Fact]
        public void GetBodyStream_ReturnsEmptyStream_WhenBodyStringIsNull()
        {
            var msg = new PlaybackMessage("/api/test", "", null!, "text/plain", 0);

            using var stream = msg.GetBodyStream();

            Assert.Equal(0, stream.Length);
        }

        [Fact]
        public void Metadata_IsInitializedWithResponseTime()
        {
            var msg = new PlaybackMessage("/api/test", "", "body", "text", 200);

            Assert.True(msg.Metadata.ContainsKey("responseTime"));
            Assert.Equal("200", msg.Metadata["responseTime"]);
        }
    }
}
