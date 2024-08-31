using TMPro;
using UnityEngine;

public class OffsetInputField : MonoBehaviour, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private TMP_InputField inputField;

    public void Enable()
    {
        inputField.interactable = true;
        ReadOffset();
    }

    // Private
    private float offset = 0f;

    // Static

    // Defined Function
    public void SetOffset()
    {
        float newOffset = int.Parse(inputField.text) / 1000f;
        if (offset < 0)
        {
            HeaderMessage.Show("偏移应为非负值", HeaderMessage.MessageType.Warn);
            inputField.text = Mathf.RoundToInt(offset * 1000f).ToString();
        }
        TimeProvider.Instance.Offset = newOffset;
        EditingLevelManager.Instance.RawChartInfo.Offset = newOffset;
        EditingLevelManager.Instance.AskForResetField();
    }

    public void ReadOffset()
    {
        inputField.text = Mathf.RoundToInt(EditingLevelManager.Instance.RawChartInfo.Offset * 1000).ToString();
    }

    // System Function
}