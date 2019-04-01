using pmilet.Playback.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public class PlaybackHandler : DelegatingHandler
    {
        readonly IPlaybackStorageService _playbackStorageService;
        readonly IPlaybackContext _playbackContext;
        readonly string _handlerName = string.Empty;
        readonly HttpClientPlaybackErrorSimulationConfig _config;
        private int _failedCount = 0;
        private int _requestNumber = 0;
        public PlaybackHandler( HttpMessageHandler innerHandler, 
            IPlaybackContext playbackContext, 
            IPlaybackStorageService playbackStorageService,
            string handlerName,
            IHttpClientPlaybackErrorSimulationService configService) 
            : base(innerHandler)
        {
            _playbackStorageService = playbackStorageService;
            _playbackContext = playbackContext;
            _handlerName = handlerName;
            _config = configService.GetNamedConfig(handlerName).Result;
        }

        public PlaybackHandler( 
            IPlaybackContext playbackContext, 
            IPlaybackStorageService playbackStorageService,
            string handlerName,
            IHttpClientPlaybackErrorSimulationService configService)
            : this(new HttpClientHandler(), playbackContext, playbackStorageService, handlerName, configService)
        {}
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
          
            PerformPlaybackSimulationStrategy();
            _requestNumber++;

            if (_playbackContext.PlaybackMode == PlaybackMode.Record)
            {
                string content = request.Content !=null? await request.Content.ReadAsStringAsync() : string.Empty;
                string playbackId = $"{_handlerName}Req{_requestNumber}{_playbackContext.PlaybackId}";
                await _playbackStorageService.UploadToStorageAsync(playbackId, content);
            }

            HttpResponseMessage replayedResponse = null;

            if (_playbackContext.IsPlayback())
            {
                string playbackId = $"{_handlerName}Resp{_requestNumber}{_playbackContext.PlaybackId}";
                return replayedResponse = await Replay(playbackId);
            }

            var freshResponse = await base.SendAsync(request, cancellationToken);

            if (_playbackContext.PlaybackMode == PlaybackMode.Record)
            {
                string playbackId = $"{_handlerName}Resp{_requestNumber}{_playbackContext.PlaybackId}";
                await Save(freshResponse, playbackId);
            }
            return freshResponse;
        }

        private async Task Save(HttpResponseMessage freshResponse, string playbackId)
        {
            string content = await freshResponse.Content.ReadAsStringAsync();
            await _playbackStorageService.UploadToStorageAsync(playbackId, content);
        }

        private async Task<HttpResponseMessage> Replay(string playbackId)
        {
            var m = await _playbackStorageService.DownloadFromStorageAsync(playbackId);
            string content = m.BodyString;
            var savedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            if (content == null)
            {
                return null;
            }
            savedResponse.Content = new StringContent(content);
            return savedResponse;
        }

        private void PerformPlaybackSimulationStrategy()
        {
            if (_config.ThrowException)
            {
                throw new PlaybackFakeException($"Fake Exception thrown by PlaybackHandler");

            }
            else if (_config.RetriesBeforeReturningSuccess > _failedCount)
            {
                _failedCount++;
                 throw new PlaybackFakeException($"Fake Exception thrown by PlaybackHandler after {_failedCount} retries");
            }
           
            _failedCount = 0;
        }
    }

    public class HttpClientFactory
    {                
        public static HttpClient WithPlaybackContext(
            IPlaybackContext playbackContext, 
            IPlaybackStorageService playbackStorageService,
            string prefix,
            IHttpClientPlaybackErrorSimulationService configService)
        {           
            var handler = new PlaybackHandler(playbackContext, playbackStorageService, prefix, configService);
            var httpClient = new HttpClient(handler);            
            return httpClient;
        }

        
    }
}
