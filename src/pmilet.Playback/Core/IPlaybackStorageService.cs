// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace pmilet.Playback.Core
{
    /// <summary>
    /// Defines storage operations for playback messages.
    /// </summary>
    public interface IPlaybackStorageService
    {
        /// <summary>
        /// Downloads a playback message from storage.
        /// </summary>
        /// <param name="fileId">The unique identifier of the playback message.</param>
        /// <returns>The playback message.</returns>
        Task<PlaybackMessage> DownloadFromStorageAsync(string fileId);
        
        /// <summary>
        /// Uploads a playback message to storage with full request details.
        /// </summary>
        /// <param name="fileId">The unique identifier for the playback message.</param>
        /// <param name="path">The request path.</param>
        /// <param name="queryString">The request query string.</param>
        /// <param name="bodyString">The request body as a string.</param>
        /// <param name="elapsedTime">The elapsed time in milliseconds.</param>
        Task UploadToStorageAsync(string fileId, string path, string queryString, string bodyString, long elapsedTime = 0);
        
        /// <summary>
        /// Uploads content to storage with a simple content string.
        /// </summary>
        /// <param name="fileId">The unique identifier for the playback message.</param>
        /// <param name="content">The content to upload.</param>
        /// <param name="elapsedTime">The elapsed time in milliseconds.</param>
        Task UploadToStorageAsync(string fileId, string content, long elapsedTime = 0);
        
        /// <summary>
        /// Uploads an object to storage by serializing it to JSON.
        /// </summary>
        /// <param name="fileId">The unique identifier for the playback message.</param>
        /// <param name="obj">The object to serialize and upload.</param>
        /// <param name="elapsedTime">The elapsed time in milliseconds.</param>
        Task UploadToStorageAsync(string fileId, object obj, long elapsedTime = 0);
        
        /// <summary>
        /// Replays content from storage and deserializes it.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="fileId">The unique identifier of the playback message.</param>
        /// <returns>The deserialized object.</returns>
        Task<T> ReplayFromStorageAsync<T>(string fileId);
        
        /// <summary>
        /// Replays content from storage with playback mode simulation.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="playbackMode">The playback mode to use for simulation.</param>
        /// <param name="fileId">The unique identifier of the playback message.</param>
        /// <returns>The deserialized object.</returns>
        Task<T> ReplayFromStorageAsync<T>(PlaybackMode playbackMode, string fileId);
        
        /// <summary>
        /// Replays content from storage with playback mode simulation.
        /// </summary>
        /// <param name="playbackMode">The playback mode to use for simulation.</param>
        /// <param name="fileId">The unique identifier of the playback message.</param>
        /// <returns>The content as a string.</returns>
        Task<string> ReplayFromStorageAsync(PlaybackMode playbackMode, string fileId);
    }
}