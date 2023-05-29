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
            _client.Log += LogHandler;
            _client.Ready += ReadyHandler;
            _client.SlashCommandExecuted += SlashCommandManager.SlashCommandExecutedHandler;
            if (Setting.Data.AutoLeave) _client.UserVoiceStateUpdated += UserVoiceStateUpdatedHandler;
            await _client.LoginAsync(TokenType.Bot, Setting.Data.Token);
            await _client.StartAsync();
            while (Setting.Data.ShowActivity) {
                await ClientSetGameAsync();
                await Task.Delay(10000);
            }
            await Task.Delay(-1);
        }

        private void Settings() {
            try {
                Setting.Setup();
                Console.WriteLine("Bot [" + Setting.Data.BotName + "] を起動します...");
            } catch (ConfigVerificationException e) {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            } catch {
                Console.WriteLine("settings.iniが破損しているか、存在していません。");
                Environment.Exit(1);
            }
        }

        private Task LogHandler(LogMessage message) {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }


        private async Task ReadyHandler() {
            _service = new AudioService(_client);
            SlashCommandManager.RegisterSlashCommand(_client, _service);
            await _client.SetGameAsync("");
            if (Setting.Data.ShowActivity) await ClientSetGameAsync();
        }

        private async Task UserVoiceStateUpdatedHandler(SocketUser user, SocketVoiceState before, SocketVoiceState after) {
            var beforeVC = before.VoiceChannel;
            if (beforeVC == null) return;
            if (beforeVC.ConnectedUsers.Any(u => u.Id == _client.CurrentUser.Id)) {
                if (beforeVC.ConnectedUsers.Count == 1) {
                    await _service.LeaveAudio(before.VoiceChannel.Guild.Id);
                }
            }
        }

        private async Task ClientSetGameAsync() {
            var vcCount = 0;
            foreach (var guild in _client.Guilds) {
                foreach (var vc in guild.VoiceChannels) {
                    if (vc.ConnectedUsers.Any(u => u.Id == _client.CurrentUser.Id)) {
                        vcCount++;
                    }
                }
            }
            await _client.SetGameAsync("♫MusicPlayer♫ /join | " + _client.Guilds.Count.ToString() + " servers | " + vcCount.ToString() + " vc");
        }
    }
}