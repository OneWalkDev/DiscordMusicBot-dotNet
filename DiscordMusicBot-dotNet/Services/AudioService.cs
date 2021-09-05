using Discord;
using Discord.Audio;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new();

        public AudioPlayer AudioPlayer { get; }

        private Process _ffmpeg;

        public async Task JoinAudio(IGuild guild, IVoiceChannel target) {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
                return;
            }
            if (target.Guild.Id != guild.Id) {
                return;
            }

            var audioClient = await target.ConnectAsync();

            ConnectedChannels.TryAdd(guild.Id, audioClient);

        }

        public async Task LeaveAudio(IGuild guild) {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client)) {
                await client.StopAsync();
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target, string str) {
            IAudioClient client;
            string music = "";
            Audio.Audio audio;

            if (!ConnectedChannels.TryGetValue(guild.Id, out _)) {
                await JoinAudio(guild, target);
            }

            Skip();

            var type = DownloadHelper.GetType(str).Result;
            switch (type) {
                case YoutubeType.Video:
                    audio = DownloadHelper.GetAudio(str).Result;
                    music = audio.Path;
                    break;
                case YoutubeType.Search:
                    audio = DownloadHelper.Search(str).Result;
                    if (audio.Path == string.Empty
                        || audio.Title == string.Empty
                        || audio.Url == string.Empty) {
                        await channel.SendMessageAsync("なかった");
                        return;
                    }
                    music = DownloadHelper.GetPath(audio.Path);
                    await channel.SendMessageAsync(audio.Title + " を再生します");
                    str = audio.Url;
                    break;
                case YoutubeType.Playlist:
                    //Todo
                    break;
            }

            if (!File.Exists(music)) {
                await channel.SendMessageAsync("ダウンロードしてるからまって");
                await DownloadHelper.Download(str, music);
            }

            if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
                _ffmpeg = CreateProcess(music);
                using (var stream = client.CreatePCMStream(AudioApplication.Music)) {
                    try {
                        await _ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                        _ffmpeg.Dispose();
                        _ffmpeg = null;
                    } finally {
                        await stream.FlushAsync();
                    }
                }
            }
        }

        public async void SkipAudio(IMessageChannel channel) {
            if (Skip()) await channel.SendMessageAsync("スキップしたよ");
        }

        public async void StopAudio(IMessageChannel channel) {
            if (Skip()) await channel.SendMessageAsync("停止したよ");
        }

        public bool Skip() {
            if (_ffmpeg != null) {
                _ffmpeg.Dispose();
                _ffmpeg = null;
                return true;
            }
            return false;
        }

        private Process CreateProcess(string path) {
            return Process.Start(new ProcessStartInfo {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

    }
}

