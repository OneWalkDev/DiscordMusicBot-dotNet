using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        private static CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private IAudioClient _client;

        public async Task JoinAudio(IGuild guild, IVoiceChannel target) {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
                return;
            }
            if (target.Guild.Id != guild.Id) {
                return;
            }

            var audioClient = await target.ConnectAsync();
            _client = client;

            if (ConnectedChannels.TryAdd(guild.Id, audioClient)) {
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task LeaveAudio(IGuild guild) {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client)) {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target) {
            AudioOutStream dstream = null;
            try {
                using (Stream ms = new MemoryStream()) {
                    var youtubeClient = new YoutubeClient();
                    var video = await youtubeClient.Videos.GetAsync(@"https://www.youtube.com/watch?v=aRyjZa89g4o");
                    var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                    var url = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;
                    using (Stream stream = WebRequest.Create(url).GetResponse().GetResponseStream()) {
                        byte[] buffer = new byte[32768];
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
                            ms.Write(buffer, 0, read);
                        }
                    }

                    ms.Position = 0;
                    using (WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms)))) {

                        var naudio = WaveFormatConversionStream.CreatePcmStream(blockAlignedStream);

                        dstream = _client.CreatePCMStream(AudioApplication.Music);
                        byte[] buffer = new byte[naudio.Length];

                        int rest = (int)(naudio.Length - naudio.Position);
                        await naudio.ReadAsync(buffer, 0, rest);
                        await dstream.WriteAsync(buffer, 0, rest, cancellationToken.Token);

                    }
                }
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
                if (e.InnerException != null)
                    Debug.WriteLine(e.InnerException.Message);
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
