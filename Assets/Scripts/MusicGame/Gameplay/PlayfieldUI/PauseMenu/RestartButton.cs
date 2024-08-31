using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    // Serializable and Public
    public GameObject pauseMenu;
    // Private
    // Static
    // Defined Funtion
    public void OnClick()
    {
        EventManager.Trigger(EventManager.EventName.LevelInit);
        pauseMenu.SetActive(false);
    }
    // System Funtion
}
