using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectModeUI : MonoBehaviour, IPointerClickHandler, ICanEnableUI
{
    // Serializable and Public
    [SerializeField] private Image background;
    [SerializeField] private List<SelectModeUI> others;
    [SerializeField] private SelectMode selectMode;

    public void Enable()
    {
        if (SelectManager.Instance.SelectMode == selectMode) Select();
        else UnSelect();
        _isEnable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isEnable) Select();
    }

    // Private
    private bool _isEnable = false;

    // Static
    private static Color defaultColor = Color.white;
    private static Color selectedColor = new(0.5f, 0.5f, 1f);

    // Defined Functions
    public void Select()
    {
        SelectManager.Instance.SelectMode = selectMode;
        background.color = selectedColor;
        foreach (var ui in others) ui.UnSelect();
    }

    public void UnSelect()
    {
        background.color = defaultColor;
    }

    // System Functions
    void Update()
    {
        //if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToggleSelectMode))
        //{
        //    if (selectMode != SelectManager.Instance.SelectMode)
        //    {
        //        Select();
        //    }
        //}
    }
}
