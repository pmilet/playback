// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Diagnostics.Tracing;

namespace pmilet.HttpPlayback
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