using System.Windows.Forms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimingBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image image;
    [SerializeField] private TGridController grid;

    //private bool isEntered = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicked");
        if (EditingLevelManager.Instance.GlobalSetting.IsCopyToClipboardAllowed)
        {
            Clipboard.SetText(Mathf.RoundToInt(grid.Time * 1000).ToString());
        }
        if (DeselectInputFieldGet.DeselectedInputField != null)
        {
            DeselectInputFieldGet.DeselectedInputField.text = Mathf.RoundToInt(grid.Time * 1000).ToString();
            DeselectInputFieldGet.DeselectedInputField.onEndEdit.Invoke(DeselectInputFieldGet.DeselectedInputField.text);
            Debug.Log("content set");
        }
        else Debug.Log("null");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //isEntered = true;
        transform.localScale = new(1, 1.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //isEntered = false;
        transform.localScale = new(1, 1);
    }
}
