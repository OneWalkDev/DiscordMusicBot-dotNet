using DiscordMusicBot_dotNet.Exception;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DiscordMusicBot_dotNet.Configurations {
    public class Setting {

        public static AppSettingsConfig Data { get; set; }

        public static void Setup() {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddIniFile("settings.ini", optional: false);
            var configuration = builder.Build();
            Data = configuration.Get<AppSettingsConfig>();
            Verification();
        }

        private static void Verification() {
            if (Data.Token == string.Empty) throw new ConfigVerificationException("Tokenが入力されていないか、settings.iniが壊れています。");
            if (Data.BotName == string.Empty) throw new ConfigVerificationException("BotNameが入力されていないか、settings.iniが壊れています。");
            if (Data.Global == false && Data.GuildId == null) throw new ConfigVerificationException("Globalがfalseの時はGuildIdは必須です。または、settings.iniが壊れています。");
            if (Data.HelpCommandName == string.Empty) throw new ConfigVerificationException("HelpCommandNameが入力されていないか、settings.iniが壊れています。");
            if (Data.JoinCommandName == string.Empty) throw new ConfigVerificationException("JoinCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.LeaveCommandName == string.Empty) throw new ConfigVerificationException("LeaveCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.PlayCommandName == string.Empty) throw new ConfigVerificationException("PlayCommandNameが入力されていないか、settings.iniが壊れています。");
            if (Data.SkipCommandName == string.Empty) throw new ConfigVerificationException("SkipCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.QueueCommandName == string.Empty) throw new ConfigVerificationException("QueueCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.LoopCommandName == string.Empty) throw new ConfigVerificationException("LoopCommandNameが入力されていないか、settings.iniが壊れています。");
            if (Data.QueueLoopCommandName == string.Empty) throw new ConfigVerificationException("QueueLoopCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.DeleteCommandName == string.Empty) throw new ConfigVerificationException("DeleteCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.SearchCommandName == string.Empty) throw new ConfigVerificationException("SearchCommandNameが入力されていないか、settings.iniが壊れています。");
            if (Data.ShuffleCommandName == string.Empty) throw new ConfigVerificationException("ShuffleCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.StatusCommandName == string.Empty) throw new ConfigVerificationException("StatusCommandNameが入力されていないか、settings.iniが壊れています");
            if (Data.NextPlayCommandName == string.Empty) throw new ConfigVerificationException("NextPlayCommandNameが入力されていないか、settings.iniが壊れています");
        }
    }
}
