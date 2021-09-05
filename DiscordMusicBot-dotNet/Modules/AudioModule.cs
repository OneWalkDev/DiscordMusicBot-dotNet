using Discord;
using Discord.Commands;
using DiscordMusicBot_dotNet.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Command {

    public class AudioModule : ModuleBase<SocketCommandContext> {

        private readonly AudioService _service;

        public AudioModule(AudioService service) {
            _service = service;
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
            await _service.SendAudioAsync(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, url[0]);
        }

        [Command("search", RunMode = RunMode.Async)]
        public async Task Search(params string[] url) {
            var user = Context.User as IGuildUser;
            if (url.Length == 0) {
                await ReplyAsync("むり");
                return;
            }
            await _service.SearchAudioAsync(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, url[0]);
        }
    }
}
