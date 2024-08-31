using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] TMP_Text comboText;
    // Private

    // Static

    // Defined Function
    void UpdateCombo(object data)
    {
        int combo = (int)data;
        comboText.text = $"{combo}";
    }

    void ComboInit()
    {
        comboText.text = "0";
    }

    // System Function
    void Awake()
    {
        EventManager.AddListener(EventManager.EventName.UpdateCombo, UpdateCombo);
        EventManager.AddListener(EventManager.EventName.LevelInit, ComboInit);
        ComboInit();
    }
}
