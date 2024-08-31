using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace Takana3.Settings
{
    /// <summary>
    /// 关卡开始前需要使用的必要设置信息
    /// </summary>
    public class MusicSetting
    {
        private static readonly string gamePath = $"InfoData/MusicSetting";
        private static readonly string editorPath = Path.Combine(Application.streamingAssetsPath, "music.json");

        [DefaultValue(1.0f)] public float Speed { get; set; } = 1.0f;
        public float AudioDeviation { get; set; } = 0f;
        [DefaultValue(GameMode.Auto)] public GameMode Mode { get; set; } = GameMode.Auto;

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static MusicSetting GameLoad()
        {
            string json = Resources.Load<TextAsset>(gamePath).text;
            return JsonConvert.DeserializeObject<MusicSetting>(json);
        }

        public void EditorSave()
        {
            Save(editorPath);
        }

        public static MusicSetting EditorLoad()
        {
            if (!File.Exists(editorPath)) new MusicSetting().EditorSave();
            return JsonConvert.DeserializeObject<MusicSetting>(File.ReadAllText(editorPath));
        }

    }
}
