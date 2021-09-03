namespace DiscordMusicBot_dotNet {
    public class TokenManager {

        public TokenManager() {
            DiscordToken = "";
        }

        public string DiscordToken { get; private set; }
    }

}