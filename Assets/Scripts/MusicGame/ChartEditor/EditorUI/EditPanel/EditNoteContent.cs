using NUnit;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 感觉有点不优雅的一个实现
public class EditNoteContent : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_InputField timeJudgeInputField;
    [SerializeField] private TMP_InputField timeEndInputField = null;

    public BaseNote Note { get; private set; }

    // Private
    private float judge;
    private float end;

    // Static
    private static List<GameObject> prefabs = null;

    // Defined Functions
    /// <summary> type是prefab的样式，目前0为轨道的Note列表下的样式，1为独立的样式 </summary>
    public static EditNoteContent DirectInstantiate(EditingNote note, int type, RectTransform parent)
    {
        GetPrefab();
        GameObject prefab = note switch
        {
            EditingTap => prefabs[type * 3],
            EditingSlide => prefabs[type * 3 + 1],
            EditingHold => prefabs[type * 3 + 2],
            _ => throw new System.Exception("Unhandled note type")
        };
        GameObject instance = Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        EditNoteContent ret = instance.GetComponent<EditNoteContent>();
        ret.Initialize(note.Note);
        return ret;

        static void GetPrefab() => prefabs ??= new()
        {
            MyResources.Load<GameObject>("Prefabs/EditorUI/EditInTrackTapContent"),
            MyResources.Load<GameObject>("Prefabs/EditorUI/EditInTrackSlideContent"),
            MyResources.Load<GameObject>("Prefabs/EditorUI/EditInTrackHoldContent"),
            MyResources.Load<GameObject>("Prefabs/EditorUI/EditTapContent"),
            MyResources.Load<GameObject>("Prefabs/EditorUI/EditSlideContent"),
            MyResources.Load<GameObject>("Prefabs/EditorUI/EditHoldContent"),
        };
    }

    public void Initialize(BaseNote note)
    {
        Note = note;
        title.text = $"ID: {note.Id}";
        judge = note.TimeJudge;
        timeJudgeInputField.text = Mathf.RoundToInt(note.TimeJudge * 1000).ToString();
        if (note is Hold hold && timeEndInputField != null)
        {
            end = hold.TimeEnd;
            timeEndInputField.text = Mathf.RoundToInt(hold.TimeEnd * 1000).ToString();
        }
    }

    public void UpdateNote()
    {
        ICommand command;
        switch (Note)
        {
            case Tap:
                Tap newTap = new(Note.Id, NoteType.Tap, judge, Note.BelongingTrack);
                command = new UpdateNoteCommand(Note, newTap);
                break;
            case Slide:
                Slide newSlide = new(Note.Id, NoteType.Slide, judge, Note.BelongingTrack);
                command = new UpdateNoteCommand(Note, newSlide);
                break;
            case Hold:
                Hold newHold = new(Note.Id, NoteType.Hold, judge, Note.BelongingTrack, end);
                command = new UpdateNoteCommand(Note, newHold);
                break;
            default:
                throw new System.Exception("Unhandled note type");
        }
        CommandManager.Instance.Add(command);
    }

    public void JumpToTrack() { } // TODO: jump to track

    public void OnUnselectPressed()
    {
        SelectManager.Instance.UnselectNote(Note.Id);
        EditPanelManager.Instance.AskForRender();
    }

    public void OnDeletePressed()
    {
        CommandManager.Instance.Add(new DeleteNoteCommand(Note));
        EditPanelManager.Instance.AskForRender();
    }

    public void OnJudgeEndEdit()
    {
        if (int.TryParse(timeJudgeInputField.text, out int newJudgeInt))
        {
            if (newJudgeInt == Mathf.RoundToInt(judge * 1000f)) return;
            float newJudge = newJudgeInt / 1000f;
            if (newJudge >= 0 && (Note is not Hold hold || newJudge < hold.TimeEnd)
             && newJudge <= Note.BelongingTrack.TimeEnd
             && newJudge >= Note.BelongingTrack.TimeInstantiate)
            {
                judge = newJudge;
                UpdateNote();
                return;
            }
        }
        HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
        timeJudgeInputField.text = Mathf.RoundToInt(judge * 1000f).ToString();
    }

    public void OnEndEndEdit()
    {
        if (int.TryParse(timeEndInputField.text, out int newEndInt))
        {
            if (newEndInt == Mathf.RoundToInt(end * 1000f)) return;
            float newEnd = newEndInt / 1000f;
            if (newEnd >= 0 && Note is Hold hold && newEnd > hold.TimeJudge
             && newEnd <= Note.BelongingTrack.TimeEnd
             && newEnd >= Note.BelongingTrack.TimeInstantiate)
            {
                end = newEnd;
                UpdateNote();
                return;
            }
        }
        HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
        timeEndInputField.text = Mathf.RoundToInt(end * 1000f).ToString();
    }
}
