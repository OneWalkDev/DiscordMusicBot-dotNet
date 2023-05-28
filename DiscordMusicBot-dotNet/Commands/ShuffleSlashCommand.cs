using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    internal class ShuffleSlashCommand : BaseSlashCommand {
        public override string Name => Setting.Data.ShuffleCommandName;

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
            slashCommandBuilder.Description = "現在のキューをシャッフルして再生します。";
            slashCommandBuilder.AddOption("シャッフル", ApplicationCommandOptionType.String, "シャッフルをするか選択します", isRequired: false, choices: choice);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            if (command.Data.Options.Count == 0) {
                await command.RespondAsync("シャッフルを変更します。");
                service.ChangeShuffle(command.GuildId, command.Channel);
                return;
            }

            bool result;
            bool.TryParse(command.Data.Options.First().Value.ToString(), out result);

            await command.RespondAsync("シャッフルを設定します。");
            service.ChangeShuffle(command.GuildId, command.Channel, result);
        }
    }
}
