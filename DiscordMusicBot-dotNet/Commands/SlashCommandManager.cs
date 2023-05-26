using Discord.Net;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class SlashCommandManager {

        private static readonly Dictionary<string, BaseSlashCommand> SlashCommands = new();
        private static AudioService _service;

        public async static Task RegisterSlashCommand(DiscordSocketClient client, AudioService service) {
            _service = service;
            try {
                if (Settings.Global) {
                    _ = CreateGrobalCommand<HelpSlashCommand>(client);
                    _ = CreateGrobalCommand<JoinSlashCommand>(client);
                    _ = CreateGrobalCommand<LeaveSlashCommand>(client);
                    _ = CreateGrobalCommand<PlaySlashCommand>(client);
                    _ = CreateGrobalCommand<SkipSlashCommand>(client);
                    _ = CreateGrobalCommand<QueueSlashCommand>(client);
                    _ = CreateGrobalCommand<LoopSlashCommand>(client);
                    _ = CreateGrobalCommand<QueueLoopSlashCommand>(client);
                    //_ = CreateGrobalCommand<ShuffleSlashCommand>(client);
                    _ = CreateGrobalCommand<ResetSlashCommand>(client);
                    // _ =CreateGuildCommand<SearchSlashCommand>(client);
                    // _ =CreateGuildCommand<DeleteSlashCommand>(client);
                } else {
                    ulong guildId = Settings.GuildId;
                    _ = CreateGuildCommand<HelpSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<JoinSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<LeaveSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<PlaySlashCommand>(client, guildId);
                    _ = CreateGuildCommand<SkipSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<QueueSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<LoopSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<QueueLoopSlashCommand>(client, guildId);
                    //_ = CreateGuildCommand<ShuffleSlashCommand>(client, guildId);
                    _ = CreateGuildCommand<ResetSlashCommand>(client, guildId);
                    // _ =CreateGuildCommand<SearchSlashCommand>(client, guildId);
                    // _ =CreateGuildCommand<DeleteSlashCommand>(client, guildId);
                }


            } catch (HttpException exception) {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private static async Task CreateGuildCommand<T>(DiscordSocketClient client, ulong guildId) where T : BaseSlashCommand, new() {
            var command = new T();
            SlashCommands.Add(command.Name, command);
            Console.WriteLine($"[DiscordMusicBotDotnet]    Register [{command.Name}] Guild Command");
            await client.Rest.CreateGuildCommand(command.CommandBuilder().Build(), guildId);
        }

        private static async Task CreateGrobalCommand<T>(DiscordSocketClient client) where T : BaseSlashCommand, new() {
            var command = new T();
            SlashCommands.Add(command.Name, command);
            Console.WriteLine($"[DiscordMusicBotDotnet]    Register [{command.Name}] Global Command");
            await client.Rest.CreateGlobalCommand(command.CommandBuilder().Build());

        }

        public static async Task SlashCommandHandler(SocketSlashCommand command) {
            if (!SlashCommands.ContainsKey(command.Data.Name)) return;
            await SlashCommands[command.Data.Name].Execute(command, _service);
        }

    }
}
