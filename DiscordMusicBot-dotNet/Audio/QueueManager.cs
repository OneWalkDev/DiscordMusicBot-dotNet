using System.Collections.Generic;

namespace DiscordMusicBot_dotNet.Audio {
    public class QueueManager {

        private List<Audio> queue = new List<Audio>();

        public void AddQueue(Audio audio) {
            queue.Add(audio);
        }

    }
}
