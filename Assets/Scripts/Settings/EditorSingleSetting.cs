using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Takana3.Settings
{
    public class EditorSingleSetting
    {
        public int Difficulty { get; set; } = 3;
        public int TGridLineCount { get; set; } = 4;
        public float XGridInterval { get; set; } = 1.5f;
        public float XGridOffset { get; set; } = 0f;
        public List<(float time, float bpm)> BpmList { get; set; } = new();

        public static EditorSingleSetting Load(string path)
        {
            return JsonConvert.DeserializeObject<EditorSingleSetting>(File.ReadAllText(path));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
