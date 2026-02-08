// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;

namespace pmilet.Playback
{
    /// <summary>
    /// Exception thrown when storage operations fail during playback recording or replay.
    /// </summary>
    public class PlaybackStorageException : PlaybackFakeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackStorageException"/> class with a playback ID, message, and inner exception.
        /// </summary>
        /// <param name="playbackId">The playback identifier associated with the failed operation.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PlaybackStorageException(string playbackId, string message, Exception innerException)
            : base(message, innerException)
        {
            PlaybackId = playbackId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackStorageException"/> class with a playback ID and message.
        /// </summary>
        /// <param name="playbackId">The playback identifier associated with the failed operation.</param>
        /// <param name="message">The error message.</param>
        public PlaybackStorageException(string playbackId, string message)
            : base(message)
        {
            PlaybackId = playbackId;
        }

        /// <summary>
        /// Gets the playback identifier associated with this exception.
        /// </summary>
        public string PlaybackId { get; private set; }
    }
}