using Iberfabric.Exceptions.Core;
using System;
using System.Diagnostics.Tracing;

namespace pmilet.Playback
{  
    public class PlaybackStorageException : Exception
     {
        public PlaybackStorageException(string playbackId, string message, Exception innerException) 
            : base ( message, innerException )
        {
            PlaybackId = playbackId;
        }

        public PlaybackStorageException(string playbackId,string message) 
            : base (message)
        {
            PlaybackId = playbackId;
        }

        public string PlaybackId { get; private set; } 
    }
}