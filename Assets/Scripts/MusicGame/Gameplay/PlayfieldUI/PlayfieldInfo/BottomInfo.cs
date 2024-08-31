using Takana3.MusicGame.LevelSelect;
using TMPro;
using UnityEngine;

public class BottomInfo : MonoBehaviour
{
    // Serializable and Public
    public TMP_Text songName;
    public TMP_Text difficulty;

    // Private

    // Static
    readonly string[] diffName = { "Special", "Basic", "Advanced", "Master", "Extra" };

    // Defined Function

    // System Function
    void Awake()
    {
        LevelInfo levelInfo = InfoReader.ReadInfo<LevelInfo>();
        songName.text = levelInfo.SongInfo.title.Get() + " - " + levelInfo.SongInfo.composer.Get();
        DiffInfo diff = levelInfo.SongInfo.GetDiffInfo(levelInfo.Difficulty);
        difficulty.text = diffName[diff.difficulty] + " Lv." + diff.rating;
    }
}
