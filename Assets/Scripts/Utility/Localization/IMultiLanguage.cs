public interface IMultiLanguage
{
    public enum LanguageLabel
    {
        en = 0,
        zh_s = 1,
        zh_t = 2,
    }

    /// <summary>
    /// "Language" label in PlayerPrefs corresponding to the language player choose.<br/>
    /// 0: English; 1: Simplified Chinese; 2: Traditional Chinese.<br/>
    /// And can be more with the development...
    /// </summary>
    public string Get();
}
