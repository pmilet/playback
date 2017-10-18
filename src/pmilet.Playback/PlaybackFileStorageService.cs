using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using pmilet.Playback.Core;
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
        private readonly string _folderPath;
        public PlaybackFileStorageService(string rootFolder, string folderName)
        {
            _folderPath = $"{rootFolder}\\{folderName}";
        }

        public PlaybackFileStorageService(IConfigurationRoot configuration)
        {
            var section = configuration.GetSection("PlaybackStorage");
            var rootFolder = section.GetSection("ConnectionString").Value;
            var folderName = section.GetSection("Name").Value;
            _folderPath = $"{rootFolder}\\{folderName}";
        }


        public override async Task<PlaybackMessage> DownloadFromStorageAsync(string fileId)
        {
            string content = await DownloadRecordedFileAsync(fileId, _folderPath);
            return JsonConvert.DeserializeObject<PlaybackMessage>(content);
        }

        public override async Task UploadToStorageAsync(string playbackId, string path, string queryString, string bodyString, long elapsedTime = 0)
        {
            if (string.IsNullOrWhiteSpace(playbackId))
                throw new PlaybackStorageException(playbackId, "playbackId not found");
            PlaybackMessage playbackMessage = new PlaybackMessage(path, queryString, bodyString, "text", elapsedTime);
            try
            {
                await UploadRecordedFileAsync( JsonConvert.SerializeObject(playbackMessage), $"_folderPath\\{playbackId}" );                
            }
            catch (Exception ex)
            {
                throw new PlaybackStorageException(playbackId, "playback upload error", ex);
            }
        }

        internal async Task<string> DownloadRecordedFileAsync(string fileName, string folderPath)
        {
            try
            {
                string fullPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory,$".\\{folderPath}\\{fileName}" );

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException(string.Format("the file {0} was not found", fullPath));
                }
                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error downloading file {0} from file storage - {1}", fileName, ex.Message);
                return errorMessage;
            }
        }

        internal async Task UploadRecordedFileAsync(string content, string folderPath)
        {
            File.WriteAllText(folderPath, content);
        }


    }

}
