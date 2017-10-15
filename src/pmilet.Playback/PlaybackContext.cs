// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using pmilet.Playback.Core;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public class PlaybackContext : IPlaybackContext
    {
        private static string _assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty;

        private HttpContext _context;

        private string _playbackId = string.Empty;

        private string _requestBodyString = string.Empty;

        private string _queryString = string.Empty;

        private readonly IPlaybackStorageService _playbackStorageService;

        public PlaybackContext(IHttpContextAccessor accessor, IPlaybackStorageService playbackStorageService)
        {
            if (accessor?.HttpContext != null)
                ReadHttpContext(accessor.HttpContext);

            _playbackStorageService = playbackStorageService;
        }

        public string PlaybackId
        {
            get
            {
                if (string.IsNullOrEmpty(_playbackId))
                    GenerateNewPlaybackId();
                return _playbackId;
            }
            set { _playbackId = value; }
        }

        public PlaybackMode PlaybackMode
        {
            get; private set;
        }

        public bool IsPlayback { get{ return PlaybackMode == PlaybackMode.Playback ||
                 PlaybackMode == PlaybackMode.PlaybackChaos ||
                 PlaybackMode == PlaybackMode.PlaybackReal; } }

        public bool IsRecord{ get{ return PlaybackMode == PlaybackMode.Record; } }

        private string DefaultFileName<T>() { return PlaybackId + "_" + typeof(T).Name; }

        public async Task RecordResult<T>(T result, string fileNameOverride =null )
        {
            string fileName = fileNameOverride == null ? DefaultFileName<T>() : fileNameOverride;
            var body = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            await _playbackStorageService.UploadToStorageAsync( fileName, body);
        }

        public async Task<T> PlaybackResult<T>(string fileNameOverride = null)
        {
            string fileName = fileNameOverride == null ? DefaultFileName<T>() : fileNameOverride;
            return await _playbackStorageService.ReplayFromStorageAsync<T>(PlaybackMode, fileName );
        }

        internal void ReadHttpContext(HttpContext context)
        {
            _context = context;
            Microsoft.Extensions.Primitives.StringValues headerValues;

            var keyfound = context.Request.Headers.TryGetValue("X-Playback-RequestContext", out headerValues);
            if (keyfound)
            {
                ContextInfo = headerValues.FirstOrDefault();
            }

            keyfound = _context.Request.Headers.TryGetValue("X-Playback-Mode", out headerValues);
            if (keyfound)
            {
                PlaybackMode pbm = PlaybackMode.None;
                Enum.TryParse<PlaybackMode>(headerValues.FirstOrDefault(), out pbm);
                PlaybackMode = pbm;
            }

            keyfound = _context.Request.Headers.TryGetValue("X-Playback-Version", out headerValues);
            if (keyfound)
                Version = headerValues.FirstOrDefault();
            else
                Version = "1.0";

            keyfound = _context.Request.Headers.TryGetValue("X-Playback-Id", out headerValues);
            if (keyfound)
                PlaybackId = headerValues.FirstOrDefault();
        }

        internal void GenerateNewPlaybackId()
        {
            PlaybackId =  ContextInfo + "_" + _assemblyName + "_" + "v" + Version + "_" + RequestPath + "_" + RequestMethod + "_" + RequestContentHashCode;
        }

        internal string Content
        {
            get
            {
                if (string.IsNullOrEmpty(_requestBodyString))
                {
                    _context.Request.EnableRewind();
                    _requestBodyString = ReadToEnd(_context.Request.Body);
                }
                return _requestBodyString;
            }
        }

        public IPlaybackStorageService PlaybackStorageService => _playbackStorageService;

        private string RequestMethod
        {
            get { return _context.Request.Method; }
        }

        private string RequestPath
        {
            //TODO: check
            get { return _context.Request.Path.Value.Replace("api", "").Trim('/'); }
        }

        private string Version
        {
            get;set;
        }

        private string RequestContentHashCode
        {
            get
            {
                return !string.IsNullOrEmpty(Content) ? Content.GetHashCode().ToString() : QueryString.GetHashCode().ToString();
            }
        }

        private string ContextInfo
        {
            get;set;
        }
        
        private string QueryString
        {
            get
            {
                if (string.IsNullOrEmpty(_queryString))
                    _queryString = _context.Request.QueryString.HasValue ? _context.Request.QueryString.Value : string.Empty;
                return _requestBodyString;
            }
        }

        private string ReadToEnd(Stream bodyStream)
        {
            bodyStream.Position = 0;
            StreamReader sr = new StreamReader(bodyStream);
            return sr.ReadToEnd();
        }
    }
}