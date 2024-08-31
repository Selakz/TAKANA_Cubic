using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicSpeedButton : MonoBehaviour, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private Button musicSpeedButton;
    [SerializeField] private TMP_Text speedText;

    public void Enable()
    {
        musicSpeedButton.interactable = true;
    }

    // Private
    private List<float> speeds = new() { 1.00f, 0.75f, 0.50f, 0.25f };
    private int speedIndex = 0;

    // Static

    // Defined Functions
    public void SwitchSpeed()
    {
        speedIndex = (speedIndex + 1) % speeds.Count;
        TimeProvider.Instance.PlaybackSpeed = speeds[speedIndex];
        speedText.text = $"{Mathf.RoundToInt(speeds[speedIndex] * 100)}%";
    }

    // System Functions
    void Start()
    {

    }

    void Update()
    {

    }
}
