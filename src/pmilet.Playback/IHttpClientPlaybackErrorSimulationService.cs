using pmilet.Playback;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public interface IHttpClientPlaybackErrorSimulationService
    {
        IDictionary<string,HttpClientPlaybackErrorSimulationConfig> HttpClientPlaybackErrorSimulationConfigs { get;  }

        Task AddOrUpdate(string name, HttpClientPlaybackErrorSimulationConfig newConfig );

        Task ChangeAll(HttpClientPlaybackErrorSimulationConfig newConfig);

        Task<HttpClientPlaybackErrorSimulationConfig> GetDefaultConfig();

        Task<HttpClientPlaybackErrorSimulationConfig> GetNamedConfig(string v);
    }
}