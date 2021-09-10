using Discord.Audio;
using DiscordMusicBot_dotNet.Audio;
using NAudio.Wave;
using System.Threading;

namespace DiscordMusicBot_dotNet.Assistor {
    public class AudioContainer {
        public IAudioClient AudioClient { get; set; }
        public AudioOutStream AudioOutStream { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public QueueManager QueueManager { get; set; }
        public ResamplerDmoStream ResamplerDmoStream { get; set; }
    }
}
