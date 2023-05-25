using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class LoopSlashCommand : BaseSlashCommand {
        public override string Name => Settings.LoopCommandName;

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
            slashCommandBuilder.Description = "現在再生している曲をループします。";
            slashCommandBuilder.AddOption("ループ", ApplicationCommandOptionType.String, "ループをするか選択します", isRequired: false, choices: choice);
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            if(command.Data.Options.Count == 0) {
                await command.RespondAsync("ループを変更します。");
                service.ChangeLoop(command.GuildId, command.Channel);
                return;
            }

            bool result;
            bool.TryParse(command.Data.Options.First().Value.ToString(), out result);

            await command.RespondAsync("ループを設定します。");
            service.ChangeLoop(command.GuildId, command.Channel, result);
        }
    }
}
