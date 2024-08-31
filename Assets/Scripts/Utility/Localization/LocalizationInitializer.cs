using TMPro;
using UnityEngine;

/// <summary>
/// Set the fileName and label in Inspector, and the text this component attaches to will
/// automatically update to the corresponding string.<br/> You can also use UpdateText() manually to
/// update this text or trigger EventManager.UpdateAllText() to update all texts in the scene 
/// if needed (e.g. To support changing language during the game)
/// </summary>
public class LocalizationInitializer : MonoBehaviour
{
    // Serializable and Public
    public TMP_Text tmp_text;
    public string fileName = "UIText";
    public string label = "default";

    // Private

    // Static

    // Defined Function
    public void UpdateText()
    {
        tmp_text.text = TextReader.Get(fileName, label).Get();
    }

    // System Function
    void Awake()
    {
        EventManager.AddListener(EventManager.EventName.UpdateText, UpdateText);
        UpdateText();
    }
}
