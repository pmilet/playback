// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace pmilet.Playback.Core
{
    /// <summary>
    /// Provides context information for the current playback session.
    /// </summary>
    public interface IPlaybackContext
    {
        /// <summary>
        /// Gets the unique identifier for the current playback session.
        /// </summary>
        string PlaybackId { get; }
        
        /// <summary>
        /// Gets the current playback mode.
        /// </summary>
        PlaybackMode PlaybackMode { get; }
        
        /// <summary>
        /// Records a result object for later playback.
        /// </summary>
        /// <typeparam name="T">The type of the result to record.</typeparam>
        /// <param name="result">The result object to record.</param>
        /// <param name="fileNameOverride">Optional custom file name for storage.</param>
        Task RecordResult<T>(T result, string? fileNameOverride = null);
        
        /// <summary>
        /// Retrieves a previously recorded result.
        /// </summary>
        /// <typeparam name="T">The type of the result to retrieve.</typeparam>
        /// <param name="fileNameOverride">Optional custom file name for retrieval.</param>
        /// <returns>The recorded result.</returns>
        Task<T> PlaybackResult<T>(string? fileNameOverride = null);
        
        /// <summary>
        /// Gets a value indicating whether the current mode is a playback mode.
        /// </summary>
        bool IsPlayback { get; }
        
        /// <summary>
        /// Gets a value indicating whether the current mode is record mode.
        /// </summary>
        bool IsRecord { get; }
    }
}