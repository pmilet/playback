using System;

namespace pmilet.Playback
{
    /// <summary>
    /// Exception thrown during playback simulation for testing fault tolerance.
    /// </summary>
    public class PlaybackFakeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackFakeException"/> class.
        /// </summary>
        public PlaybackFakeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackFakeException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PlaybackFakeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackFakeException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public PlaybackFakeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}