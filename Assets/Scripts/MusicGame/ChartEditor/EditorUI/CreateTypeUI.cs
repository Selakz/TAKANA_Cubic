using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreateTypeUI : MonoBehaviour, IPointerClickHandler, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private Image background;
    [SerializeField] private List<CreateTypeUI> others;
    [SerializeField] private NoteType createType;

    public void Enable()
    {
        if (InScreenEditManager.Instance.CreateType == createType) Select();
        else UnSelect();
        _isEnable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isEnable)
        {
            Select();
        }
    }

    // Private
    private bool _isEnable = false;

    // Static
    private static Color defaultColor = new(0.85f, 0.85f, 0.85f);
    private static Color selectedColor = new(0.5f, 0.5f, 1f);

    // Defined Functions
    public void Select()
    {
        InScreenEditManager.Instance.CreateType = createType;
        background.color = selectedColor;
        foreach (CreateTypeUI ui in others) ui.UnSelect();
    }

    public void UnSelect()
    {
        background.color = defaultColor;
    }

    // System Functions
    void Update()
    {
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.SwitchToTap))
        {
            if (createType == NoteType.Tap) Select();
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.SwitchToSlide))
        {
            if (createType == NoteType.Slide) Select();
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.SwitchToHold))
        {
            if (createType == NoteType.Hold) Select();
        }
    }
}
