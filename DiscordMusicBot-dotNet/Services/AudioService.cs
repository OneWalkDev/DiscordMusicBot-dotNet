using Discord;
using Discord.Audio;
using DiscordMusicBot_dotNet.Assistor;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new();

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
            if (!ConnectedChannels.TryGetValue(guild.Id, out client)) {
                await JoinAudio(guild, target);
            }

            if (_ffmpeg != null) {
                _ffmpeg.Kill();
                _ffmpeg = null;
            }

            var type = DownloadHelper.GetType(str).Result;
            string music = "";

            if (type == YoutubeType.Video) {
                music = DownloadHelper.GetPath(DownloadHelper.GetId(str).Result);
            } else if (type == YoutubeType.Search) {
                var value = DownloadHelper.Search(str).Result;

                if (value[0] == string.Empty
                    || value[1] == string.Empty
                    || value[2] == string.Empty) {
                    await channel.SendMessageAsync("なかった");
                    return;
                }

                music = DownloadHelper.GetPath(value[0]);
                await channel.SendMessageAsync(value[1] + " を再生します");
                str = value[2];
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
                        _ffmpeg = null;
                    } finally {
                        await stream.FlushAsync();
                    }
                }
            }
        }

        public async void SkipAudio() {
            if (_ffmpeg != null) {
                _ffmpeg.Kill();
                _ffmpeg = null;
            }
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

