// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pmilet.Playback.Core
{
    /// <summary>
    /// Defines the playback mode for HTTP request recording and replay.
    /// </summary>
    public enum PlaybackMode
    {
        /// <summary>
        /// Playback functionality is disabled.
        /// </summary>
        None,
        
        /// <summary>
        /// Record incoming and outgoing HTTP requests.
        /// </summary>
        Record,
        
        /// <summary>
        /// Replay recorded HTTP requests without simulating delays.
        /// </summary>
        Playback,
        
        /// <summary>
        /// Replay recorded HTTP requests with original response time delays.
        /// </summary>
        PlaybackReal,
        
        /// <summary>
        /// Replay recorded HTTP requests with randomized (chaos) delays.
        /// </summary>
        PlaybackChaos
    }
}
