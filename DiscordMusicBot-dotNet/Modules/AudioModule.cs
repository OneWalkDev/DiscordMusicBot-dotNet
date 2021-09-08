using Discord;
using Discord.Commands;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Command {

    public class AudioModule : ModuleBase<SocketCommandContext> {

        private readonly AudioService _service;

        public AudioModule(AudioService service) {
            _service = service;
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task help() {
            var user = Context.User as IGuildUser;
            var embed = new EmbedBuilder();
            embed.WithTitle("DiscordMusicBot.NET ヘルプ");
            embed.WithColor(Color.Blue);
            embed.WithTimestamp(DateTime.Now);
            embed.WithDescription("エイリアスは*です\nhelp : ヘルプを表示\njoin : botを入室\nleave : botが退出\nplay [YoutubeURL,検索したい文字] : 曲をキューに追加\nskip : 曲をスキップ\nstop : 曲をストップ\nsearch : 動画をyoutubeから探す\n\ngithub : https://github.com/yurisi0212/DiscordMusicBot-dotNet");
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("join", RunMode = RunMode.Async)]
        [Alias("j")]
        public async Task Join() {
            var user = Context.User as IGuildUser;
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("Leave", RunMode = RunMode.Async)]
        [Alias("l")]
        public async Task Leave() {
            var user = Context.User as IGuildUser;
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        public async Task Play(params string[] url) {
            var user = Context.User as IGuildUser;
            if (url.Length == 0) {
                await ReplyAsync("むり");
                return;
            }
            await _service.AddQueue(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, url[0]);
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("s")]
        public async Task Skip() {
            var user = Context.User as IGuildUser;
            _service.SkipAudio(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop() {
            var user = Context.User as IGuildUser;
            _service.StopAudio(Context.Guild, Context.Channel);
        }

        [Command("search", RunMode = RunMode.Async)]
        public async Task Search(params string[] url) {
            var user = Context.User as IGuildUser;
            if (url.Length == 0) {
                await ReplyAsync("むり");
                return;
            }
            //Todo
        }

        [Command("loop", RunMode = RunMode.Async)]
        public Task Loop(params string[] url) {
            var user = Context.User as IGuildUser;
            _service.ChangeLoop(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("qloop", RunMode = RunMode.Async)]
        public Task QLoop(params string[] url) {
            var user = Context.User as IGuildUser;
            _service.ChangeQueueLoop(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("shuffle", RunMode = RunMode.Async)]
        public Task Shuffle(params string[] url) {
            var user = Context.User as IGuildUser;
            _service.ChangeShuffle(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }
    }
}
