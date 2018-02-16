using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pmilet.Playback;

namespace pmilet.Playback
{
    public class HttpClientPlaybackErrorSimulationService : IHttpClientPlaybackErrorSimulationService
    {
        private Dictionary<string, HttpClientPlaybackErrorSimulationConfig> dict  = new Dictionary<string, HttpClientPlaybackErrorSimulationConfig>();
        public HttpClientPlaybackErrorSimulationService()
        {
            dict.Add("default", new HttpClientPlaybackErrorSimulationConfig("default",0,false));
        }

        public IDictionary<string, HttpClientPlaybackErrorSimulationConfig> HttpClientPlaybackErrorSimulationConfigs { get { return dict; } }

        public Task<HttpClientPlaybackErrorSimulationConfig> GetDefaultConfig() { return Task.FromResult(dict["default"]); }

        public Task<HttpClientPlaybackErrorSimulationConfig> GetNamedConfig(string name)
        {
            return Task.FromResult(dict.Where(c => c.Key == name).SingleOrDefault().Value?? dict["default"]);
        }

        public Task ChangeAll( HttpClientPlaybackErrorSimulationConfig newConfig)
        {
            foreach( var c in dict.ToArray())
            {
                dict[c.Key] = newConfig;
            }
            return Task.CompletedTask;
        }

        public Task AddOrUpdate(string name, HttpClientPlaybackErrorSimulationConfig newConfig)
        {
            if (dict.ContainsKey(name))
            {
                dict[name] = newConfig;
            }
            else
            {
                dict.Add(name, newConfig);
            }
            return Task.CompletedTask;
        }        
    }
}
