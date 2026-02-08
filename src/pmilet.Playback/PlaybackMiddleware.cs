// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Http.Features;

namespace pmilet.Playback
{
    public class PlaybackMiddleware
    {
        private readonly IPlaybackStorageService _messageStorageService;
        private readonly RequestDelegate _next;
        private readonly PlaybackContext? _playbackContext;

        public PlaybackMiddleware(RequestDelegate next, IPlaybackStorageService messageStorageService, IPlaybackContext playbackContext)
        {
            _messageStorageService = messageStorageService;
            _next = next;
            _playbackContext = playbackContext as PlaybackContext;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext), "null Http Context found when invoking Playback Middleware");

            if (_playbackContext == null)
                throw new InvalidOperationException("PlaybackContext must be of type PlaybackContext");

            _playbackContext.ReadHttpContext(httpContext);

            var syncIOFeature = httpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }

            httpContext.Request.EnableBuffering();     

            switch (_playbackContext.PlaybackMode)
            {
                case PlaybackMode.Record:
                    await RecordHandler(httpContext);
                    break;
                case PlaybackMode.Playback:
                case PlaybackMode.PlaybackReal:
                case PlaybackMode.PlaybackChaos:
                    await PlaybackHandler(httpContext);
                    break;
                default:
                    await _next.Invoke(httpContext);
                    break;
            }
        }

        private async Task RecordHandler(HttpContext httpContext)
        {
            if (_playbackContext == null)
                throw new InvalidOperationException("PlaybackContext is null");

            var pathDecode = WebUtility.UrlDecode(httpContext.Request.Path);
            var playbackId = _playbackContext.GenerateNewPlaybackId();
            PlaybackEventSource.Current.NewPlaybackMessage("Record Playback", playbackId);

            await _messageStorageService.UploadToStorageAsync(_playbackContext.PlaybackId, pathDecode, httpContext.Request.QueryString.Value ?? string.Empty, _playbackContext.Content);
            httpContext.Request.Body.Position = 0;
            httpContext.Response.OnStarting(state =>
            {
                var httpContextState = (HttpContext)state;
                httpContextState.Response.Headers["X-Playback-Id"] = _playbackContext.PlaybackId;
                return Task.CompletedTask;
            }, httpContext);
            await _next.Invoke(httpContext);
        }

        private async Task PlaybackHandler(HttpContext httpContext)
        {
            if (_playbackContext == null)
                throw new InvalidOperationException("PlaybackContext is null");

            if (!string.IsNullOrWhiteSpace(_playbackContext.PlaybackId))
            {
                PlaybackMessage playbackMessage = await _messageStorageService.DownloadFromStorageAsync(_playbackContext.PlaybackId);
                httpContext.Request.Body = playbackMessage.GetBodyStream();
                httpContext.Request.QueryString = new QueryString(playbackMessage.QueryString);
                var path = WebUtility.UrlDecode(playbackMessage.Path);
                httpContext.Request.Path = path;
            }
            await _next.Invoke(httpContext);
        }
    }
}
