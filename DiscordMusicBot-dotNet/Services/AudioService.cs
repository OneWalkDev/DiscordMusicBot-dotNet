using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;
using DiscordMusicBot_dotNet.Core;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class AudioService {

        private readonly ConcurrentDictionary<ulong, AudioContainer> _connectedChannels = new();

        private readonly DiscordSocketClient _discord;

        public AudioService(DiscordSocketClient discord) {
            _discord = discord;
        }

        public async Task JoinAudio(ulong? guild, IVoiceChannel target) {
            _ = Task.Run(async () => {
                await joinVoiceChat(guild, target);
            });
            return;
        }

        private async Task joinVoiceChat(ulong? guild, IVoiceChannel target) {
            if (guild == null) return;
            if (_connectedChannels.TryGetValue((ulong)guild, out _)) return;
            if (target.Guild.Id != guild) return;

            var player = new AudioPlayer();

            var Container = new AudioContainer {
                AudioClient = await target.ConnectAsync(),
                CancellationTokenSource = new CancellationTokenSource(),
                QueueManager = new QueueManager(player),
            };


            Container.AudioOutStream = Container.AudioClient.CreatePCMStream(AudioApplication.Music);

            Container.QueueManager.AudioPlayer.PlaybackState = Assistor.PlaybackState.Stopped;
            Container.QueueManager.AudioPlayer.NextPlay = true;

            _connectedChannels.TryAdd((ulong)guild, Container);
        }

        public async Task LeaveAudio(ulong? guild) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryRemove((ulong)guild, out AudioContainer container)) {
                    container.QueueManager.AudioPlayer.NextPlay = false;
                    container.CancellationTokenSource.Cancel();
                    await container.AudioClient.StopAsync();
                }
            });
            return;
        }

        public async Task AddQueue(ulong? guild, IMessageChannel channel, IVoiceChannel target, string str) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (!_connectedChannels.TryGetValue((ulong)guild, out _)) await joinVoiceChat((ulong)guild, target);
                _connectedChannels.TryGetValue((ulong)guild, out AudioContainer container);
                var description = "";
                var play = false;
                if (container.QueueManager.GetQueueCount() == 0)
                    play = true;

                var type = StreamHelper.GetType(str).Result;
                var embed = new EmbedBuilder();
                embed.WithTitle("追加");
                embed.WithColor(Color.Green);
                embed.WithTimestamp(DateTime.Now);
                Audio.Audio music;
                if (type == YoutubeType.Playlist) {
                    var playlist = StreamHelper.GetPlaylists(str).Result;
                    await foreach (var video in StreamHelper.GetPlaylistClient().GetVideosAsync(playlist.Id).Select((value, index) => new { value, index })) {
                        music = new Audio.Audio { Path = StreamHelper.getHighestBitrateUrl(video.value.Id).Result, Title = video.value.Title, Url = video.value.Url };
                        container.QueueManager.AddQueue(music);
                        if (play) {
                            SendAudioAsync((ulong)guild, channel, music);
                            play = false;
                        }
                        var num = video.index + 1;
                        description += num + " : " + music.Title + "\n";
                    }
                    embed.WithDescription(description);
                } else {
                    music = container.QueueManager.GetAudioFromString(str, type);
                    if (music.Title == null || music.Path == null || music.Url == null) {
                        await channel.SendMessageAsync("ERROR >> 結果が見つかりませんでした。");
                        return;
                    }

                    container.QueueManager.AddQueue(music);
                    if (play) {
                        SendAudioAsync((ulong)guild, channel, music);
                        play = false;
                    }
                    embed.WithDescription("1 : " + music.Title);

                }
                await channel.SendMessageAsync(embed: embed.Build());
            });
            return;
        }

        public async Task SendAudioAsync(ulong guild, IMessageChannel channel, Audio.Audio music) {
            if (_connectedChannels.TryGetValue(guild, out AudioContainer container)) {
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

        public async void DeleteAudio(IGuild guild, int num) {
            if (_connectedChannels.TryGetValue(guild.Id, out AudioContainer container)) {
                container.QueueManager.Delete(num);
            }
        }

        public async void SkipAudio(ulong? guild, IMessageChannel channel, IVoiceChannel target) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    if (container.QueueManager.AudioPlayer.PlaybackState != Assistor.PlaybackState.Stopped) {
                        container.CancellationTokenSource.Cancel();
                        return;
                    }
                    await channel.SendMessageAsync($"ERROR >> 現在読込中です。しばらくお待ちいただくか、/{Settings.LeaveCommandName}で退出させてもう一度曲を入れ直してみてください。");
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> 音楽を再生する場合は/{Settings.JoinCommandName}か/{Settings.PlayCommandName}でbotを参加させてください。");
            });
            return;
        }

        public async void ResetAudio(ulong? guild, IMessageChannel channel) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    container.QueueManager.AudioPlayer.NextPlay = false;
                    container.CancellationTokenSource.Cancel();
                    container.QueueManager.AudioPlayer.NextPlay = true;
                    container.QueueManager.Reset();
                    await channel.SendMessageAsync("NOTICE >> キューをリセットしました");
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Settings.JoinCommandName}でVCに接続してから実行してください。");
            });
        }

        public async void ChangeLoop(ulong? guild, IMessageChannel channel, bool? loop = null) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    player.Loop = loop == null ? !player.Loop : (bool)loop;
                    await channel.SendMessageAsync("SETTING >> ループ " + (player.Loop ? "ON" : "OFF"));
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Settings.JoinCommandName}でVCに接続してから設定してください。");
            });
        }

        public async void ChangeQueueLoop(ulong? guild, IMessageChannel channel, bool? loop = null) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    player.QueueLoop = loop == null ? !player.QueueLoop : (bool)loop;
                    await channel.SendMessageAsync("SETTING >> キューループ " + (player.QueueLoop ? "ON" : "OFF"));
                    if (!player.QueueLoop) {
                        container.QueueManager.LoopDisable();
                    }
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Settings.JoinCommandName}でVCに接続してから設定してください。");
            });
        }


        public async void ChangeShuffle(ulong? guild, IMessageChannel channel, bool? shuffle = null) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    player.Shuffle = shuffle == null ? !player.Shuffle : (bool)shuffle;
                    await channel.SendMessageAsync("SETTING >> シャッフル " + (player.Shuffle ? "ON" : "OFF"));
                    if (!player.Shuffle) {
                        container.QueueManager.LoopDisable();
                    }
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Settings.JoinCommandName}でVCに接続してから設定してください。");
            });
        }

        public async Task GetQueueList(ulong? guild, IMessageChannel channel, int num) {
            _ = Task.Run(async () => {
                Console.WriteLine(num.ToString());
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var titles = container.QueueManager.GetQueueMusicTitles();
                    var playing = container.QueueManager.GetNowPlayingMusicTitle();
                    if (titles == null || playing == null) {
                        await channel.SendMessageAsync("NOTICE>> 曲が一つも追加されていません。");
                        return;
                    }
                    var maxpage = Math.Ceiling(titles.Length / 10.0);
                    var description = "ページ " + num + "/" + maxpage + "\n\n";
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
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Settings.JoinCommandName}でVCに接続してから実行してください。");
            });
            return;
        }
    }
}

