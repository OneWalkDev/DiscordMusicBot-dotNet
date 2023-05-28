using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    internal class SkipSlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.SkipCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "曲をスキップします。";
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            await command.RespondAsync("スキップします...");
            service.SkipAudio(command.GuildId, command.Channel, (command.User as IVoiceState).VoiceChannel);
        }
    }
}
