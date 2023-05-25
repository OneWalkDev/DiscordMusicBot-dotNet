using Discord.Net;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class SlashCommandManager {

        private static readonly Dictionary<string, BaseSlashCommand> SlashCommands = new();
        private static AudioService _service;

        public static async Task RegisterSlashCommandAsync(DiscordSocketClient client, AudioService service) {
            ulong guildId = 980903882437836860;

            _service = service;

            try {
                await CreateGuildCommand<HelpSlashCommand>(client, guildId);
                await CreateGuildCommand<JoinSlashCommand>(client, guildId);
            } catch (HttpException exception) {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private static async Task CreateGuildCommand<T>(DiscordSocketClient client, ulong guildId) where T : BaseSlashCommand, new() {
            var command = new T();
            SlashCommands.Add(command.Name, command);
            await client.Rest.CreateGuildCommand(command.CommandBuilder().Build(), guildId);
        }

        public static async Task SlashCommandHandler(SocketSlashCommand command) {
            if (!SlashCommands.ContainsKey(command.Data.Name)) return;
            await SlashCommands[command.Data.Name].Execute(command, _service);
        }

    }
}
