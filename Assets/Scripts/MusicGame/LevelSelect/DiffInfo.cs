using System.ComponentModel;

namespace Takana3.MusicGame.LevelSelect
{
    public record DiffInfo
    {
        // 虽然说属性开头要大写但是这是要写入json的所以算了吧
        public int difficulty { get; set; } = 3;
        public string chart { get; set; } = "<missing>";
        public string illust { get; set; } = "<missing>";
        [DefaultValue("00")] public string ratingDisplay { get; set; } = "00";
        public int rating { get; set; } = 0;
        public bool ratingPlus { get; set; } = false;
        [DefaultValue(0)] public int score { get; set; } = 0;
    }
}
