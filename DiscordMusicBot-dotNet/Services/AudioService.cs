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

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string url) {
            var music = DownloadHelper.GetPath(DownloadHelper.getId(url).Result).Result;

            if (!File.Exists(music)) {
                await DownloadHelper.Download(url, music);
            }

            IAudioClient client;

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
