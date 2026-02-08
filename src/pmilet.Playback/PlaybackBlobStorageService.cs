// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Threading.Tasks;
using pmilet.Playback.Core;
using Newtonsoft.Json;

namespace pmilet.Playback
{
    /// <summary>
    /// Provides Azure Blob Storage-based storage for playback messages.
    /// </summary>
    public class PlaybackBlobStorageService : PlaybackStorageServiceBase, IPlaybackStorageService
    {
        private string _containerName = "playback";
        private readonly string _connectionString = "DefaultEndpointsProtocol = https; AccountName=XXXXXXXXX;AccountKey=XXXXXXXXX;EndpointSuffix=core.windows.net";

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackBlobStorageService"/> class.
        /// </summary>
        /// <param name="blobStorageConnectionString">The Azure Storage connection string.</param>
        /// <param name="blobStorageContainerName">The blob container name.</param>
        public PlaybackBlobStorageService(string blobStorageConnectionString, string blobStorageContainerName)
        {
            _connectionString = blobStorageConnectionString;
            _containerName = blobStorageContainerName;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackBlobStorageService"/> class from configuration root.
        /// </summary>
        /// <param name="configuration">The configuration root.</param>
        public PlaybackBlobStorageService(IConfigurationRoot configuration) : this((IConfiguration)configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackBlobStorageService"/> class from configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when required configuration is missing.</exception>
        public PlaybackBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("PlaybackStorage").GetSection("ConnectionString").Value 
                ?? throw new ArgumentNullException(nameof(configuration), "PlaybackStorage:ConnectionString is required");
            _containerName = configuration.GetSection("PlaybackStorage").GetSection("ContainerName").Value 
                ?? throw new ArgumentNullException(nameof(configuration), "PlaybackStorage:ContainerName is required");
        }

        public async override Task UploadToStorageAsync(string playbackId, string path, string queryString, string bodyString, long elapsedTime = 0)
        {
            if (string.IsNullOrWhiteSpace(playbackId))
                throw new PlaybackStorageException(playbackId, "playbackId not found");
            
            PlaybackMessage playbackMessage = new PlaybackMessage(path, queryString, bodyString, "text", elapsedTime);
            
            try
            {
                // Create BlobServiceClient from connection string
                var blobServiceClient = new BlobServiceClient(_connectionString);
                
                // Get container reference and create if not exists
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                // Get blob reference and upload content
                var blobClient = containerClient.GetBlobClient(playbackId);
                
                string content = JsonConvert.SerializeObject(playbackMessage);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                
                // Upload with metadata
                var metadata = new Dictionary<string, string>(playbackMessage.Metadata);
                var options = new BlobUploadOptions
                {
                    Metadata = metadata
                };
                
                await blobClient.UploadAsync(stream, options);
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId, "playback upload error", ex);
            }
        }


        public async override Task<PlaybackMessage> DownloadFromStorageAsync(string playbackId)
        {
            try
            {
                // Create BlobServiceClient from connection string
                var blobServiceClient = new BlobServiceClient(_connectionString);
                
                // Get container reference
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Check if container exists
                if (!await containerClient.ExistsAsync())
                {
                    throw new PlaybackStorageException(playbackId, "Container does not exist");
                }

                // Get blob reference and download
                var blobClient = containerClient.GetBlobClient(playbackId);
                
                if (!await blobClient.ExistsAsync())
                {
                    throw new PlaybackStorageException(playbackId, "Playback blob not found");
                }

                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                
                string bodyString = Encoding.UTF8.GetString(memoryStream.ToArray());
                var result = JsonConvert.DeserializeObject<PlaybackMessage>(bodyString);
                
                if (result == null)
                {
                    throw new PlaybackStorageException(playbackId, "Failed to deserialize playback message");
                }
                
                return result;
            }
            catch (PlaybackStorageException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId, "playback download error", ex);
            }
        }


    }
}
