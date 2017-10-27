// Copyright (c) 2017 Pierre Milet. All rights reserved.
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
    public class PlaybackBlobStorageService : PlaybackStorageServiceBase, IPlaybackStorageService
    {
        private string _containerName;
        private readonly string _connectionString;

        public PlaybackBlobStorageService(string blobStorageConnectionString, string blobStorageContainerName)
        {
            _connectionString = blobStorageConnectionString;
            _containerName = blobStorageContainerName;
        }
        public PlaybackBlobStorageService(IConfigurationRoot configuration) : this( (IConfiguration)configuration)
        {
        }

        public PlaybackBlobStorageService(IConfiguration configuration)
        {
            var section = configuration.GetSection("PlaybackBlobStorage");
            _connectionString = section.GetSection("ConnectionString").Value;
            _containerName = section.GetSection("ContainerName").Value;
        }

        public async override Task UploadToStorageAsync(string playbackId, string path, string queryString, string bodyString, long elapsedTime = 0)
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
                await container.CreateIfNotExistsAsync();

                // Retrieve reference to a blob.
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(playbackId);

                await blockBlob.UploadTextAsync(JsonConvert.SerializeObject(playbackMessage));
                foreach (string key in playbackMessage.Metadata.Keys)
                    blockBlob.Metadata.Add(key, playbackMessage.Metadata[key]);
                await blockBlob.SetMetadataAsync();
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId,"playback upload error", ex);
            }
        }

    
        public async override Task<PlaybackMessage> DownloadFromStorageAsync(string playbackId)
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
                if (! await container.ExistsAsync()) return null;

                // Retrieve reference to a blob named "photo1.jpg".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(playbackId);

                using (var memoryStream = new MemoryStream())
                {
                    await blockBlob.DownloadToStreamAsync(memoryStream);
                    bodyString = Encoding.UTF8.GetString(memoryStream.ToArray());
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
