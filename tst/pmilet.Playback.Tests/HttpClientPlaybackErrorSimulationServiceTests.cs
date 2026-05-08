// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Xunit;

namespace pmilet.Playback.Tests
{
    public class HttpClientPlaybackErrorSimulationServiceTests
    {
        [Fact]
        public async Task GetDefaultConfig_ReturnsDefaultConfig()
        {
            var svc = new HttpClientPlaybackErrorSimulationService();
            var config = await svc.GetDefaultConfig();

            Assert.NotNull(config);
            Assert.Equal("default", config.Name);
            Assert.Equal(0u, config.RetriesBeforeReturningSuccess);
            Assert.False(config.ThrowException);
        }

        [Fact]
        public async Task GetNamedConfig_ReturnsDefaultConfig_WhenNameNotFound()
        {
            var svc = new HttpClientPlaybackErrorSimulationService();
            var config = await svc.GetNamedConfig("nonexistent");

            Assert.NotNull(config);
            Assert.Equal("default", config.Name);
        }

        [Fact]
        public async Task AddOrUpdate_AddsNewConfig()
        {
            var svc = new HttpClientPlaybackErrorSimulationService();
            var newConfig = new HttpClientPlaybackErrorSimulationConfig("myService", 3, false);

            await svc.AddOrUpdate("myService", newConfig);
            var retrieved = await svc.GetNamedConfig("myService");

            Assert.Equal("myService", retrieved.Name);
            Assert.Equal(3u, retrieved.RetriesBeforeReturningSuccess);
        }

        [Fact]
        public async Task AddOrUpdate_UpdatesExistingConfig()
        {
            var svc = new HttpClientPlaybackErrorSimulationService();
            var initial = new HttpClientPlaybackErrorSimulationConfig("myService", 1, false);
            var updated = new HttpClientPlaybackErrorSimulationConfig("myService", 5, true);

            await svc.AddOrUpdate("myService", initial);
            await svc.AddOrUpdate("myService", updated);

            var retrieved = await svc.GetNamedConfig("myService");
            Assert.Equal(5u, retrieved.RetriesBeforeReturningSuccess);
            Assert.True(retrieved.ThrowException);
        }

        [Fact]
        public async Task ChangeAll_UpdatesAllConfigs()
        {
            var svc = new HttpClientPlaybackErrorSimulationService();
            var first = new HttpClientPlaybackErrorSimulationConfig("svc1", 0, false);
            var second = new HttpClientPlaybackErrorSimulationConfig("svc2", 0, false);
            await svc.AddOrUpdate("svc1", first);
            await svc.AddOrUpdate("svc2", second);

            var newConfig = new HttpClientPlaybackErrorSimulationConfig("any", 10, true);
            await svc.ChangeAll(newConfig);

            var cfg1 = await svc.GetNamedConfig("svc1");
            var cfg2 = await svc.GetNamedConfig("svc2");
            var cfgDefault = await svc.GetDefaultConfig();

            Assert.Equal(10u, cfg1.RetriesBeforeReturningSuccess);
            Assert.Equal(10u, cfg2.RetriesBeforeReturningSuccess);
            Assert.Equal(10u, cfgDefault.RetriesBeforeReturningSuccess);
        }

        [Fact]
        public void HttpClientPlaybackErrorSimulationConfigs_ContainsDefaultEntry()
        {
            var svc = new HttpClientPlaybackErrorSimulationService();

            Assert.True(svc.HttpClientPlaybackErrorSimulationConfigs.ContainsKey("default"));
        }

        [Fact]
        public void HttpClientPlaybackErrorSimulationConfig_SetsProperties()
        {
            var config = new HttpClientPlaybackErrorSimulationConfig("myHandler", 7, true);

            Assert.Equal("myHandler", config.Name);
            Assert.Equal(7u, config.RetriesBeforeReturningSuccess);
            Assert.True(config.ThrowException);
        }
    }
}
