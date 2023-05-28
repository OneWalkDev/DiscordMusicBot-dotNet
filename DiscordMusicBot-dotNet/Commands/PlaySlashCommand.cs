using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class PlaySlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.PlayCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "キューに音楽を追加します。";
            slashCommandBuilder.AddOption("曲名", ApplicationCommandOptionType.String, "曲名かYoutubeのURLを入力してください。", isRequired: true);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            await command.RespondAsync("ロード中...");
            if(command.Data.Options.First().Value is string music) {
                await service.AddQueue(command.GuildId, command.Channel, (command.User as IVoiceState).VoiceChannel, music);
            }
        }
    }
}
