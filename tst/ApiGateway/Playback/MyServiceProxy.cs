using pmilet.Playback.Core;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class MyServiceProxy
    {
        IPlaybackContext _playbackContext;

        public MyServiceProxy(IPlaybackContext context )
        {
            _playbackContext = context;
            _playbackContext.ReadHttpContext();
        }

        public async Task<MyServiceResponse> Execute( MyServiceRequest command)
        {
            var result =  new MyServiceResponse() {  Response = command.Command + " OK" };
            if (_playbackContext.PlaybackMode == PlaybackMode.Record)
            {
                var body = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                await _playbackContext.PlaybackStorageService.UploadToStorageAsync(_playbackContext.PlaybackId + "_" + this.GetType().Name, body);
            }
            else if (_playbackContext.PlaybackMode == PlaybackMode.Playback ||
                _playbackContext.PlaybackMode == PlaybackMode.PlaybackChaos ||
                _playbackContext.PlaybackMode == PlaybackMode.PlaybackReal)
            {
                return await _playbackContext.PlaybackStorageService.ReplayFromStorageAsync<MyServiceResponse>(_playbackContext.PlaybackMode, _playbackContext.PlaybackId + "_" + this.GetType().Name);
            }
            return result;
        }
    }
}
