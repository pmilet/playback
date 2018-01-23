using Newtonsoft.Json;
using pmilet.HttpPlayback;
using pmilet.HttpPlayback.Core;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace ApiGateway_Sample
{
    public class PlaybackFileStorageService : PlaybackStorageServiceBase, IPlaybackStorageService
    {
        private readonly string _folderPath;
        public PlaybackFileStorageService()
        {
            _folderPath = $"{Environment.CurrentDirectory}\\playback";
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
                await UploadRecordedFileAsync( JsonConvert.SerializeObject(playbackMessage), _folderPath, playbackId );                
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
                string fullPath = $"{folderPath}\\{fileName}";

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

        internal async Task UploadRecordedFileAsync(string content, string folderPath, string fileId)
        {
            folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{folderPath}");
            string fullPath = $"{folderPath}\\{fileId}";
            DirectorySecurity securityRules = new DirectorySecurity();
            securityRules.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath, securityRules);
            File.WriteAllText($"{folderPath}\\{fileId}", content);
        }


    }

}
