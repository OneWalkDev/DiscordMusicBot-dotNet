using Discord;
using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Core;
using DiscordMusicBot_dotNet.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Audio {
    public class QueueManager {

        private AudioPlayer _player;

        private List<Audio> queue = new List<Audio>();

        public QueueManager(AudioPlayer player) {
            _player = player;
        }

        public async Task<Audio> GetAudioforString(string str) {
            Audio audio;
            switch (DownloadHelper.GetType(str).Result) {
                case YoutubeType.Video:
                    audio = DownloadHelper.GetAudio(str).Result;
                    break;

                case YoutubeType.Playlist:
                    //Todo
                    audio = null;
                    break;

                case YoutubeType.Search:
                    audio = DownloadHelper.Search(str).Result;
                    if (audio.Path == string.Empty
                        || audio.Title == string.Empty
                        || audio.Url == string.Empty) {
                        throw new SearchNotFoundException("見つかりませんでした。");
                    }
                    break;

                default:
                    audio = null;
                    break;
            }
            return audio;
        }

        public void AddQueue(Audio audio) {
            queue.Add(audio);
        }

        public void RemoveQueue(int index) {
            queue.RemoveAt(index);
        }

        public Audio GetAudio() {
            return queue[_player.NowQueue];
        }

        public int GetQueueCount() {
            return queue.Count();
        }

        public bool IsQueueinMusic() {
            return queue.Count != 0;
        }

        public string[] GetQueueMusicTitles() {
            string[] titles = new string[queue.Count];
            foreach(var item in queue.Select((value, index) => new { index, value })) 
                titles[item.index] = item.value.Title;
            return titles;
        }

        public Audio getMusicinQueue(int index) {
            if (queue.Count < index) throw new QueueNotFoundException("indexが存在しません");
            return queue[index];
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

        public int GetRandomIndex() {
            while (true) {
                var rand = new Random().Next(0, queue.Count);
                if (rand != _player.NowQueue) {
                    return rand;
                }
            }
        }
    }
}
