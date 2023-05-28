using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class QueueSlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.QueueCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "現在の曲リストを取得します";
            slashCommandBuilder.AddOption("ページ", ApplicationCommandOptionType.String, "ページの指定がある際は入力してください。", isRequired: false);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            _ = Task.Run(async () => {
                int id;
                try{
                    int.TryParse(command.Data.Options.First().Value.ToString(), out id);
                } catch {
                    id = 1;
                }
                if (id == 0) id = 1;
                

                service.GetQueueList(command.GuildId, command.Channel, id);
            });

            await command.RespondAsync("キューを読み込み中...");
        }
    }
}
