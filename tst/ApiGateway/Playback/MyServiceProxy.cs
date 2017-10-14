using Microsoft.AspNetCore.Server.Kestrel.Internal;
using pmilet.Playback.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class MyServiceProxy
    {
        IPlaybackContext _playbackContext;
        IPlaybackStorageService _playbackStorageService;

        public MyServiceProxy()
        {
        }

        public void SetPlayback(IPlaybackContext serviceContext, IPlaybackStorageService playbackStorageService)
        {
            _playbackContext = serviceContext;
            _playbackStorageService = playbackStorageService;
        }

        public async Task<MyServiceResponse> Execute( MyServiceRequest command)
        {
           
            var result =  new MyServiceResponse() {  Response = command.Command + " OK" };
            if (_playbackContext.PlaybackMode == PlaybackMode.Grabacion)
            {
                var body = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                await _playbackStorageService.UploadToStorageAsync(_playbackContext.PlaybackId + "_" + this.GetType().Name, body);
            }
            else if
                (_playbackContext.PlaybackMode == PlaybackMode.Playback ||
                _playbackContext.PlaybackMode == PlaybackMode.PlaybackChaos ||
                _playbackContext.PlaybackMode == PlaybackMode.PlaybackReal)
            {
                return await _playbackStorageService.ReplayFromStorageAsync<MyServiceResponse>(_playbackContext.PlaybackMode, _playbackContext.PlaybackId + "_" + this.GetType().Name);
            }
            return result;
        }
    }
}
