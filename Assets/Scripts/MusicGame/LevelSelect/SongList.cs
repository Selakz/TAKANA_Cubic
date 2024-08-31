using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Takana3.MusicGame.LevelSelect
{
    public class SongList
    {
        private static readonly string path = $"InfoData/SongList";

        // Serializable and Public
        public SongInfo this[int index] => filteredSongs[index];
        public int Count => filteredSongs.Count;

        // Private
        // TODO: 选关界面排序思路：选关界面用一个List<SongInfo>决定展示的歌。先GetSongList()，然后
        // 对这个SongList进行或排序或筛选的函数，用函数返回的List进行展示。而内部实际上是一直存着所有
        // 的songs，筛选则用songs生成新的filteredSongs，排序则是直接对filteredSongs进行排序。
        // TODO: 用MultiSortList替换
        private readonly List<SongInfo> songs;
        private List<SongInfo> filteredSongs;

        // Defined Functions
        public SongList()
        {
            string json = MyResources.Load<TextAsset>(path).text;
            songs = JsonConvert.DeserializeObject<List<SongInfo>>(json);
            filteredSongs = new(songs);
        }

        public SongInfo GetSongInfo(int idx)
        {
            // 歌曲idx总是非负的，所以歌曲在list中的下标总是小于等于其idx
            for (int i = Math.Min(idx, songs.Count - 1); i >= 0; i--)
            {
                if (songs[i].idx == idx)
                {
                    return songs[i];
                }
            }
            return null;
        }
        public SongInfo GetSongInfo(string id)
        {
            for (int i = songs.Count - 1; i >= 0; i--)
            {
                if (songs[i].id == id)
                {
                    return songs[i];
                }
            }
            return null;
        }

        public static bool SaveScore(int idx, int difficulty, int score)
        {
            bool flag = false;
            SongList songList = new();
            SongInfo songInfo = songList.GetSongInfo(idx);
            foreach (DiffInfo diff in songInfo.difficulties)
            {
                if (diff.difficulty == difficulty) diff.score = score;
                flag = true;
            }
            SaveSongList(songList);
            return flag;
        }

        private static void SaveSongList(SongList toSave)
        {
            // TODO: 排序、检查id和idx是否有重复。暂时没有方法涉及这些情况，所以无所谓
            // TODO: 其他数据合法性检查
            // TODO: ?
            JsonSerializerSettings jsetting = new()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            File.WriteAllText(path, JsonConvert.SerializeObject(toSave, Formatting.Indented, jsetting));
        }
    }
}
