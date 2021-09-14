using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;
using DiscordMusicBot_dotNet.Core;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;

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
            Container.QueueManager.AudioPlayer.PlaybackState = Assistor.PlaybackState.Stopped;
            Container.QueueManager.AudioPlayer.NextPlay = true;
            _connectedChannels.TryAdd(guild.Id, Container);
        }

        public async Task LeaveAudio(IGuild guild) {
            if (_connectedChannels.TryRemove(guild.Id, out AudioContainer container)) {
                container.QueueManager.AudioPlayer.NextPlay = false;
                container.CancellationTokenSource.Cancel();
                await container.AudioClient.StopAsync();
            }
        }

        public async Task AddQueue(IGuild guild, IMessageChannel channel, IVoiceChannel target, string str) {
            if (!_connectedChannels.TryGetValue(guild.Id, out _)) {
                await JoinAudio(guild, target);
            }

            _connectedChannels.TryGetValue(guild.Id, out AudioContainer container);

            var description = "";
            var play = false;
            if (container.QueueManager.GetQueueCount() == 0)
                play = true;

            var type = StreamHelper.GetType(str).Result;
            var embed = new EmbedBuilder();
            embed.WithTitle("追加");
            embed.WithColor(Color.Red);
            embed.WithTimestamp(DateTime.Now);
            Audio.Audio music;
            if (type == YoutubeType.Playlist) {
                var youtubeClient = new YoutubeClient();
                var playlists = youtubeClient.Playlists;
                var playlist = await playlists.GetAsync(str);
                await foreach (var video in playlists.GetVideosAsync(playlist.Id).Select((value, index) => new { value, index })) {
                    music = new Audio.Audio { Path = StreamHelper.getHighestBitrateUrl(video.value.Id).Result, Title = video.value.Title, Url = video.value.Url };
                    container.QueueManager.AddQueue(music);
                    if (play) {
                        SendAudioAsync(guild, channel, music);
                        play = false;
                    }
                    var num = video.index + 1;
                    description += num + " : " + music.Title + "\n";
                }
                embed.WithDescription(description);
            } else {
                try {
                    music = container.QueueManager.GetAudioFromString(str, type);
                } catch (System.Exception) {
                    await channel.SendMessageAsync("なかった");
                    return;
                }

                container.QueueManager.AddQueue(music);
                if (play) {
                    SendAudioAsync(guild, channel, music);
                    play = false;
                }
                embed.WithDescription("1 : " + music.Title);

            }
            await channel.SendMessageAsync(embed: embed.Build());
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, Audio.Audio music) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var audioOutStream = container.AudioOutStream;
                var token = container.CancellationTokenSource.Token;
                var format = new WaveFormat(48000, 16, 2);
                using var reader = new MediaFoundationReader(music.Path);
                using var resamplerDmo = new ResamplerDmoStream(reader, format);
                try {
                    container.ResamplerDmoStream = resamplerDmo;
                    container.QueueManager.AudioPlayer.PlaybackState = Assistor.PlaybackState.Playing;
                    await resamplerDmo.CopyToAsync(audioOutStream, token);
                } finally {
                    container.QueueManager.AudioPlayer.PlaybackState = Assistor.PlaybackState.Stopped;
                    await audioOutStream.FlushAsync();
                    await _discord.SetGameAsync(null);
                    container.CancellationTokenSource = new CancellationTokenSource();
                    if (container.QueueManager.AudioPlayer.NextPlay) {
                        var next = container.QueueManager.Next();
                        if (next != null) {
                            SendAudioAsync(guild, channel, next);
                        }
                    }
                }
            }
        }

        public async void SkipAudio(IGuild guild, IMessageChannel channel, IVoiceChannel target) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                if (container.QueueManager.AudioPlayer.PlaybackState != Assistor.PlaybackState.Stopped) {
                    container.CancellationTokenSource.Cancel();
                    await channel.SendMessageAsync("スキップしたよ");
                    return;
                }
                await channel.SendMessageAsync("はやい");
                return;
            }
            await channel.SendMessageAsync("いまいない");
        }

        public async void ResetAudio(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                container.QueueManager.AudioPlayer.NextPlay = false;
                container.CancellationTokenSource.Cancel();

                container.QueueManager.AudioPlayer.NextPlay = true;
                container.QueueManager.Reset();
                await channel.SendMessageAsync("キューをリセットしたよ");
            }
        }

        public async void ChangeLoop(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var player = container.QueueManager.AudioPlayer;
                player.Loop = !player.Loop;
                if (player.Loop) {
                    await channel.SendMessageAsync("ループ >> ON");
                } else {
                    await channel.SendMessageAsync("ループ >> OFF");
                }
            }
        }

        public async void ChangeQueueLoop(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var player = container.QueueManager.AudioPlayer;
                player.QueueLoop = !player.QueueLoop;
                if (player.Loop) {
                    await channel.SendMessageAsync("キューループ >> ON");
                } else {
                    await channel.SendMessageAsync("キューループ >> OFF");
                    container.QueueManager.LoopDisable();
                }
            }
        }

        public async void ChangeShuffle(IGuild guild, IMessageChannel channel) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var player = container.QueueManager.AudioPlayer;
                player.Shuffle = !player.Shuffle;
                if (player.Shuffle) {
                    await channel.SendMessageAsync("シャッフル >> ON");
                } else {
                    await channel.SendMessageAsync("シャッフル >> OFF");
                    container.QueueManager.LoopDisable();
                }
            }
        }

        public async Task GetQueueList(IGuild guild, IMessageChannel channel,int num) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                var titles = container.QueueManager.GetQueueMusicTitles();
                var playing = container.QueueManager.GetNowPlayingMusicTitle();
                if (titles == null || playing == null) {
                    await channel.SendMessageAsync("何も再生してないよ");
                    return;
                }
                var maxpage = Math.Ceiling(titles.Length / 10.0);
                var description = "ページ "+ num +"/" + maxpage+"\n\n";
                description += "*現在再生中 : " + playing + "\n\n";
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

