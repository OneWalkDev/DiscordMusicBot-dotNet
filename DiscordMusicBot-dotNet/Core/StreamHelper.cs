using DiscordMusicBot_dotNet.Assistor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace DiscordMusicBot_dotNet.Core {
    class StreamHelper {

        public static async Task<Audio.Audio> GetAudio(string url) {
            var youtubeClient = new YoutubeClient();
            var video = await youtubeClient.Videos.GetAsync(url);
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
            var streamUrl = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;
            return new Audio.Audio { Path = streamUrl, Title = video.Title, Url = video.Url };
        }

        public static async Task<Audio.Audio> Search(string str) {
            var youtubeClient = new YoutubeClient();
            await foreach (var result in youtubeClient.Search.GetVideosAsync(str)) {
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(result.Id);
                var url = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;
                return new Audio.Audio { Path = url, Title = result.Title, Url = result.Url };
            }
            return new Audio.Audio { Path = String.Empty, Title = String.Empty, Url = String.Empty };
        }

        public static async Task<string> getHighestBitrateUrl(YoutubeExplode.Videos.VideoId id) {
            var youtubeClient = new YoutubeClient();
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(id);
            return streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;
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
