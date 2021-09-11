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
                "join : botを入室\n" +
                "leave : botが退出\n" +
                "play [YoutubeURL,検索したいワード] : 曲をキューに追加\n" +
                "skip : 曲をスキップ\n" +
                "stop : 曲をストップ\n" +
                "search : 動画をyoutubeから探す\n" +
                "loop : 1曲ループする\n" +
                "qloop : キュー内をループする\n" +
                "shuffle : ループしシャッフル再生する\n" +
                "\ngithub : https://github.com/yurisi0212/DiscordMusicBot-dotNet");
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
        public async Task Play(params string[] str) {
            var url = "";
            if (str.Length == 0) {
                await ReplyAsync("むり");
                return;
            }

            foreach(var word in str) 
                url += word + " ";

            await ReplyAsync("ロード中...");
            await _service.AddQueue(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, str[0]);
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("s")]
        public Task Skip() {
            _service.SkipAudio(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel);
            return Task.CompletedTask;
        }

        /*[Command("stop", RunMode = RunMode.Async)]
        public Task Stop() {
            _service.StopAudio(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }*/

        [Command("search", RunMode = RunMode.Async)]
        public async Task Search(params string[] url) {
            if (url.Length == 0) {
                await ReplyAsync("むり");
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
                        await ReplyAsync("1以上じゃないとむり");
                    }
                } else {
                    await ReplyAsync("ページ番号数字じゃないとむり");
                }
            }
            await _service.GetQueueList(Context.Guild, Context.Channel,num);
        }

        [Command("loop", RunMode = RunMode.Async)]
        public Task Loop(params string[] url) {
            _service.ChangeLoop(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("qloop", RunMode = RunMode.Async)]
        public Task QLoop(params string[] url) {
            _service.ChangeQueueLoop(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("shuffle", RunMode = RunMode.Async)]
        public Task Shuffle(params string[] url) {
            _service.ChangeShuffle(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }

        [Command("Reset", RunMode = RunMode.Async)]
        public Task Reset(params string[] url) {
            _service.ResetAudio(Context.Guild, Context.Channel);
            return Task.CompletedTask;
        }
    }
}
