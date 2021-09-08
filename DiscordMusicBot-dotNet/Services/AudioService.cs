using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;
using DiscordMusicBot_dotNet.Core;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, AudioContainer> _connectedChannels = new();

        private readonly DiscordSocketClient _discord;

        public AudioService(DiscordSocketClient discord) {
            _discord = discord;
        }

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
            if (_connectedChannels.TryRemove(guild.Id, out AudioContainer container)) {
                await container.AudioClient.StopAsync();
            }
        }

        public async Task AddQueue(IGuild guild, IMessageChannel channel, IVoiceChannel target, string str) {
            if (!_connectedChannels.TryGetValue(guild.Id, out _)) {
                await JoinAudio(guild, target);
            }

            Audio.Audio[] audios;

            _connectedChannels.TryGetValue(guild.Id, out AudioContainer container);

            try {
                audios = container.QueueManager.GetAudioforString(str);
            } catch (System.Exception) {
                await channel.SendMessageAsync("なかった");
                return;
            }

            var play = false;

            if (container.QueueManager.GetQueueCount() == 0)
                play = true;

            foreach (var music in audios) {
                container.QueueManager.AddQueue(music);
                await channel.SendMessageAsync("追加 >> " + music.Title);
                if (play) {
                    SendAudioAsync(guild, channel, music);
                    play = false;
                }
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, Audio.Audio music) {
            AudioContainer container;
            if (_connectedChannels.TryGetValue(guild.Id, out container)) {
                var audioOutStream = container.AudioOutStream;
                var token = container.CancellationTokenSource.Token;

                var format = new WaveFormat(48000, 16, 2);
                using var reader = new MediaFoundationReader(music.Path);
                await _discord.SetGameAsync("再生中 : " + music.Title);
                using var resamplerDmo = new ResamplerDmoStream(reader, format);

                try {
                    container.ResamplerDmoStream = resamplerDmo;
                    await resamplerDmo.CopyToAsync(audioOutStream, token).ContinueWith(t => { return; });
                } finally {
                    await audioOutStream.FlushAsync();
                    await _discord.SetGameAsync(null);
                    container.CancellationTokenSource = new CancellationTokenSource();
                    var next = container.QueueManager.Next();
                    if (next != null)
                        await SendAudioAsync(guild, channel, next);
                }
            }
        }

        public async void SkipAudio(IGuild guild, IMessageChannel channel, IVoiceChannel target) {
            AudioContainer container;
            if (_connectedChannels.TryGetValue(guild.Id, out container)) {
                container.CancellationTokenSource.Cancel();
                await channel.SendMessageAsync("スキップしたよ");
            } else {
                await channel.SendMessageAsync("いまいない");
            }
        }

        public async void ResetAudio(IGuild guild, IMessageChannel channel) {
            //Todo
        }

        public async void ChangeLoop(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var player = container.QueueManager.GetAudioPlayer();
                player.Loop = player.Loop ? false : true;
                if (player.Loop) {
                    await channel.SendMessageAsync("ループ >> ON");
                } else {
                    await channel.SendMessageAsync("ループ >> OFF");
                }
            }
        }

        public async void ChangeQueueLoop(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var player = container.QueueManager.GetAudioPlayer();
                player.QueueLoop = player.QueueLoop ? false : true;
                if (player.Loop) {
                    await channel.SendMessageAsync("キューループ >> ON");
                } else {
                    await channel.SendMessageAsync("キューループ >> OFF");
                }
            }
        }

        public async void ChangeShuffle(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var player = container.QueueManager.GetAudioPlayer();
                player.Shuffle = player.Shuffle ? false : true;
                if (player.Shuffle) {
                    await channel.SendMessageAsync("シャッフル >> ON");
                } else {
                    await channel.SendMessageAsync("シャッフル >> OFF");
                }
            }
        }

        public async Task GetQueueList(IGuild guild, IMessageChannel channel,int num) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var titles = container.QueueManager.GetQueueMusicTitles();
                if (titles == null) {
                    await channel.SendMessageAsync("何も再生してないよ");
                    return;
                }
                var maxpage = Math.Ceiling(titles.Length / 10.0);
                var description = "ページ "+ num +"/" + maxpage+"\n\n";
                for (int i = 0 + 10 * (num - 1); i < 10 + 10 * (num - 1); i++) {
                    if (titles.Length == i) break;
                    var number = i + 1;
                    description += number + " : " + titles[i] + "\n";
                }
                var embed = new EmbedBuilder();
                embed.WithTitle("キュー");
                embed.WithColor(Color.Red);
                embed.WithTimestamp(DateTime.Now);
                embed.WithDescription(description);
                await channel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}

