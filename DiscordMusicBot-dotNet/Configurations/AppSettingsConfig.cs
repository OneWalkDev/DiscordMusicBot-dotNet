namespace DiscordMusicBot_dotNet.Configurations {
    public class AppSettingsConfig {

        public string Token { get; set; }

        public string BotName { get; set; }

        public bool Global { get; set; }

        public ulong? GuildId { get; set; }

        public bool AutoLeave { get; set; }

        public string HelpCommandName { get; set; }

        public string JoinCommandName { get; set; }

        public string LeaveCommandName { get; set; }

        public string PlayCommandName { get; set; }

        public string SkipCommandName { get; set; }

        public string QueueCommandName { get; set; }

        public string LoopCommandName { get; set; }

        public string QueueLoopCommandName { get; set; }

        public string DeleteCommandName { get; set; }

        public string SearchCommandName { get; set; }

        public string ShuffleCommandName { get; set; }

        public string ResetCommandName { get; set; }

        public string StatusCommandName { get; set; }

        public string NextPlayCommandName { get; set; }
    }
}
