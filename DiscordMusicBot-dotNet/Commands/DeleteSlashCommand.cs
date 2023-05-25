using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    internal class DeleteSlashCommand : BaseSlashCommand {
        public override string Name => Settings.DeleteCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "指定のIDの曲をキューから削除します";
            slashCommandBuilder.AddOption("id", ApplicationCommandOptionType.Integer, $"削除する曲のid(/{Settings.QueueCommandName}で確認できます)", isRequired: true);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            /*var num = 1;
            if (str.Length != 1) {
                await ReplyAsync("曲番号を指定してください。");
                return;
            }

            if (int.TryParse(str[0], out num)) {

            }*/
            // TODO
            await command.RespondAsync("未実装");
        }
    }
}
