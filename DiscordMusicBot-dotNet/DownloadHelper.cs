using DiscordMusicBot_dotNet.Assistor;
using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace DiscordMusicBot_dotNet {
    class DownloadHelper {

        public static async Task<Audio.Audio> GetAudio(string url) {
            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(url);
            return new Audio.Audio { Path = GetPath(video.Id), Title = video.Title, Url = video.Url };
        }

        public static async Task<Audio.Audio> Search(string str) {
            var youtubeClient = new YoutubeClient();
            await foreach (var result in youtubeClient.Search.GetVideosAsync(str)) {
                return new Audio.Audio{Path = GetPath(result.Id), Title = result.Title, Url = result.Url };
            }
            return new Audio.Audio { Path = String.Empty, Title = String.Empty, Url = String.Empty };
        }

        public static string GetPath(string id) {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\DiscordMusicBot\cache\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path + id + ".mp3";
        }

        public static async Task Download(string url, string path) {
            var youtubeClient = new YoutubeClient();
            await youtubeClient.Videos.DownloadAsync(url, path);
        }

        public static async Task<YoutubeType> GetType(string url) {
            var youtube = new YoutubeClient();
            YoutubeType value = YoutubeType.Search;
            try {
                var playlist = await youtube.Playlists.GetAsync(url);
                youtube.Playlists.GetVideosAsync(playlist.Id);
                value = YoutubeType.Playlist;
            } catch (System.Exception e) {
                if (e is ArgumentException || e is YoutubeExplode.Exceptions.PlaylistUnavailableException) {
                    try {
                        var youtubeClient = new YoutubeClient();
                        var video = await youtubeClient.Videos.GetAsync(url);
                        value = YoutubeType.Video;
                    } catch (ArgumentException) {
                        value =  YoutubeType.Search;
                    }
                }
            }
            return value;
        }
    }
}
