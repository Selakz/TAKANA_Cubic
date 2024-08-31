using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DifficultyBar : MonoBehaviour, IPointerClickHandler, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private int difficulty;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_InputField ratingInputField;

    public int Difficulty => difficulty;
    public string Rating { get => ratingInputField.text; set => ratingInputField.text = value; }

    public void Enable()
    {
        ratingInputField.interactable = true;
        _isEnabled = true;
    }

    // Private
    private bool _isEnabled = false;

    // Static

    // Defined Functions
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isEnabled) InfoPanelManager.Instance.ChangeDifficulty(Difficulty);
    }

    public void Select()
    {
        difficultyText.fontStyle = FontStyles.Bold;
    }

    public void UnSelect()
    {
        difficultyText.fontStyle = FontStyles.Normal;
    }

    // System Functions
    void Start()
    {

    }

    void Update()
    {

    }
}
