using pmilet.Playback.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    public class PlaybackFileStorageService : PlaybackStorageServiceBase, IPlaybackStorageService
    {
        private string _storagePath;

        public PlaybackFileStorageService(string storagePath)
        {
            _storagePath = storagePath;
            CreateDirecotyIfNotExists(storagePath);
        }

        private static void CreateDirecotyIfNotExists(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }


        public override Task<PlaybackMessage> DownloadFromStorageAsync(string playbackId)
        {
            try
            {
                string path = $"{_storagePath}\\{playbackId}";
                string bodyString = File.ReadAllText(path);
                var playbackMessage = Task.FromResult(JsonConvert.DeserializeObject<PlaybackMessage>(bodyString));

                if (playbackMessage.Result.BodyString != null)
                    return playbackMessage;

                return Task.FromResult(new PlaybackMessage(path, string.Empty, bodyString, "text", 0));                
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error retrieving file {0} from folder {1}", playbackId, _storagePath);
                throw new StorageException(errorMessage, ex);
            }
        }

        public override Task UploadToStorageAsync(string playbackId, string path, string queryString, string bodyString, long elapsedTime = 0)
        {
            if (string.IsNullOrWhiteSpace(playbackId))
                throw new PlaybackStorageException(playbackId, "playbackId not found");
            PlaybackMessage playbackMessage = new PlaybackMessage(path, queryString, bodyString, "text", elapsedTime);
            try
            {
                var content = JsonConvert.SerializeObject(playbackMessage);
                File.WriteAllText($"{_storagePath}\\{playbackId}", content);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId, "playback upload error", ex);
            }
        }
    }
}
