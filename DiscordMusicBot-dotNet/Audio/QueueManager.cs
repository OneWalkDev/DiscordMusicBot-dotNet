using DiscordMusicBot_dotNet.Core;
using System.Collections.Generic;

namespace DiscordMusicBot_dotNet.Audio {
    public class QueueManager {

        private AudioPlayer _player;

        public QueueManager(AudioPlayer player) {
            _player = player;
        }

        private List<Audio> queue = new List<Audio>();

        public void AddQueue(Audio audio) {
            queue.Add(audio);
        }


        public Audio Next() {
            if(queue.Count >= _player.NowQueue) {
                _player.NowQueue += 1;
                return queue[_player.NowQueue];
            } else {
                _player.NowQueue = 0;
                if (_player.Loop) {
                    return queue[_player.NowQueue];
                }
                return null;
            }
        }
    }
}
