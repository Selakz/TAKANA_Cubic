using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class Slide : BaseNote
{
    // Serializable and Public
    [SerializeField] private GameObject slidePrefab;

    // Private

    // Defined Functions
    public Slide(int id, NoteType type, float timeJudge, Track belongingTrack, float speedRate = 1.0f)
        : base(id, type, type.GetJudgeType(), timeJudge, belongingTrack, speedRate) { }

    internal override float CalcTimeInstantiate(float speed, bool isSet = false)
    {
        float result = moveList.CalcTimeInstantiate(speed);
        if (isSet) TimeInstantiate = result;
        return result;
    }

    public override void Initialize(MusicSetting setting)
    {
        if (ThisObject != null) Object.Destroy(ThisObject);
        else
        {
            JudgeInfo = Type.GetJudgeInfo();
            slidePrefab = Type.GetGameObject();
        }
        CalcTimeInstantiate(setting.Speed, true);
        moveList.FixRaw(setting.Speed);
        IsInitialized = true;

        if (NoteType.USlide.UNoteTypes().Contains(Type)) BelongingTrack.TimeInstantiate = TimeInstantiate - 0.0001f;
        else TimeInstantiate = Mathf.Max(TimeInstantiate, BelongingTrack.TimeInstantiate + 0.0001f);

        InputInfo = setting.Mode switch
        {
            GameMode.Common => null,
            GameMode.Auto => GetInput(),
            _ => InputInfo,
        };
    }

    public override bool HandleInput(float timeInput)
    {
        if (Controller == null) return false;
        return Controller.HandleInput(timeInput);
    }

    public override bool Instantiate()
    {
        Assert.IsTrue(IsInitialized);
        if (ThisObject != null) return true;

        ThisObject = Object.Instantiate(slidePrefab);
        Assert.IsNotNull(BelongingTrack.ThisObject);
        ThisObject.transform.SetParent(BelongingTrack.ThisObject.transform, false);
        Controller = ThisObject.GetComponent<SlideController>();
        Controller.InfoInit(this, InputInfo);
        return true;
    }

    public override InputInfo GetInput()
    {
        return new InputInfo()
        {
            Note = this,
            TimeInput = TimeJudge,
        };
    }
}
