// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Xunit;

namespace pmilet.Playback.Tests
{
    public class PlaybackIdExtensionTests
    {
        // Example: DemoUser_ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046
        private const string SamplePlaybackId = "DemoUser_ApiGateway+Sample_v1.0_Hello%252Fhello_GET_757602046";

        [Fact]
        public void Context_ExtractsFirstSegment()
        {
            Assert.Equal("DemoUser", SamplePlaybackId.Context());
        }

        [Fact]
        public void Name_ExtractsSecondSegment()
        {
            Assert.Equal("ApiGateway+Sample", SamplePlaybackId.Name());
        }

        [Fact]
        public void Version_ExtractsThirdSegment()
        {
            Assert.Equal("v1.0", SamplePlaybackId.Version());
        }

        [Fact]
        public void RequestPath_ExtractsFourthSegment()
        {
            Assert.Equal("Hello%252Fhello", SamplePlaybackId.RequestPath());
        }

        [Fact]
        public void RequestMethod_ExtractsFifthSegment()
        {
            Assert.Equal("GET", SamplePlaybackId.RequestMethod());
        }

        [Fact]
        public void HashCode_ExtractsSixthSegment()
        {
            Assert.Equal("757602046", SamplePlaybackId.HashCode());
        }

        [Fact]
        public void Context_ReturnsEmpty_WhenPlaybackIdIsEmpty()
        {
            Assert.Equal(string.Empty, string.Empty.Context());
        }

        [Fact]
        public void Name_ReturnsEmpty_WhenPlaybackIdHasOnlyOneSegment()
        {
            Assert.Equal(string.Empty, "OnlyOneSegment".Name());
        }

        [Fact]
        public void HashCode_ReturnsEmpty_WhenNotEnoughSegments()
        {
            Assert.Equal(string.Empty, "A_B_C_D_E".HashCode());
        }

        [Fact]
        public void AllParts_CanBeExtracted_FromWellFormedPlaybackId()
        {
            // Matches the format: Context_Name_Version_Path_Method_Hash
            var id = "ctx_name_ver_path_METHOD_hash123";

            Assert.Equal("ctx", id.Context());
            Assert.Equal("name", id.Name());
            Assert.Equal("ver", id.Version());
            Assert.Equal("path", id.RequestPath());
            Assert.Equal("METHOD", id.RequestMethod());
            Assert.Equal("hash123", id.HashCode());
        }
    }
}
