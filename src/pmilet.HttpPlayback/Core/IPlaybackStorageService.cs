// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace pmilet.HttpPlayback.Core
{
    public interface IPlaybackStorageService
    {
        Task<PlaybackMessage> DownloadFromStorageAsync(string fileId);
        Task UploadToStorageAsync(string fileId, string path, string queryString, string bodyString, long elapsedTime = 0);
        Task UploadToStorageAsync(string fileId, string content, long elapsedTime = 0);
        Task UploadToStorageAsync(string fileId, object obj, long elapsedTime = 0);
        Task<T> ReplayFromStorageAsync<T>(string fileId);
        Task<T> ReplayFromStorageAsync<T>(PlaybackMode playbackMode, string fileId);
        Task<string> ReplayFromStorageAsync(PlaybackMode playbackMode, string fileId);
    }
}