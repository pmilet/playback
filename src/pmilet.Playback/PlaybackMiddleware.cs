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
        protected readonly RequestDelegate _next;
        private readonly PlaybackContext _playbackContext;

        public PlaybackMiddleware(RequestDelegate next, IPlaybackStorageService messageStorageService, IPlaybackContext playbackContext)
        {
            _messageStorageService = messageStorageService;
            _next = next;
            _playbackContext = playbackContext as PlaybackContext;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new Exception("null Http Context found when invoking Playback Middleware");

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
            var pathDecode = WebUtility.UrlDecode(httpContext.Request.Path);
            var playbackId = _playbackContext.GenerateNewPlaybackId();
            PlaybackEventSource.Current.NewPlaybackMessage("Record Playback", playbackId);

            await _messageStorageService.UploadToStorageAsync(_playbackContext.PlaybackId, pathDecode, httpContext.Request.QueryString.Value, _playbackContext.Content);
            httpContext.Request.Body.Position = 0;
            httpContext.Response.OnStarting(state =>
            {
                var httpContextState = (HttpContext)state;
                httpContextState.Response.Headers.Add("X-Playback-Id", new[] { _playbackContext.PlaybackId });
                return Task.FromResult(0);
            }, httpContext);
            await _next.Invoke(httpContext);
        }

        private readonly RequestDelegate next;
        private async Task<string> ReadBody(HttpContext context)
        {
            Stream originalBody = context.Response.Body;
            string responseBody = null;
            try
            {
                using (var memStream = new MemoryStream())
                {
                    context.Response.Body = memStream;

                    await next(context);

                    memStream.Position = 0;
                    responseBody = new StreamReader(memStream).ReadToEnd();

                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                }
                return responseBody;
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private async Task PlaybackHandler(HttpContext httpContext)
        {
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
