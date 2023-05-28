using Discord;
using Discord.WebSocket;
using DiscordMusicBot_dotNet.Configurations;
using DiscordMusicBot_dotNet.Services;
using System;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Commands {
    public class HelpSlashCommand : BaseSlashCommand {

        public override string Name => Setting.Data.HelpCommandName;

        public override SlashCommandBuilder CommandBuilder() {
            var slashCommandBuilder = new SlashCommandBuilder();
            slashCommandBuilder.Name = Name;
            slashCommandBuilder.Description = "ヘルプを表示します";

            return slashCommandBuilder;
        }

        public async override Task Execute(SocketSlashCommand command, AudioService service) {
            var embed = new EmbedBuilder();
            embed.WithTitle($"{Setting.Data.BotName} ヘルプ");
            embed.WithColor(Color.Blue);
            embed.WithTimestamp(DateTime.Now);
            embed.WithDescription(
                $"/{Setting.Data.HelpCommandName} : ヘルプを表示\n" +
                $"/{Setting.Data.JoinCommandName} : botを入室\n" +
                $"/{Setting.Data.LeaveCommandName} : botが退出\n" +
                $"/{Setting.Data.PlayCommandName} [YoutubeURL,検索したいワード] : 曲をキューに追加\n" +
                $"/{Setting.Data.NextPlayCommandName} [YoutubeURL,検索したいワード] : 曲をキューの先頭に割り込み追加\n" +
                $"/{Setting.Data.DeleteCommandName} [id] : キューから指定されたIDの音楽を削除\n" +
                $"/{Setting.Data.QueueCommandName} : キューの中に入ってる曲を表示\n" +
                $"/{Setting.Data.SkipCommandName} : 曲をスキップ\n" +
                //$"/{Setting.Data.SearchCommandName} : 動画をyoutubeから探す(現在非対応)\n" +
                $"/{Setting.Data.LoopCommandName} : 1曲ループする\n" +
                $"/{Setting.Data.QueueLoopCommandName} : キュー内をループする\n" +
                $"/{Setting.Data.ShuffleCommandName} : シャッフル再生する\n" +
                $"/{Setting.Data.StatusCommandName} : 設定を表示します。\n" +
                $"/{Setting.Data.ResetCommandName} : 再生を停止してキューをリセットします。\n" +
                //ライセンス欄なので変更するとGPL3に違反する可能性があります
                "\ngithub : https://github.com/yurisi0212/DiscordMusicBot-dotNet\n" +
                "\n本プログラムはGNU General Public License v3.0に基づき配布されています。" +
                "\nライセンスの規定を守れば、誰でも自由に複製、改変、配布することができます。");
            await command.RespondAsync(embed: embed.Build());
        }
    }
}
