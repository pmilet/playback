// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;

namespace pmilet.Playback.Core
{

    public interface IPlaybackContext
    {
        string PlaybackId { get; }
        PlaybackMode PlaybackMode { get; }
        string Content { get; }
        void ReadHttpContext();
        void ReadHttpContext(HttpContext context);
        IPlaybackStorageService PlaybackStorageService { get; }
        string GenerateNewPlaybackId();        
    }
}