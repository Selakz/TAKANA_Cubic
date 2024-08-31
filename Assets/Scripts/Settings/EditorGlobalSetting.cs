using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace Takana3.Settings
{
    public class EditorGlobalSetting
    {
        private static readonly string path = Path.Combine(Application.streamingAssetsPath, "global.json");

        public bool IsCopyToClipboardAllowed { get; set; } = true;

        public int DefaultCurveSeries { get; set; } = 0;

        public float TrackOpacity { get; set; } = 1.0f;

        public string LastOpenedPath { get; set; } = string.Empty;

        [DefaultValue(true)] public bool IsAutoSaveAllowed { get; set; } = true;

        /// <summary> Ϊ������ʱ�������б��� </summary>
        [DefaultValue(1)] public int AutoSaveInterval_Minute { get; set; } = 1;

        public static EditorGlobalSetting Load()
        {
            if (!File.Exists(path)) new EditorGlobalSetting().Save();
            return JsonConvert.DeserializeObject<EditorGlobalSetting>(File.ReadAllText(path));
        }

        public void Save()
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
