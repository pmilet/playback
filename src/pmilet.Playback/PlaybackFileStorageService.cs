using pmilet.Playback.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace pmilet.Playback
{
    /// <summary>
    /// Provides file system-based storage for playback messages.
    /// </summary>
    public class PlaybackFileStorageService : PlaybackStorageServiceBase, IPlaybackStorageService
    {
        private string _storagePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackFileStorageService"/> class.
        /// </summary>
        /// <param name="storagePath">The directory path where playback files will be stored.</param>
        public PlaybackFileStorageService(string storagePath)
        {
            _storagePath = storagePath;
            CreateDirectoryIfNotExists(storagePath);
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }


        public override Task<PlaybackMessage> DownloadFromStorageAsync(string playbackId)
        {
            string path = Path.Combine(_storagePath, playbackId);
            try
            {
                string bodyString = File.ReadAllText(path);
                var playbackMessage = JsonConvert.DeserializeObject<PlaybackMessage>(bodyString);

                if (playbackMessage?.BodyString != null)
                    return Task.FromResult(playbackMessage);

                return Task.FromResult(new PlaybackMessage(path, string.Empty, bodyString, "text", 0));
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId, "playback download error", ex);
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
                File.WriteAllText(Path.Combine(_storagePath, playbackId), content);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId, "playback upload error", ex);
            }
        }
    }
}
