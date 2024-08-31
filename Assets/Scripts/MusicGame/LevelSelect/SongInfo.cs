using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Takana3.MusicGame.LevelSelect
{
    /// <summary>
    /// 关于一首歌的全部信息
    /// </summary>
    public record SongInfo : IPassInfo
    {
        public int idx { get; set; } = 0;
        public string id { get; set; } = "Default";
        public MultiLanguageString title { get; set; } = new();
        public MultiLanguageString composer { get; set; } = new();
        public string bpmDisplay { get; set; } = string.Empty;
        public int bpmDefault { get; set; }
        public string set { get; set; } = "base";
        public MultiLanguageString description { get; set; } = new();
        public List<DiffInfo> difficulties { get; set; } = new();

        public DiffInfo GetDiffInfo(int difficulty)
        {
            foreach (var singleDiff in difficulties)
            {
                if (singleDiff.difficulty == difficulty)
                {
                    return singleDiff;
                }
            }
            return new DiffInfo();
        }

        public static SongInfo Load(string path)
        {
            return JsonConvert.DeserializeObject<SongInfo>(File.ReadAllText(path));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
        }
    }
}
