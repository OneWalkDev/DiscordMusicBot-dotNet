using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class QueueLoopSlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.QueueLoopCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var choice = new ApplicationCommandOptionChoiceProperties[2] {
                new ApplicationCommandOptionChoiceProperties() {
                    Name="する",
                    Value="true",
                },
               new ApplicationCommandOptionChoiceProperties() {
                    Name="しない",
                    Value="false"
                }
            };


            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "現在のキューをループします。";
            slashCommandBuilder.AddOption("キューループ", ApplicationCommandOptionType.String, "キューループをするか選択します", isRequired: false, choices: choice);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            if (command.Data.Options.Count == 0) {
                await command.RespondAsync("キューループを変更します。");
                service.ChangeQueueLoop(command.GuildId, command.Channel);
                return;
            }

            bool result;
            bool.TryParse(command.Data.Options.First().Value.ToString(), out result);

            await command.RespondAsync("キューループを設定します。");
            service.ChangeQueueLoop(command.GuildId, command.Channel, result);
        }
    }
}
