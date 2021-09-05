using Discord;
using Discord.Audio;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new();

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

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target, string url) {
            IAudioClient client;
            if (!ConnectedChannels.TryGetValue(guild.Id, out client)) {
                await JoinAudio(guild, target);
            }

            var music = DownloadHelper.GetPath(DownloadHelper.GetId(url).Result).Result;

            if (!File.Exists(music)) {
                await channel.SendMessageAsync("ダウンロードしてるからまって");
                await DownloadHelper.Download(url, music);
            }

            if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
                using (var ffmpeg = CreateProcess(music)) {
                    using (var stream = client.CreatePCMStream(AudioApplication.Music)) {
                        try {
                            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                        } finally {
                            await stream.FlushAsync();
                        }
                    }
                }

            }
        }
        public async Task SearchAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target, string str) {
            IAudioClient client;
            if (!ConnectedChannels.TryGetValue(guild.Id, out client)) {
                await JoinAudio(guild, target);
            }
            var value = DownloadHelper.Search(str).Result;

            if (   value[0] == string.Empty 
                || value[1] == string.Empty 
                || value[2] == string.Empty) {
                await channel.SendMessageAsync("なかった");
                return;
            }

            var music = DownloadHelper.GetPath(value[0]).Result;

            await channel.SendMessageAsync(value[1]+" を再生します");

            if (!File.Exists(music)) {
                await channel.SendMessageAsync("ダウンロードしてるからまって");
                await DownloadHelper.Download(value[2], music);
            }

            if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
                using (var ffmpeg = CreateProcess(music)) {
                    using (var stream = client.CreatePCMStream(AudioApplication.Music)) {
                        try {
                            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                        } finally {
                            await stream.FlushAsync();
                        }
                    }
                }

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

