using System;
using System.Collections.Generic;
using System.IO;
using Takana3.MusicGame.LevelSelect;
using Takana3.Settings;
using UnityEngine;

public class SelectFolderButton : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private List<GameObject> toSetActiveManagers;

    // Private

    // Static

    // Defined Functions
    public void SelectFolder()
    {
        int difficulty = 3;
        string folder = FileBrowser.OpenFolderDialog("选择文件夹");
        if (folder == null) return;
        Directory.CreateDirectory(Path.Combine(folder, "Takana_Data", "AutoSave"));
        if (File.Exists(Path.Combine(folder, "Takana_Data", "setting.json")))
        {
            var setting = EditorSingleSetting.Load(Path.Combine(folder, "Takana_Data", "setting.json"));
            difficulty = setting.Difficulty;
        }
        else
        {
            var setting = new EditorSingleSetting() { Difficulty = difficulty, BpmList = new() { (0, 100) } };
            setting.Save(Path.Combine(folder, "Takana_Data", "setting.json"));
        }
        LoadProject(folder, difficulty);
    }

    public void LoadProject(string folder, int difficulty)
    {
        try
        {
            if (!File.Exists(Path.Combine(folder, "music.mp3"))) throw new FileNotFoundException();
            if (!File.Exists(Path.Combine(folder, $"{difficulty}.dlf"))) File.WriteAllText(Path.Combine(folder, $"{difficulty}.dlf"), "-");
            InfoReader.SetInfo(new LevelInfo(folder, difficulty));
            foreach (GameObject manager in toSetActiveManagers) manager.SetActive(false);
            foreach (GameObject manager in toSetActiveManagers) manager.SetActive(true);
            HeaderMessage.Show("加载完成！", HeaderMessage.MessageType.Success);
        }
        catch (Exception ex)
        {
            HeaderMessage.Show("读取工程时发生错误，请重新检查", HeaderMessage.MessageType.Error);
            Debug.Log(ex);
        }
    }

    // System Functions
    void Start()
    {

    }

    void Update()
    {

    }
}
