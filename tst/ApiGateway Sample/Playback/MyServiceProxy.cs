using pmilet.Playback.Core;
using System.Threading.Tasks;

namespace ApiGateway_Sample
{
    public class MyServiceRequest
    {
        public string Input { get; set; }
    }

    public class MyServiceResponse
    {
        public string Output { get; set; }
    }

    public class MyServiceProxy
    {
        IPlaybackContext _playbackContext;
        public MyServiceProxy(IPlaybackContext context )
        {
            _playbackContext = context;
        }

        public async Task<MyServiceResponse> Execute( MyServiceRequest command)
        {
            var result =  new MyServiceResponse() {  Output = $"MyService received input: {command.Input}" };
            if (_playbackContext.IsRecord)
            {
                await _playbackContext.RecordResult<MyServiceResponse>(result);
            }
            else if ( _playbackContext.IsPlayback )
            {
                return await _playbackContext.PlaybackResult<MyServiceResponse>();
            }
            return result;
        }
    }
}
