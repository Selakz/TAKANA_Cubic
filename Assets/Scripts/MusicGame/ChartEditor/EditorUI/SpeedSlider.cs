using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text speedText;

    public void Enable()
    {
        speedSlider.interactable = true;
        speedSlider.value = EditingLevelManager.Instance.MusicSetting.Speed * 10;
        speedText.text = EditingLevelManager.Instance.MusicSetting.Speed.ToString("0.0");
    }

    // Private

    // Static

    // Defined Functions
    public void SetSpeed()
    {
        speedText.text = (speedSlider.value / 10f).ToString("0.0");
        EditingLevelManager.Instance.MusicSetting.Speed = speedSlider.value / 10f;
        EditingLevelManager.Instance.AskForResetField();
    }

    // System Functions
    void Start()
    {

    }

    void Update()
    {

    }
}
