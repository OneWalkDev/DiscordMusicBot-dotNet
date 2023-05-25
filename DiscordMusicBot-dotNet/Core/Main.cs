using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Commands;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Core {
    class Main {
        private DiscordSocketClient _client;
        private TokenManager _token;
        private AudioService _audio;

        public async Task MainAsync() {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.SlashCommandExecuted += SlashCommandManager.SlashCommandHandler;
            _token = new TokenManager();
            await _client.LoginAsync(TokenType.Bot, _token.DiscordToken);
            await _client.StartAsync();
            await _client.SetGameAsync(null);
            await Task.Delay(-1);
        }

        private async Task ReadyAsync() {
            _audio = new AudioService(_client);
            await SlashCommandManager.RegisterSlashCommand(_client, _audio);
        }

        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}
