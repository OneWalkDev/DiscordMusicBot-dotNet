using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace DiscordMusicBot_dotNet {
    class DownloadHelper {

        public static async Task<string> getId(string url) {
            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(url);
            return video.Id;
        }

        public static async Task<string> GetPath(string id) {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\DiscordMusicBot\cache\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path + id + ".mp3";
        }

        public static async Task Download(string url, string path) {
            var youtubeClient = new YoutubeClient();
            await youtubeClient.Videos.DownloadAsync(url, path);
        }
    }
}
