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

        private List<Audio> _queue = new();

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
                        throw new SearchNotFoundException("なかった");
                    }
                    break;

                default:
                    audio = null;
                    break;
            }
            return audio;
        }

        public void AddQueue(Audio audio) {
            _queue.Add(audio);
        }

        public void RemoveQueue(int index) {
            _queue.RemoveAt(index);
        }

        public Audio GetAudio() {
            return _queue[_player.NowQueue];
        }

        public AudioPlayer GetAudioPlayer() {
            return _player;
        }

        public int GetQueueCount() {
            return _queue.Count();
        }

        public bool IsQueueinMusic() {
            return _queue.Count != 0;
        }

        public string[] GetQueueMusicTitles() {
            if (_queue.Count == 0) return null;
            string[] titles = new string[_queue.Count];
            foreach (var item in _queue.Select((value, index) => new { index, value }))
                titles[item.index] = item.value.Title;
            return titles;
        }

        public Audio getMusicinQueue(int index) {
            if (_queue.Count < index) throw new QueueNotFoundException("indexが存在しません");
            return _queue[index];
        }

        public Audio Next() {
            if (_player.QueueLoop) {
                if (_queue.Count >= _player.NowQueue) {
                    _player.NowQueue += 1;
                    return _queue[_player.NowQueue];
                } else {
                    _player.NowQueue = 0;
                    return _queue[_player.NowQueue];
                }
            }
            if (!_player.Loop) {
                RemoveQueue(0);
            }
            if (_queue.Count == 0) return null;
            return _queue[0];
        }


        public int GetRandomIndex() {
            while (true) {
                var rand = new Random().Next(0, _queue.Count);
                if (rand != _player.NowQueue) {
                    return rand;
                }
            }
        }

        public void Reset() {
            _queue = new();
            _player.NowQueue = 0;
            _player.Loop = false;
            _player.Shuffle = false;
        }
    }
}
