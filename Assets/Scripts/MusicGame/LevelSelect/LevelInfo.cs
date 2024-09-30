using System.Collections.Generic;
using System.IO;
using Takana3.Settings;
using UnityEngine;

namespace Takana3.MusicGame.LevelSelect
{
    public class LevelInfo : IPassInfo
    {
        public MusicSetting MusicSetting { get; set; }
        public Texture2D Cover { get; set; }
        public AudioClip Music { get; set; }
        public SongInfo SongInfo { get; set; }
        public ChartInfo ChartInfo { get; set; }
        public List<InputInfo> InputInfos { get; set; }
        public int Difficulty { get; set; }
        public string LevelPath { get; set; } = string.Empty;
        public LayerInfo LayerInfo { get; set; } = null;

        public LevelInfo(SongInfo songInfo, int difficulty)
        {
            SongInfo = songInfo;
            Difficulty = difficulty;
            LevelPath = $"Levels/{songInfo.id}/";

            Cover = MyResources.Load<Texture2D>(LevelPath + "cover");
            Music = MyResources.Load<AudioClip>(LevelPath + "music");

            MusicSetting = MusicSetting.GameLoad();

            ChartInfo = ChartReader.Read(songInfo.id, difficulty, MusicSetting);
            InputInfos = new();
        }

        public LevelInfo(string path, int difficulty)
        {
            LevelPath = path;
            // SongInfo
            if (File.Exists(Path.Combine(path, "Takana_Data", "song.json")))
            {
                SongInfo = SongInfo.Load(Path.Combine(path, "Takana_Data", "song.json"));
            }
            else
            {
                SongInfo = new SongInfo()
                {
                    difficulties = new()
                    {
                        new(){ difficulty = 1 },
                        new(){ difficulty = 2 },
                        new(){ difficulty = 3 },
                        new(){ difficulty = 4 },
                        new(){ difficulty = 5 },
                    }
                };
                SongInfo.Save(Path.Combine(path, "Takana_Data", "song.json"));
            }
            // Difficulty
            Difficulty = difficulty;
            // Cover
            Cover = File.Exists(Path.Combine(path, "cover.jpg")) ?
                Loader.LoadTexture2D(Path.Combine(path, "cover.jpg")) :
                MyResources.Load<Texture2D>("Images/Background/Default");
            // Music
            Music = Loader.LoadAudioFile(Path.Combine(path, "music.mp3"));
            // MusicSetting
            MusicSetting = MusicSetting.EditorLoad();
            // Chart
            ChartInfo = ChartReader.ReadExternal(path, difficulty, MusicSetting, out LayerInfo layerInfo);
            LayerInfo = layerInfo;
            InputInfos = new();
        }
    }
}
