using System.ComponentModel;

namespace Takana3.MusicGame.LevelSelect
{
    public record DiffInfo
    {
        // ��Ȼ˵���Կ�ͷҪ��д��������Ҫд��json���������˰�
        public int difficulty { get; set; } = 3;
        public string chart { get; set; } = "<missing>";
        public string illust { get; set; } = "<missing>";
        [DefaultValue("00")] public string ratingDisplay { get; set; } = "00";
        public int rating { get; set; } = 0;
        public bool ratingPlus { get; set; } = false;
        [DefaultValue(0)] public int score { get; set; } = 0;
    }
}
