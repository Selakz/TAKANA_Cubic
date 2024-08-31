using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    // Serializable and Public
    public TMP_Text countdown;
    // Private
    // Static
    // Defined Function
    IEnumerator Run()
    {
        countdown.text = "3";
        yield return new WaitForSeconds(1);
        countdown.text = "2";
        yield return new WaitForSeconds(1);
        countdown.text = "1";
        yield return new WaitForSeconds(1);
        EventManager.Trigger(EventManager.EventName.Resume);
        countdown.gameObject.SetActive(false);
    }
    // System Function
    void OnEnable()
    {
        StartCoroutine(Run());
    }
}
