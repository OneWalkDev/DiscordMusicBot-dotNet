using Discord;
using Discord.WebSocket;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Services {
    public class VoiceChatService {

        public static async Task<bool> Join(IVoiceChannel voiceChannel, IMessageChannel channel) {
            if (voiceChannel == null) {
                await channel.SendMessageAsync("むり");
                return false;
            }
            var guildId = voiceChannel.Guild.Id;

            await voiceChannel.ConnectAsync();

            
            return true;
        }
    }
}
