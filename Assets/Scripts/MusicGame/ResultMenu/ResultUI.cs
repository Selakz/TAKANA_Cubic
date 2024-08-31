using Takana3.MusicGame.LevelSelect;
using Takana3.Settings;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] TMP_Text MaxComboText;

    // Private
    private ResultInfo resultInfo;

    // Static

    // Defined Functions
    public void SetData()
    {
        MaxComboText.text = resultInfo.MaxCombo.ToString();
    }

    public void Restart(GameMode gameMode)
    {
        LevelInfo levelInfo = resultInfo.LevelInfo;
        levelInfo.MusicSetting.Mode = gameMode;
        InfoReader.SetInfo(levelInfo);
        SceneLoader.LoadScene("Playfield", "Shutter");
    }

    // System Functions
    void Start()
    {
        resultInfo = InfoReader.ReadInfo<ResultInfo>();
        SetData();
    }
}
