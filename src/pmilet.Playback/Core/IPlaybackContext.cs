// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace pmilet.Playback.Core
{

    public interface IPlaybackContext
    {
        string PlaybackId { get; }
        string Fake{ get; }
        PlaybackMode PlaybackMode { get; }
        Task RecordResult<T>(T result, string fileNameOverride = null);
        Task<T> PlaybackResult<T>(string fileNameOverride = null);
        bool IsPlayback { get; }
        bool IsRecord { get; }

    }
}