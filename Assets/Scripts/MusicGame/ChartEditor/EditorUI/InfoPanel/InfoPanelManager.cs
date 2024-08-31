using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private SelectFolderButton selectFolderButton;
    [SerializeField] private Button saveProjectButton;
    [SerializeField] private TMP_InputField musicNameInputField;
    [SerializeField] private TMP_InputField composerNameInputField;
    [SerializeField] private TMP_InputField charterNameInputField;
    [SerializeField] private TMP_InputField illustNameInputField;
    [SerializeField] private TMP_InputField bpmInputField;
    [SerializeField] private List<DifficultyBar> difficulties;

    public static InfoPanelManager Instance => _instance;

    // Private

    // Static
    private static InfoPanelManager _instance;

    // Defined Functions
    public void ReadInfo()
    {
        var songInfo = EditingLevelManager.Instance.SongInfo;
        var difficulty = EditingLevelManager.Instance.SingleSetting.Difficulty;
        musicNameInputField.text = songInfo.title.Get();
        composerNameInputField.text = songInfo.composer.Get();
        charterNameInputField.text = songInfo.GetDiffInfo(difficulty).chart;
        illustNameInputField.text = songInfo.GetDiffInfo(difficulty).illust;
        bpmInputField.text = songInfo.bpmDisplay;
        foreach (var diff in difficulties)
        {
            diff.Enable();
            diff.Rating = songInfo.GetDiffInfo(diff.Difficulty).ratingDisplay;
            if (diff.Difficulty == difficulty) diff.Select();
            else diff.UnSelect();
        }
        saveProjectButton.interactable = true;
    }

    public void SaveInfoToLevelManager()
    {
        var songInfo = EditingLevelManager.Instance.SongInfo;
        var difficulty = EditingLevelManager.Instance.SingleSetting.Difficulty;
        songInfo.title.en = musicNameInputField.text;
        songInfo.composer.en = composerNameInputField.text;
        songInfo.GetDiffInfo(difficulty).chart = charterNameInputField.text;
        songInfo.GetDiffInfo(difficulty).illust = illustNameInputField.text;
        songInfo.bpmDisplay = bpmInputField.text;
        foreach (var diff in difficulties)
        {
            songInfo.GetDiffInfo(diff.Difficulty).ratingDisplay = diff.Rating;
        }
    }

    public void ChangeDifficulty(int difficulty)
    {
        if (difficulty == EditingLevelManager.Instance.SingleSetting.Difficulty) return;
        else
        {
            EditingLevelManager.Instance.SaveProject();
            EditingLevelManager.Instance.SingleSetting.Difficulty = difficulty;
            EditingLevelManager.Instance.SingleSetting.Save(Path.Combine(EditingLevelManager.Instance.LevelPath, "Takana_Data", "setting.json"));
            selectFolderButton.LoadProject(EditingLevelManager.Instance.LevelPath, difficulty);
        }
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {

    }

    void OnEnable()
    {
        ReadInfo();
    }

    void Update()
    {

    }
}
