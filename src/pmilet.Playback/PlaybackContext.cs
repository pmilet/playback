// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using pmilet.Playback.Core;
using System.Threading.Tasks;
using System.Net;

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
            if (accessor == null)
                throw new Exception("null Http Context accessor when creating Playback Context");

            if (accessor.HttpContext != null)
                ReadHttpContext(accessor.HttpContext);

            _playbackStorageService = playbackStorageService;
        }

        public static void ChangePlaybackMode( PlaybackMode newPlaybackMode)
        {
            DefaultPlaybackMode = newPlaybackMode;
            PlaybackEventSource.Current.PlaybackModeChanged(newPlaybackMode);
        }

        public static void ChangePlaybackRequestContext( string newRequestPlaybackContext )
        {
            DefaultPlaybackRequestContext = newRequestPlaybackContext;
            PlaybackEventSource.Current.PlaybackRequestContextChanged(newRequestPlaybackContext);
        }

        public static void ChangePlaybackFake(string newPlaybackFake)
        {
            DefaultPlaybackFake = newPlaybackFake;
            PlaybackEventSource.Current.PlaybackFakeChanged(newPlaybackFake);
        }

        public static string DefaultPlaybackRequestContext { get; set; }

        public static PlaybackMode DefaultPlaybackMode {get;  internal set;}

        public static string DefaultPlaybackFake { get; internal set; }

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
            PlaybackMode = DefaultPlaybackMode;
            _context = context;
            Microsoft.Extensions.Primitives.StringValues headerValues;

            var keyfound = context.Request.Headers.TryGetValue("X-Playback-RequestContext", out headerValues);
            RequestContextInfo = keyfound ? RequestContextInfo = headerValues.FirstOrDefault() : DefaultPlaybackRequestContext;
            
            keyfound = _context.Request.Headers.TryGetValue("X-Playback-Mode", out headerValues);
            PlaybackMode pbm = PlaybackMode.None;
            if (keyfound)
            {
                Enum.TryParse<PlaybackMode>(headerValues.FirstOrDefault(), out pbm);
                PlaybackMode = pbm;
            }

            keyfound = context.Request.Headers.TryGetValue("X-Playback-Fake", out headerValues);
            Fake = keyfound ? Fake = headerValues.FirstOrDefault() : DefaultPlaybackFake;

            keyfound = _context.Request.Headers.TryGetValue("X-Playback-Version", out headerValues);
            Version = keyfound ? headerValues.FirstOrDefault() : string.Empty;

            keyfound = _context.Request.Headers.TryGetValue("X-Playback-Id", out headerValues);
            PlaybackId = keyfound ? headerValues.FirstOrDefault() : string.Empty;
        }

        internal string GenerateNewPlaybackId()
        {
            if(_context == null )
                throw new Exception("null HttpContext when generating new playback Id");

            _context.Request.EnableRewind();
            _requestBodyString = ReadToEnd(_context.Request.Body);
            PlaybackId = WebUtility.UrlEncode(RequestContextInfo + "_" + _assemblyName + "_" + "v" + Version + "_" + RequestPath + "_" + RequestMethod + "_" + RequestContentHashCode);

            return PlaybackId;
        }

        internal string Content
        {
            get
            {
               
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
            get { return WebUtility.UrlEncode(_context.Request.Path.Value.Replace("api", "").Trim('/')); }
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

        private string RequestContextInfo
        {
            get;set;
        }

        public string Fake
        {
            get; set;
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