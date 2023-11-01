using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;
using DiscordMusicBot_dotNet.Configurations;
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

        public async Task JoinAudio(ulong? guild, IMessageChannel channel, IVoiceChannel target) {
            _ = Task.Run(async () => {
                await joinVoiceChat(guild, channel, target);
            });
            return;
        }

        private async Task joinVoiceChat(ulong? guild, IMessageChannel channel, IVoiceChannel target) {
            if (guild == null) return;
            if (!(target is IVoiceChannel)) {
                await channel.SendMessageAsync("ERROR >> BOTが参加できませんでした。あなた自身がこのBOTを入れたいVCに参加する必要があります。");
                return;
            }

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
                if (!_connectedChannels.TryGetValue((ulong)guild, out _)) await joinVoiceChat((ulong)guild, channel, target);
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
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
                }
            });
            return;
        }

        public void NextAddQueue(ulong? guild, IMessageChannel channel, IVoiceChannel target, string str) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var type = StreamHelper.GetType(str).Result;
                    if (type == YoutubeType.Playlist) {
                        await channel.SendMessageAsync($"ERROR >> プレイリストは追加できません。");
                        return;
                    }

                    var music = container.QueueManager.GetAudioFromString(str, type);
                    if (music.Title == null || music.Path == null || music.Url == null) {
                        await channel.SendMessageAsync("ERROR >> 結果が見つかりませんでした。");
                        return;
                    }

                    if (!container.QueueManager.IsQueueinMusic()) {
                        await channel.SendMessageAsync($"ERROR >> 現在音楽が再生されているときのみに追加できます");
                        return;
                    }

                    container.QueueManager.AddNextQueue(music);
                    var embed = new EmbedBuilder();
                    embed.WithTitle("割り込み追加");
                    embed.WithColor(Color.Green);
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithDescription(music.Title);
                    await channel.SendMessageAsync(embed: embed.Build());
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから実行してください。");
            });
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

        public void DeleteAudio(ulong? guild, IMessageChannel channel, int num) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var audio = container.QueueManager.Delete(num);
                    if (audio == null) {
                        await channel.SendMessageAsync($"ERROR >>idが存在しなかったようです。{Setting.Data.QueueCommandName}でもう一度確認してください。");
                        return;
                    }
                    var embed = new EmbedBuilder();
                    embed.WithTitle("削除");
                    embed.WithColor(Color.Red);
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithDescription(num.ToString() + ": " + audio.Title);
                    await channel.SendMessageAsync(embed: embed.Build());
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから実行してください。");
            });
        }

        public void SkipAudio(ulong? guild, IMessageChannel channel, IVoiceChannel target) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    if (container.QueueManager.AudioPlayer.PlaybackState != Assistor.PlaybackState.Stopped) {
                        container.CancellationTokenSource.Cancel();
                        return;
                    }
                    await channel.SendMessageAsync($"ERROR >> 現在読込中です。しばらくお待ちいただくか、/{Setting.Data.LeaveCommandName}で退出させてもう一度曲を入れ直してみてください。");
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> 音楽を再生する場合は/{Setting.Data.JoinCommandName}か/{Setting.Data.PlayCommandName}でbotを参加させてください。");
            });
            return;
        }

        public void ResetAudio(ulong? guild, IMessageChannel channel) {
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
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから実行してください。");
            });
        }

        public void ChangeLoop(ulong? guild, IMessageChannel channel, bool? loop = null) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    player.Loop = loop == null ? !player.Loop : (bool)loop;
                    await channel.SendMessageAsync("SETTING >> ループ " + (player.Loop ? "ON" : "OFF"));
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから設定してください。");
            });
        }

        public void ChangeQueueLoop(ulong? guild, IMessageChannel channel, bool? loop = null) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    player.QueueLoop = loop == null ? !player.QueueLoop : (bool)loop;
                    await channel.SendMessageAsync("SETTING >> キューループ " + (player.QueueLoop ? "ON" : "OFF"));
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから設定してください。");
            });
        }


        public void ChangeShuffle(ulong? guild, IMessageChannel channel, bool? shuffle = null) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    player.Shuffle = shuffle == null ? !player.Shuffle : (bool)shuffle;
                    await channel.SendMessageAsync("SETTING >> シャッフル " + (player.Shuffle ? "ON" : "OFF"));
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから設定してください。");
            });
        }

        public void GetQueueList(ulong? guild, IMessageChannel channel, int num) {
            _ = Task.Run(async () => {
                if (guild == null) return;
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
                    embed.WithColor(Color.Blue);
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithDescription(description);
                    await channel.SendMessageAsync(embed: embed.Build());
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから実行してください。");
            });
            return;
        }

        public void GetStatus(ulong? guild, IMessageChannel channel) {
            _ = Task.Run(async () => {
                if (guild == null) return;
                if (_connectedChannels.TryGetValue((ulong)guild, out AudioContainer container)) {
                    var player = container.QueueManager.AudioPlayer;
                    var loop = player.Loop ? "ON" : "OFF";
                    var qloop = player.QueueLoop ? "ON" : "OFF";
                    var shuffle = player.Shuffle ? "ON" : "OFF";

                    var embed = new EmbedBuilder();
                    embed.WithTitle("状態確認");
                    embed.WithColor(Color.Blue);
                    embed.WithTimestamp(DateTime.Now);
                    embed.WithDescription(
                        $"ループ : {loop}\n"+
                        $"キューループ: {qloop}\n"+
                        $"シャッフル: {shuffle}"
                    );
                    await channel.SendMessageAsync(embed: embed.Build());
                    return;
                }
                await channel.SendMessageAsync($"ERROR >> /{Setting.Data.JoinCommandName}でVCに接続してから実行してください。");
            });
        }
    }
}

