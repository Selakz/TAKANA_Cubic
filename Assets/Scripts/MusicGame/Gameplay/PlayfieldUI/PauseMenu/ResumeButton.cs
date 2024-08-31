using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class ResumeButton : MonoBehaviour
{
    // Serializable and Public
    public GameObject pauseMenu;
    public TMP_Text countdown;
    // Private

    // Static
    // Defined Function
    public void OnClick()
    {
        pauseMenu.SetActive(false);
        countdown.gameObject.SetActive(true);
    }
    // System Function
}
