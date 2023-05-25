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
        public async Task Help() {
            var embed = new EmbedBuilder();
            embed.WithTitle("DiscordMusicBot.NET ヘルプ");
            embed.WithColor(Color.Blue);
            embed.WithTimestamp(DateTime.Now);
            embed.WithDescription("エイリアスは*です\n" +
                "help : ヘルプを表示\n" +
                "join(j) : botを入室\n" +
                "leave(l) : botが退出\n" +
                "play(p) [YoutubeURL,検索したいワード] : 曲をキューに追加\n" +
                "skip(s) : 曲をスキップ\n" +
                "search : 動画をyoutubeから探す(現在非対応)\n" +
                "loop : 1曲ループする\n" +
                "qloop : キュー内をループする\n" +
                "shuffle : ループしシャッフル再生する\n" +
                "quere(q) : キューの中に入ってる曲を表示\n"+
                "reset : 再生を停止してキューをリセットします。\n" +
                "\ngithub : https://github.com/yurisi0212/DiscordMusicBot-dotNet");
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("join", RunMode = RunMode.Async)]
        [Alias("j")]
        public async Task Join() {
           //await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("Leave", RunMode = RunMode.Async)]
        [Alias("l")]
        public async Task Leave() {
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        public async Task Play(params string[] str) {
            var url = "";
            if (str.Length == 0) {
                await ReplyAsync("検索ワードかURLを入力してください。");
                return;
            }

            if(Uri.IsWellFormedUriString(str[0], UriKind.Absolute)) {
                url = str[0];
            } else {
                foreach (var word in str) 
                    url += word + " ";
            }

            await ReplyAsync("ロード中...");
            await _service.AddQueue(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, url);
        }

        [Command("delete", RunMode = RunMode.Async)]
        [Alias("d")]

        public async Task Delete(params string[] str) {
            var num = 1;
            if (str.Length != 1) {
                await ReplyAsync("曲番号を指定してください。");
                return;
            }

            if (int.TryParse(str[0], out num)) {

            }

        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("s")]
        public Task Skip() {
            _service.SkipAudio(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel);
            return Task.CompletedTask;
        }

        [Command("search", RunMode = RunMode.Async)]
        public async Task Search(params string[] url) {
            if (url.Length == 0) {
                await ReplyAsync("現在非対応です");
                return;
            }
            //Todo
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        public async Task Queue(params string[] url) {
            var num = 1;
            if (url.Length != 0) {
               if(int.TryParse(url[0],out num)) {
                    if(num < 1) {
                        await ReplyAsync("ページは1以上を指定してください。");
                    }
                } else {
                    await ReplyAsync("ページ番号は数字です。");
                }
            }
            await _service.GetQueueList(Context.Guild, Context.Channel,num);
        }

        [Command("loop", RunMode = RunMode.Async)]
        public Task Loop() {
            _service.ChangeLoop(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("qloop", RunMode = RunMode.Async)]
        public Task QLoop() {
            _service.ChangeQueueLoop(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("shuffle", RunMode = RunMode.Async)]
        public Task Shuffle() {
            _service.ChangeShuffle(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("reset", RunMode = RunMode.Async)]
        public Task Reset() {
            _service.ResetAudio(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }
    }
}
