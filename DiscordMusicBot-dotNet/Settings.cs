namespace DiscordMusicBot_dotNet {
    public class Settings {

        /// <summary>
        /// Botの名前
        /// </summary>
        public const string BotName = "DiscordMusicBot.Net";

        /// <summary>
        /// 2つ以上のサーバーで使用する場合はtrueにしてください。
        /// コマンドの登録に1時間ほど掛かる可能性があります。
        /// </summary>
        public const bool Global = false;

        /// <summary>
        /// Globalがfalseのときに入力してください。
        /// サーバーのIDを入力してください。
        /// </summary>
        public const ulong GuildId = 980903882437836860;

        /// <summary>
        /// helpコマンドの名前
        /// </summary>
        public const string HelpCommandName = "help";

        /// <summary>
        /// Joinコマンドの名前
        /// </summary>
        public const string JoinCommandName = "join";

        /// <summary>
        /// Leaveコマンドの名前
        /// </summary>
        public const string LeaveCommandName = "leave";

        /// <summary>
        /// Playコマンドの名前
        /// </summary>
        public const string PlayCommandName = "play";


        /// <summary>
        /// skipコマンドの名前
        /// </summary>
        public const string SkipCommandName = "skip";

        /// <summary>
        /// queueコマンドの名前
        /// </summary>
        public const string QueueCommandName = "queue";

        /// <summary>
        /// loopコマンドの名前
        /// </summary>
        public const string LoopCommandName = "loop";

        /// <summary>
        /// qloopコマンドの名前
        /// </summary>
        public const string QueueLoopCommandName = "qloop";


        /// <summary>
        /// deleteコマンドの名前
        /// </summary>
        public const string DeleteCommandName = "delete";


        /// <summary>
        /// searchコマンドの名前
        /// </summary>
        public const string SearchCommandName = "search";


        /// <summary>
        /// shuffleコマンドの名前
        /// </summary>
        public const string ShuffleCommandName = "shuffle";


        /// <summary>
        /// resetコマンドの名前
        /// </summary>
        public const string ResetCommandName = "reset";

        ///<summary>
        /// statusコマンドの名前
        /// </summary>
        public const string StatusCommandName = "status";

    }
}
