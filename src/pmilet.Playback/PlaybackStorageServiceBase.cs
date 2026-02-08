// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Newtonsoft.Json;
using pmilet.Playback.Core;
using System;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public abstract class PlaybackStorageServiceBase
    {
        public async Task UploadToStorageAsync(string fileId, string content, long elapsedTime = 0)
        {
            await UploadToStorageAsync(fileId, "", "", content, elapsedTime);
        }

        public async Task UploadToStorageAsync(string fileId, object obj, long elapsedTime = 0)
        {
            var content = JsonConvert.SerializeObject(obj);
            await UploadToStorageAsync(fileId, content, elapsedTime);
        }

        public abstract Task UploadToStorageAsync(string playbackId, string path, string queryString, string bodyString, long elapsedTime = 0);

        public async Task<T> ReplayFromStorageAsync<T>(string playbackId)
        {
            var message = await DownloadFromStorageAsync(playbackId);
            var result = JsonConvert.DeserializeObject<T>(message.BodyString);
            if (result == null)
                throw new PlaybackStorageException(playbackId, "Failed to deserialize playback message");
            return result;
        }

        public async Task<T> ReplayFromStorageAsync<T>(PlaybackMode playbackMode, string playbackId)
        {
            string value = await ReplayFromStorageAsync(playbackMode, playbackId);
            var result = JsonConvert.DeserializeObject<T>(value);
            if (result == null)
                throw new PlaybackStorageException(playbackId, "Failed to deserialize playback message");
            return result;
        }

        public async Task<string> ReplayFromStorageAsync(PlaybackMode playbackMode, string playbackId)
        {
            var fileInfo = await DownloadFromStorageAsync(playbackId);
            switch (playbackMode)
            {
                case PlaybackMode.PlaybackReal:
                    await WaitFor(fileInfo.ResponseTime);
                    break;
                case PlaybackMode.PlaybackChaos:
                    long min = fileInfo.ResponseTime;
                    long max = 15000;
                    long mean = (long)((max - fileInfo.ResponseTime) / 2.0);
                    min = mean < min ? mean : min;
                    max = max < mean ? mean : max;
                    var rt = (long)RandomGaussian.NextInRange(min, mean, max);
                    await WaitFor(rt);
                    break;
                default:
                    break;
            }
            return fileInfo.BodyString;
        }

        public abstract Task<PlaybackMessage> DownloadFromStorageAsync(string playbackId);

        private async Task WaitFor(long responseTime)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(responseTime));
        }

    }
}