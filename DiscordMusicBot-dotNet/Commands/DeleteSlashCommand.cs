using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    internal class DeleteSlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.DeleteCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "指定のIDの曲をキューから削除します";
            slashCommandBuilder.AddOption("id", ApplicationCommandOptionType.Integer, $"削除する曲のid(/{Setting.Data.QueueCommandName}で確認できます)", isRequired: true);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            int id;
            if (int.TryParse(command.Data.Options.First().Value.ToString(), out id)) {
                if (id < 2) {
                    await command.RespondAsync("現在再生中の音楽は削除できません。2以上を指定してください。");
                    return;
                }

                service.DeleteAudio(command.GuildId, command.Channel, id);
                await command.RespondAsync("音楽を削除します...");
                return;
            }
            await command.RespondAsync("idは数字で入力してください。");

        }
    }
}
