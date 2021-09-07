using Discord;
using Discord.Audio;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;
using DiscordMusicBot_dotNet.Core;
using DiscordMusicBot_dotNet.Exception;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new();

        private QueueManager _queue;

        private  readonly AudioPlayer _player = new();

        private Process _ffmpeg;

        public AudioService() {
            _queue = new QueueManager(_player);
        }

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

        public async Task AddQueue(IGuild guild, IMessageChannel channel, IVoiceChannel target, string str) {
            Audio.Audio music;
            try {
                music = _queue.GetAudioforString(str).Result;
            } catch (SearchNotFoundException) {
                await channel.SendMessageAsync("なかった");
                return;
            }

            var path = music.Path;

            if (!File.Exists(path) && !_queue.IsQueueinMusic()) {
                await channel.SendMessageAsync("ダウンロードしてるからまって");
                await DownloadHelper.Download(str, path);
            }

            _queue.AddQueue(music);

            if(_queue.GetQueueCount() == 1) {
                await SendAudioAsync(guild, channel, target, music);
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target,Audio.Audio music) {
            IAudioClient client;
            if (!ConnectedChannels.TryGetValue(guild.Id, out _)) {
                await JoinAudio(guild, target);
            }

            Skip();

            var path = music.Path;

            if (ConnectedChannels.TryGetValue(guild.Id, out client)) {
                _ffmpeg = CreateProcess(path);
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

        public async void ChangeLoop(IMessageChannel channel) {
            _player.Loop = _player.Loop ? false : true;
            if (_player.Loop) {
                await channel.SendMessageAsync("ループ >> ON");
            } else {
                await channel.SendMessageAsync("ループ >> OFF");
            }
        }

        public async void ChangeShuffle(IMessageChannel channel) {
            _player.Shuffle = _player.Shuffle ? false : true;
            if (_player.Shuffle) {
                await channel.SendMessageAsync("シャッフル >> ON");
            } else {
                await channel.SendMessageAsync("シャッフル >> OFF");
            }
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

        public AudioPlayer GetAudioPlayer() {
            return _player;
        }

        public QueueManager GetQueueManager() {
            return _queue;
        }

    }
}

