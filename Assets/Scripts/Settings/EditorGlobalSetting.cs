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

        [DefaultValue(false)] public bool IsForcePauseAllowed { get; set; } = false;

        [DefaultValue(false)] public bool IsInitialTrackLengthNotToEnd { get; set; } = false;

        /// <summary> Ϊ������ʱ���Ƚ��Զ�������ĩβ���� </summary>
        [DefaultValue(3000)] public int InitialTrackLength_Ms { get; set; } = 3000;

        /// <summary> �����Ƿ� </summary>
        [DefaultValue(false)] public bool IsReverseCurveName { get; set; } = false;

        [DefaultValue("1920x1080")] public string Resolution { get; set; }

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
