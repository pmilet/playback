using pmilet.Playback;
using pmilet.Playback.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApi.Controllers
{
    public class PlaybackService : Service, IService
    {
        private const string PROXY_NAME = "MyServiceProxy";
        readonly IPlaybackContext _playbackContext;
        readonly IPlaybackStorageService _playbackStorageService;
        private readonly IHttpClientPlaybackErrorSimulationService _configService;

        public PlaybackService(IPlaybackContext playbackContext, IPlaybackStorageService playbackStorageService, IHttpClientPlaybackErrorSimulationService configService) :
            base(new System.Net.Http.HttpClient())
        {
            _playbackContext = playbackContext;
            _playbackStorageService = playbackStorageService;
            _configService = configService;
            base.HttpClient = HttpClientFactory.WithPlaybackContext(playbackContext, playbackStorageService, PROXY_NAME, configService);
        }
    }
}
