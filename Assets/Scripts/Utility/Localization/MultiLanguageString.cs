using System.ComponentModel;
using static IMultiLanguage;

public class MultiLanguageString : IMultiLanguage
{
    // 虽然说属性开头要大写但是这是要写入json的所以算了吧
    public string en { get; set; } = "<missing>";
    [DefaultValue(null)] public string zh_s { get; set; } = null;
    [DefaultValue(null)] public string zh_t { get; set; } = null;

    public string Get()
    {
        int language = 0;
        return language switch
        {
            (int)LanguageLabel.en => en,
            (int)LanguageLabel.zh_s => zh_s ?? en,
            (int)LanguageLabel.zh_t => zh_t ?? en,
            _ => en
        };
    }
}
