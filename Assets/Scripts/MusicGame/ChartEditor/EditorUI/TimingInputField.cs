using TMPro;
using UnityEngine;

public class TimingInputField : MonoBehaviour, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private TMP_InputField inputField;

    public void Enable()
    {
        inputField.interactable = true;
    }

    // Private

    // Static

    // Defined Function

    // System Function
    void Start()
    {

    }

    void Update()
    {
        if (TimeProvider.Instance == null) inputField.text = "0";
        else inputField.text = Mathf.RoundToInt(TimeProvider.Instance.ChartTime * 1000).ToString();
    }
}
