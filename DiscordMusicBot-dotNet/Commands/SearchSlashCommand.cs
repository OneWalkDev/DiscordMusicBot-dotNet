using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class SearchSlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.SearchCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "音楽を検索します";
            slashCommandBuilder.AddOption("曲名", ApplicationCommandOptionType.String, "曲名を入力してください。", isRequired: true);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            /**if (url.Length == 0) {
                await ReplyAsync("現在非対応です");
                return;
            }**/
            //Todo
            await command.RespondAsync("未実装");
        }
    }
}
