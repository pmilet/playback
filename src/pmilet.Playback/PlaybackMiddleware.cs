// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using pmilet.Playback.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Net;
using System.Threading.Tasks;
using System;
using System.IO;

namespace pmilet.Playback
{
    public class PlaybackMiddleware
    {
        private readonly IFakeFactory _fakeFactory;
        private readonly IPlaybackStorageService _messageStorageService;
        protected readonly RequestDelegate _next;
        private readonly PlaybackContext _playbackContext;

        public PlaybackMiddleware(RequestDelegate next, IFakeFactory fakeFactory, IPlaybackStorageService messageStorageService, IPlaybackContext playbackContext)
        {
            _fakeFactory = fakeFactory;
            _messageStorageService = messageStorageService;
            _next = next;
            _playbackContext = playbackContext as PlaybackContext;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new Exception("null Http Context found when invoking Playback Middleware");

            _playbackContext.ReadHttpContext(httpContext);

            httpContext.Request.EnableRewind();

            switch (_playbackContext.Fake)
            {
                case "Inbound":
                    await FakeHandler(httpContext);
                    return;
                default:
                    break;
            }

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

        private async Task FakeHandler(HttpContext httpContext)
        {
            bool handled = false;
            try
            {
                handled = _fakeFactory.GenerateFakeResponse(httpContext);
            }
            catch (NotImplementedException ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;
                await httpContext.Response.WriteAsync(ex.Message);
                handled = true;
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync(ex.Message);
                handled = true;
            }

            if (!handled)
                await _next.Invoke(httpContext);
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
