using Discord;
using Discord.Audio;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;
using DiscordMusicBot_dotNet.Core;
using NAudio.Wave;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, AudioContainer> _connectedChannels = new();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target) {
            if (_connectedChannels.TryGetValue(guild.Id, out _)) {
                return;
            }
            if (target.Guild.Id != guild.Id) {
                return;
            }

            var player = new AudioPlayer();
            var Container = new AudioContainer {
                AudioClient = await target.ConnectAsync(),
                CancellationTokenSource = new CancellationTokenSource(),
                QueueManager = new QueueManager(player),
            };
            Container.AudioOutStream = Container.AudioClient.CreatePCMStream(AudioApplication.Music, bitrate: 128000);
            _connectedChannels.TryAdd(guild.Id, Container);
        }

        public async Task LeaveAudio(IGuild guild) {
            AudioContainer container;
            if (_connectedChannels.TryRemove(guild.Id, out container)) {
                await container.AudioClient.StopAsync();
            }
        }

        public async Task AddQueue(IGuild guild, IMessageChannel channel, IVoiceChannel target, string str) {
            AudioContainer container;
            if (!_connectedChannels.TryGetValue(guild.Id, out container)) {
                await JoinAudio(guild, target);
            }

            Audio.Audio music;

            _connectedChannels.TryGetValue(guild.Id, out container);

            try {
                music = container.QueueManager.GetAudioforString(str).Result;
            } catch (System.Exception) {
                await channel.SendMessageAsync("なかった");
                return;
            }

            container.QueueManager.AddQueue(music);

            await channel.SendMessageAsync("追加 >> "+music.Title);

            if (container.QueueManager.GetQueueCount() == 1) {
                await SendAudioAsync(guild, channel, target, music);
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target,Audio.Audio music) {
            AudioContainer container;
            _connectedChannels.TryGetValue(guild.Id, out container);

            var audioOutStream = container.AudioOutStream;
            var token = container.CancellationTokenSource.Token;

            var format = new WaveFormat(48000, 16, 2);
            using var reader = new MediaFoundationReader(music.Path);
            using var resamplerDmo = new ResamplerDmoStream(reader, format);

            try {
                container.ResamplerDmoStream = resamplerDmo;
                await resamplerDmo.CopyToAsync(audioOutStream, token)
                   .ContinueWith(t => { return; });
            } finally {
                await audioOutStream.FlushAsync();
                container.CancellationTokenSource = new CancellationTokenSource();
            }
        }

        public async void SkipAudio(IGuild guild, IMessageChannel channel, IVoiceChannel target) {
            if (Next(guild,channel,target).Result) await channel.SendMessageAsync("スキップしたよ");
        }

        public async void StopAudio(IGuild guild, IMessageChannel channel) {
            //Todo
        }

        public async void ChangeLoop(IGuild guild, IMessageChannel channel) {
            AudioContainer container;
            _connectedChannels.TryGetValue(guild.Id, out container);
            var player = container.QueueManager.GetAudioPlayer();
            player.Loop = player.Loop ? false : true;
            if (player.Loop) {
                await channel.SendMessageAsync("ループ >> ON");
            } else {
                await channel.SendMessageAsync("ループ >> OFF");
            }
        }

        public async void ChangeShuffle(IGuild guild, IMessageChannel channel) {
            AudioContainer container;
            _connectedChannels.TryGetValue(guild.Id, out container);
            var player = container.QueueManager.GetAudioPlayer();
            player.Shuffle = player.Shuffle ? false : true;
            if (player.Shuffle) {
                await channel.SendMessageAsync("シャッフル >> ON");
            } else {
                await channel.SendMessageAsync("シャッフル >> OFF");
            }
        }

        public async Task<bool> Next(IGuild guild, IMessageChannel channel, IVoiceChannel target) {
            AudioContainer container;
            if (_connectedChannels.TryGetValue(guild.Id, out container)) {
                container.CancellationTokenSource.Cancel();
                var next = container.QueueManager.Next();
                if (next == null)
                    return false;
                SendAudioAsync(guild, channel, target, next);
                return true;
            } else {
                await channel.SendMessageAsync("いまいない");
                return false;
            }
            
        }

    }
}

