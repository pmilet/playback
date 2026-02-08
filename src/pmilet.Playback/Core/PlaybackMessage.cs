// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace pmilet.Playback.Core
{
    /// <summary>
    /// Represents a recorded HTTP request/response message for playback.
    /// </summary>
    public class PlaybackMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackMessage"/> class.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="bodyString">The request/response body as a string.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="elapsedTime">The elapsed time in milliseconds.</param>
        public PlaybackMessage(string path, string queryString, string bodyString, string contentType, long elapsedTime)
        {
            Path = path;
            QueryString = queryString;
            BodyString = bodyString;
            ContentType = contentType;
            Metadata = new Dictionary<string, string>();
            Metadata.Add("responseTime", elapsedTime.ToString());
        }

        /// <summary>
        /// Gets or sets the request path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the request/response body as a string.
        /// </summary>
        public string BodyString { get; set; }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the playback message.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Gets the response time in milliseconds from metadata.
        /// </summary>
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

        /// <summary>
        /// Converts the body string to a memory stream.
        /// </summary>
        /// <returns>A memory stream containing the body content.</returns>
        public MemoryStream GetBodyStream()
        {
            return BodyString != null ? new MemoryStream(System.Text.Encoding.UTF8.GetBytes(BodyString)) : new MemoryStream();
        }
    }
}
