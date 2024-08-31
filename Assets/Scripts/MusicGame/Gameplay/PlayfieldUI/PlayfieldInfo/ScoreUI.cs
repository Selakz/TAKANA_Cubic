using System;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreUI : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] TMP_Text scoreText;
    // Private

    // Static

    // Defined Function
    void UpdateScore(object data)
    {
        double score = (double)data;
        scoreText.text = $"{Math.Round(score)}".PadLeft(7, '0');
    }

    void ScoreInit()
    {
        scoreText.text = $"{Math.Round(0.0)}".PadLeft(7, '0');
    }

    // System Function
    void Awake()
    {
        EventManager.AddListener(EventManager.EventName.UpdateScore, UpdateScore);
        EventManager.AddListener(EventManager.EventName.LevelInit, ScoreInit);
        ScoreInit();
    }
}
