﻿// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using pmilet.Playback.Core;
using Newtonsoft.Json;

namespace pmilet.Playback
{
    public class PlaybackBlobStorageService : IPlaybackStorageService
    {
        private string _containerName;
        private readonly string _connectionString;

        public PlaybackBlobStorageService(string blobStorageConnectionString, string blobStorageContainerName)
        {
            _connectionString = blobStorageConnectionString;
            _containerName = blobStorageContainerName;
        }

        public PlaybackBlobStorageService(IConfigurationRoot configuration)
        {
            var section = configuration.GetSection("PlaybackStorage");
            _connectionString = section.GetSection("ConnectionString").Value;
            _containerName = section.GetSection("Name").Value;
        }

        public async Task UploadToStorageAsync(string playbackId, string path, string queryString, string bodyString, long elapsedTime = 0)
        {
            if (string.IsNullOrWhiteSpace(playbackId))
                throw new PlaybackStorageException(playbackId, "playbackId not found");
            PlaybackMessage playbackMessage = new PlaybackMessage(path, queryString, bodyString, "text", elapsedTime);
            try
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

                // Create the container if it doesn't already exist.
                container.CreateIfNotExists();

                // Retrieve reference to a blob.
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(playbackId);

                await blockBlob.UploadTextAsync(JsonConvert.SerializeObject(playbackMessage));
                blockBlob.Properties.ContentType = playbackMessage.ContentType;
                blockBlob.SetProperties();
                foreach (string key in playbackMessage.Metadata.Keys)
                    blockBlob.Metadata.Add(key, playbackMessage.Metadata[key]);
                await blockBlob.SetMetadataAsync();
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId,"playback upload error", ex);
            }
        }

        public async Task UploadToStorageAsync(string fileId, string content, long elapsedTime = 0)
        {
            await UploadToStorageAsync(fileId, "", "", content, elapsedTime);
        }

        public async Task<T> ReplayFromStorageAsync<T>(PlaybackMode playbackMode, string playbackId)
        {
            string value = await ReplayFromStorageAsync(playbackMode, playbackId);
            return JsonConvert.DeserializeObject<T>( value);
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

        private async Task WaitFor(long responseTime)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(responseTime));
        }


        public async Task<PlaybackMessage> DownloadFromStorageAsync(string playbackId)
        {
            string contentType;
            string bodyString;

            try
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

                string text = string.Empty;
                if (!container.Exists()) return null;

                // Retrieve reference to a blob named "photo1.jpg".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(playbackId);

                using (var memoryStream = new MemoryStream())
                {
                    await blockBlob.DownloadToStreamAsync(memoryStream);
                    contentType = blockBlob.Properties.ContentType;
                    if (blockBlob.Properties.ContentType.Contains("text"))
                        bodyString = Encoding.UTF8.GetString(memoryStream.ToArray());
                    else
                    {
                        string encodedString = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var bytes = System.Convert.FromBase64String(encodedString);
                        bodyString = Encoding.Default.GetString(bytes);
                    }
                }
                return JsonConvert.DeserializeObject<PlaybackMessage>(bodyString);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error downloading file {0} from blob storage ", playbackId);
                throw new StorageException(errorMessage, ex);
            }
        }

        
    }
}