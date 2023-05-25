
using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class HelpSlashCommand : BaseSlashCommand {

        public override string Name => "help";

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "ヘルプを表示します";

            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            var embed = new EmbedBuilder();
            embed.WithTitle("DiscordMusicBot.NET ヘルプ");
            embed.WithColor(Color.Blue);
            embed.WithTimestamp(DateTime.Now);
            embed.WithDescription("エイリアスは*です\n" +
                "help : ヘルプを表示\n" +
                "join(j) : botを入室\n" +
                "leave(l) : botが退出\n" +
                "play(p) [YoutubeURL,検索したいワード] : 曲をキューに追加\n" +
                "skip(s) : 曲をスキップ\n" +
                "search : 動画をyoutubeから探す(現在非対応)\n" +
                "loop : 1曲ループする\n" +
                "qloop : キュー内をループする\n" +
                "shuffle : ループしシャッフル再生する\n" +
                "quere(q) : キューの中に入ってる曲を表示\n" +
                "reset : 再生を停止してキューをリセットします。\n" +
                "\ngithub : https://github.com/yurisi0212/DiscordMusicBot-dotNet");
            await command.RespondAsync(embed: embed.Build());
        }
    }
}
