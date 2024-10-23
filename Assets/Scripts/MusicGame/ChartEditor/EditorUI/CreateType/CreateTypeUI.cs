using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreateTypeUI : MonoBehaviour, IPointerClickHandler, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private Image background;
    [SerializeField] private NoteType createType;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            background.color = value ? selectedColor : defaultColor;
            _isSelected = value;
        }
    }

    public void Enable()
    {
        if (InScreenEditManager.Instance.CreateType == createType) IsSelected = true;
        _isEnable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isEnable) CreateTypeManager.Instance.Select(createType);
    }

    // Private
    private bool _isEnable = false;
    private bool _isSelected = false;

    // Static
    private static Color defaultColor = new(0.85f, 0.85f, 0.85f);
    private static Color selectedColor = new(0.5f, 0.5f, 1f);

    // Defined Functions

    // System Functions
}
