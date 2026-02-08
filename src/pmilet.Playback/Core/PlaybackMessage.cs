// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace pmilet.Playback.Core
{
    public class PlaybackMessage
    {
        public PlaybackMessage(string path, string queryString, string bodyString, string contentType, long elapsedTime)
        {
            Path = path;
            QueryString = queryString;
            BodyString = bodyString;
            ContentType = contentType;
            Metadata = new Dictionary<string, string>();
            Metadata.Add("responseTime", elapsedTime.ToString());
        }

        public string Path { get; set; }

        public string BodyString { get; set; }

        public string QueryString { get; set; }

        public string ContentType { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public long ResponseTime
        {
            get
            {
                if (Metadata.ContainsKey("responseTime"))
                {
                    long value = 0;
                    long.TryParse(Metadata["responseTime"], out value);
                    return value;
                }
                return 0;
            }
        }

        public MemoryStream GetBodyStream()
        {
            return BodyString != null ? new MemoryStream(System.Text.Encoding.UTF8.GetBytes(BodyString)) : new MemoryStream();
        }
    }
}
