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

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        public async Task Play(params string[] url) {
            var user = Context.User as IGuildUser;
            if (url.Length == 0) {
                await ReplyAsync("むり");
                return;
            }
            await _service.SendAudioAsync(Context.Guild, Context.Channel,url[0]);
        }
    }
}
