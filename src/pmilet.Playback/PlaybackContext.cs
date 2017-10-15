// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using pmilet.Playback.Core;

namespace pmilet.Playback
{
    public class PlaybackContext : IPlaybackContext
    {
        private static string assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty;

        private HttpContext _context;

        private string _playbackId = string.Empty;

        private string _requestBodyString = string.Empty;

        private string _queryString = string.Empty;

        private readonly IHttpContextAccessor _accessor;

        private readonly IPlaybackStorageService _playbackStorageService;

        public PlaybackContext(IHttpContextAccessor accessor, IPlaybackStorageService playbackStorageService)
        {
            _accessor = accessor;
            _playbackStorageService = playbackStorageService;
        }

        public void ReadHttpContext( )
        {
            ReadHttpContext(_accessor.HttpContext);
        }

        public void ReadHttpContext(HttpContext context)
        {
            _context = context;
            Microsoft.Extensions.Primitives.StringValues headerValues;

            var keyfound = context.Request.Headers.TryGetValue("PlaybackRequestContext", out headerValues);
            if (keyfound)
            {
                ContextInfo = headerValues.FirstOrDefault();
            }

            keyfound = _context.Request.Headers.TryGetValue("PlaybackMode", out headerValues);
            if (keyfound)
            {
                PlaybackMode pbm = PlaybackMode.None;
                Enum.TryParse<PlaybackMode>(headerValues.FirstOrDefault(), out pbm);
                PlaybackMode = pbm;
            }

            keyfound = _context.Request.Headers.TryGetValue("PlaybackVersion", out headerValues);
            if (keyfound)
                Version = headerValues.FirstOrDefault();

            keyfound = _context.Request.Headers.TryGetValue("PlaybackId", out headerValues);
            if (keyfound)
                PlaybackId = headerValues.FirstOrDefault();
        }

        public string PlaybackId
        {
            get
            {
                if (string.IsNullOrEmpty(_playbackId))
                    return GenerateNewPlaybackId();
                else
                    return _playbackId;
            }
            set { _playbackId = value; }
        }

        public string GenerateNewPlaybackId()
        {
            return ContextInfo + "_" + AssemblyName + "_" + "v" + Version + "_" + RequestPath + "_" + RequestMethod + "_" + RequestContentHashCode;
        }

        public PlaybackMode PlaybackMode
        {
            get; private set;
        }

        public string Content
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
        
        private static string AssemblyName
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Name ?? string.Empty;
            }
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