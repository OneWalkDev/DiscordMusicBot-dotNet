using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Core;
using DiscordMusicBot_dotNet.Exception;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordMusicBot_dotNet.Audio {
    public class QueueManager {

        private List<Audio> _queue = new();

        public AudioPlayer AudioPlayer { get; set; }

        public QueueManager(AudioPlayer player) {
            AudioPlayer = player;
        }

        public Audio GetAudioFromString(string str, YoutubeType type) {
            return type switch {
                YoutubeType.Video => StreamHelper.GetAudio(str).Result,
                YoutubeType.Search => StreamHelper.Search(str).Result,
                _ => null
            };
        }


        public void AddQueue(Audio audio) {
            _queue.Add(audio);
        }

        public void RemoveQueue(int index) {
            _queue.RemoveAt(index);
        }

        public Audio GetNowAudio() {
            return _queue[AudioPlayer.NowQueue];
        }

        public bool ExistsQueueIndex(int index) {
            return _queue.Count - 1 >= index;
        }

        public int GetQueueCount() {
            return _queue.Count();
        }

        public bool IsQueueinMusic() {
            return _queue.Count != 0;
        }

        public string GetNowPlayingMusicTitle() {
            if (_queue.Count == 0) return null;
            return _queue[AudioPlayer.NowQueue].Title;
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
            if (AudioPlayer.Loop) {
                return _queue[0];
            }

            if (AudioPlayer.QueueLoop) {
                _queue.Add(_queue[0]);
            }

            RemoveQueue(0);
            

            if (_queue.Count == 0) return null;

            if (AudioPlayer.Shuffle) {
                _queue = _queue.OrderBy(a => Guid.NewGuid()).ToList();
            }

            return _queue[0];
        }


        public void Reset() {
            _queue = new();
            AudioPlayer.NowQueue = 0;
            AudioPlayer.Loop = false;
            AudioPlayer.Shuffle = false;
        }

        public Audio? Delete(int userSelectNum) {
            var num = userSelectNum - 1;
            if (ExistsQueueIndex(num)) {
                var audio = _queue[num];
                RemoveQueue(num);
                return audio;
            }
            return null;
        }
    }
}
