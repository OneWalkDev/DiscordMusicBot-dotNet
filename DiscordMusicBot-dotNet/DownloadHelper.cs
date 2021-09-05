using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Search;

namespace DiscordMusicBot_dotNet {
    class DownloadHelper {

        public static async Task<string> GetId(string url) {
            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(url);
            return video.Id;
        }

        public static async Task<string[]> Search(string str) {
            var value = new string[3] { String.Empty,
                                        String.Empty,
                                        String.Empty};
            var youtubeClient = new YoutubeClient();
            await foreach (var result in youtubeClient.Search.GetVideosAsync(str)) {
                value[0] = result.Id;
                value[1] = result.Title;
                value[2] = result.Url;
                return value;
            }
            return value;
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
