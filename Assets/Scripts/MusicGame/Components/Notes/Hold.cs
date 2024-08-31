using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class Hold : BaseNote
{
    // Serializable and Public
    public float TimeEnd { get; } // Hold最终结束的时间

    [SerializeField] private GameObject holdPrefab;

    // Private
    private INoteMoveList scaleList;

    public Hold(int id, NoteType type, float timeJudge, Track belongingTrack, float timeEnd, float speedRate = 1.0f)
        : base(id, type, type.GetJudgeType(), timeJudge, belongingTrack, speedRate)
    {
        TimeEnd = timeEnd;
        scaleList = new BaseNoteMoveList(timeEnd, speedRate, timeJudge);
    }

    internal override float CalcTimeInstantiate(float speed, bool isSet)
    {
        // TODO: 有点问题。结合movelist和scalelist计算
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
            holdPrefab = Type.GetGameObject();
        }
        moveList.FixRaw(setting.Speed);
        scaleList.FixRaw(setting.Speed);
        CalcTimeInstantiate(setting.Speed, true);

        IsInitialized = true;

        TimeInstantiate = Mathf.Max(TimeInstantiate, BelongingTrack.TimeInstantiate + 0.0001f);

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

        ThisObject = Object.Instantiate(holdPrefab);
        Assert.IsNotNull(BelongingTrack.ThisObject);
        ThisObject.transform.SetParent(BelongingTrack.ThisObject.transform, false);
        Controller = ThisObject.GetComponent<HoldController>();
        Controller.InfoInit(this, InputInfo as HoldInputInfo);
        return true;
    }

    public float GetScale(float current)
    {
        Assert.IsTrue(IsInitialized);
        return scaleList.GetPos(current).y;
    }

    public void SetScaleList(INoteMoveList scaleList)
    {
        this.scaleList = scaleList;
    }

    public override InputInfo GetInput()
    {
        return new HoldInputInfo()
        {
            Note = this,
            TimeInput = TimeJudge,
            ReleaseTimes = new(),
        };
    }
}
