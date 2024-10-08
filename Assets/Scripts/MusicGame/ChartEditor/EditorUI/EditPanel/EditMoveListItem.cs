using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditMoveListItem : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private TMP_InputField timeInputField;
    [SerializeField] private TMP_InputField xInputField;
    [SerializeField] private TMP_InputField curveInputField;
    [SerializeField] private Button addButton;
    [SerializeField] private Button removeButton;

    public (float time, float x, string curve) Item => (time, x, curve);

    // Private
    private bool IsStart => index == 0;
    private bool IsEnd => index == moveList.Count - 1;

    private BaseTrackMoveList moveList;
    private int index;
    private float time;
    private float x;
    private string curve;

    // Static
    private const string prefabPath = "Prefabs/EditorUI/EditMoveListItem";
    private static GameObject prefab = null;

    // Defined Functions
    public static EditMoveListItem DirectInstantiate(BaseTrackMoveList moveList, int index, RectTransform parent)
    {
        GetPrefab();
        GameObject instance = Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        EditMoveListItem ret = instance.GetComponent<EditMoveListItem>();
        ret.Initialize(moveList, index);
        return ret;

        static void GetPrefab() { if (prefab == null) prefab = MyResources.Load<GameObject>(prefabPath); }
    }

    public void Initialize(BaseTrackMoveList moveList, int index)
    {
        this.moveList = moveList;
        this.index = index;
        (time, x, curve) = moveList[index];
        timeInputField.text = Mathf.RoundToInt(time * 1000).ToString();
        xInputField.text = x.ToString("0.00");
        curveInputField.text =
            EditingLevelManager.Instance.GlobalSetting.IsReverseCurveName ?
            CurveCalculator.GetEaseByName(curve).Opposite().GetString() : curve;
        if (IsStart)
        {
            timeInputField.interactable = false;
            removeButton.interactable = false;
        }
        if (IsEnd)
        {
            timeInputField.interactable = false;
            curveInputField.interactable = false;
            addButton.interactable = false;
            removeButton.interactable = false;
        }
    }

    public void OnAddPressed()
    {
        CommandManager.Instance.Add(new MoveListInsertCommand(moveList, Item));
    }

    public void OnRemovePressed()
    {
        CommandManager.Instance.Add(new MoveListDeleteCommand(moveList, Item));
    }

    public void OnTimeEndEdit()
    {
        if (int.TryParse(timeInputField.text, out int newTimeInt))
        {
            if (newTimeInt == Mathf.RoundToInt(time * 1000f)) return;
            else
            {
                float newTime = newTimeInt / 1000f;
                try
                {
                    (float time, float x, string curve) updated = (newTime, x, curve);
                    var command = new MoveListUpdateCommand(moveList, Item, updated);
                    CommandManager.Instance.Add(command);
                    return;
                }
                catch (Exception)
                {
                    HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
                    timeInputField.text = Mathf.RoundToInt(time * 1000f).ToString();
                }
            }
        }
        else
        {
            HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
            timeInputField.text = Mathf.RoundToInt(time * 1000f).ToString();
        }
    }

    public void OnXEndEdit()
    {
        if (float.TryParse(xInputField.text, out float newX))
        {
            if (Mathf.RoundToInt(newX * 100) == Mathf.RoundToInt(x * 100)) return;
            else
            {
                try
                {
                    (float time, float x, string curve) updated = (time, newX, curve);
                    var command = new MoveListUpdateCommand(moveList, Item, updated);
                    CommandManager.Instance.Add(command);
                    return;
                }
                catch (Exception)
                {
                    HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
                    xInputField.text = x.ToString("0.00");
                }
            }
        }
        else
        {
            HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
            xInputField.text = x.ToString("0.00");
        }
    }

    public void OnCurveEndEdit()
    {
        if (int.TryParse(curveInputField.text, out var curInt))
        {
            try
            {
                Eases ease;
                if (curInt >= 100) ease = CurveCalculator.GetEaseByRpeNumber(curInt - 100);
                else ease = CurveCalculator.GetEaseById(curInt);
                curveInputField.text = ease.GetString();
                if (EditingLevelManager.Instance.GlobalSetting.IsReverseCurveName) ease = ease.Opposite();
                (float time, float x, string curve) updated = (time, x, ease.GetString());
                var command = new MoveListUpdateCommand(moveList, Item, updated);
                CommandManager.Instance.Add(command);
                return;
            }
            catch
            {
                HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
                curveInputField.text =
                    EditingLevelManager.Instance.GlobalSetting.IsReverseCurveName ?
                    CurveCalculator.GetEaseByName(curve).Opposite().GetString() : curve;
            }
        }
        else
        {
            try
            {
                var ease = CurveCalculator.GetEaseByName(curveInputField.text);
                if (EditingLevelManager.Instance.GlobalSetting.IsReverseCurveName) ease = ease.Opposite();
                (float time, float x, string curve) updated = (time, x, ease.GetString());
                var command = new MoveListUpdateCommand(moveList, Item, updated);
                CommandManager.Instance.Add(command);
                return;
            }
            catch
            {
                HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
                curveInputField.text =
                    EditingLevelManager.Instance.GlobalSetting.IsReverseCurveName ?
                    CurveCalculator.GetEaseByName(curve).Opposite().GetString() : curve;
            }
        }
    }
}
