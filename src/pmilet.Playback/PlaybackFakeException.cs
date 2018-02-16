using System;
using System.Runtime.Serialization;

namespace pmilet.Playback
{
    [Serializable]
    public class PlaybackFakeException : Exception
    {
        public PlaybackFakeException()
        {
        }

        public PlaybackFakeException(string message) : base(message)
        {
        }

        public PlaybackFakeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PlaybackFakeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}