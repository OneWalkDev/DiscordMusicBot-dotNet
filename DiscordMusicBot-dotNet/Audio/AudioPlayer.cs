using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Audio;

namespace DiscordMusicBot_dotNet.Core {
    public class AudioPlayer {

        public PlaybackState PlaybackState { get; set; }

        public bool Loop { get; set; }
        
        public bool Shuffle { get; set; }

        public QueueManager Queue { get; set; }

        public int NowQueue { get; set; }
        
    }
}
