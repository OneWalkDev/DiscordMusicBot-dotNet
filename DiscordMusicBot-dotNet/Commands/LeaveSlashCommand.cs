using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class LeaveSlashCommand : BaseSlashCommand {
        public override string Name => Settings.LeaveCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "VCから退出します";

            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            await service.LeaveAudio(command.GuildId);
            await command.RespondAsync("bye!");
        }
    }
}
