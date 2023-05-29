using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Commands;
using DiscordMusicBot_dotNet.Services;
using DiscordMusicBot_dotNet.Configurations;
using System;
using System.Threading.Tasks;
using DiscordMusicBot_dotNet.Exception;
using System.Linq;


namespace DiscordMusicBot_dotNet.Core {
    class Main {
        private DiscordSocketClient _client;
        private AudioService _service;

        public async Task MainAsync() {
            Settings();
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.SlashCommandExecuted += SlashCommandManager.SlashCommandHandler;
            if (Setting.Data.AutoLeave) _client.UserVoiceStateUpdated += UserVoiceStateUpdatedHandler;
            await _client.LoginAsync(TokenType.Bot, Setting.Data.Token);
            await _client.StartAsync();
            await _client.SetGameAsync("");
            await Task.Delay(-1);
        }

        private async Task ReadyAsync() {
            _service = new AudioService(_client);
            SlashCommandManager.RegisterSlashCommand(_client, _service);
        }

        private Task Log(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private void Settings() {
            try {
                Setting.Setup();
                Console.WriteLine("Bot [" + Setting.Data.BotName + "] を起動します...");
            } catch (ConfigVerificationException e) {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            } catch {
                Console.WriteLine("settings.iniが破損または存在していません。");
                Environment.Exit(1);
            }
        }

        private async Task UserVoiceStateUpdatedHandler(SocketUser user, SocketVoiceState before, SocketVoiceState after) {
            var beforeVC = before.VoiceChannel;
            if (beforeVC == null) return;
            if (beforeVC.ConnectedUsers.Any(u => u.Id == _client.CurrentUser.Id)) {
                if(beforeVC.ConnectedUsers.Count == 1) {
                    await _service.LeaveAudio(before.VoiceChannel.Guild.Id);
                }
            }
            
        }

    }
}
