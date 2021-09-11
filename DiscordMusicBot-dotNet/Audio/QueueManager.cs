﻿using DiscordMusicBot_dotNet.Assistor;
using DiscordMusicBot_dotNet.Core;
using DiscordMusicBot_dotNet.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordMusicBot_dotNet.Audio {
    public class QueueManager {

        private List<Audio> _queue = new();

        public AudioPlayer AudioPlayer { get; set; }

        public QueueManager(AudioPlayer player) {
            AudioPlayer = player;
        }

        public Audio GetAudioFromString(string str, YoutubeType type) {
            Audio audio;
            switch (type) {
                case YoutubeType.Video:
                    return DownloadHelper.GetAudio(str).Result;

                case YoutubeType.Search:
                    audio = DownloadHelper.Search(str).Result;
                    if (audio.Path == string.Empty
                        || audio.Title == string.Empty
                        || audio.Url == string.Empty) {
                        throw new SearchNotFoundException("なかった");
                    }
                    return audio;
            }
            return null;
        }


        public void AddQueue(Audio audio) {
            _queue.Add(audio);
        }

        public void RemoveQueue(int index) {
            _queue.RemoveAt(index);
        }

        public Audio GetAudio() {
            return _queue[AudioPlayer.NowQueue];
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
            if (AudioPlayer.QueueLoop) {
                if (_queue.Count >= AudioPlayer.NowQueue) {
                    AudioPlayer.NowQueue += 1;
                    return _queue[AudioPlayer.NowQueue];
                } else {
                    AudioPlayer.NowQueue = 0;
                    return _queue[AudioPlayer.NowQueue];
                }
            }
            if (!AudioPlayer.Loop) {
                RemoveQueue(0);
            }
            if (_queue.Count == 0) return null;
            return _queue[0];
        }


        public int GetRandomIndex() {
            while (true) {
                var rand = new Random().Next(0, _queue.Count);
                if (rand != AudioPlayer.NowQueue) {
                    return rand;
                }
            }
        }

        public void Reset() {
            _queue = new();
            AudioPlayer.NowQueue = 0;
            AudioPlayer.Loop = false;
            AudioPlayer.Shuffle = false;
        }
    }
}
