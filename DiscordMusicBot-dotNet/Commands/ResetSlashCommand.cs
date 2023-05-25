using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    internal class ResetSlashCommand : BaseSlashCommand {
        public override string Name => Settings.ResetCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "キューをすべて削除します。";
            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            await command.RespondAsync("リセットを実行します。");
            service.ResetAudio(command.GuildId, command.Channel);
        }
    }
}
