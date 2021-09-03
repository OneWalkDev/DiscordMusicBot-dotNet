using Discord;
using Discord.Commands;
using DiscordMusicBot_dotNet.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Command {

    public class CommandModule : ModuleBase<SocketCommandContext> {

        
        /*public async Task Mp(params string[] msg) {
            if (msg.Length < 2) {
                await ReplyAsync("むり");
                return;
            }

        }*/

        [Command("join", RunMode = RunMode.Async)]
        [Alias("j")]
        public async Task Join() {
            var user = Context.User as IGuildUser;
            await VoiceChatService.Join(user.VoiceChannel, Context.Channel);
        }
    }
}
