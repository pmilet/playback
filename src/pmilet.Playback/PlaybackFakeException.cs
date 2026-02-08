using System;

namespace pmilet.Playback
{
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
    }
}