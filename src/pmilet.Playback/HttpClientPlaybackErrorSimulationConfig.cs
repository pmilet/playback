using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public class HttpClientPlaybackErrorSimulationConfig
    {
        public HttpClientPlaybackErrorSimulationConfig( string name, uint retries, bool throwError)
        {
            Name = name;
            RetriesBeforeReturningSuccess = retries;
            ThrowException = throwError;
        }

        public string Name { get; private set; }

        public uint RetriesBeforeReturningSuccess { get; private set; }

        public bool ThrowException { get; private set; }

    }
}
