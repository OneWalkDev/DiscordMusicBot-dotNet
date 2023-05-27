using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class JoinSlashCommand : BaseSlashCommand {
        public override string Name => Settings.JoinCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "VCに参加します";

            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            await command.RespondAsync("参加します...");
            await service.JoinAudio(command.GuildId, command.Channel, (command.User as IVoiceState).VoiceChannel);
        }
    }
}
