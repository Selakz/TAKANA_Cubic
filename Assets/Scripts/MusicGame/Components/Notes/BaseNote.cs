using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using static Takana3.MusicGame.Values;

/// <summary>
/// ����������͵�Note������ʽ��һ��������һ��Track�ϣ���TimeJudgeʱ�䵽��Ӧ�ж���
/// </summary>
public abstract class BaseNote : INote
{
    // Implement IComponent
    public int Id { get; }
    public bool IsInitialized { get; protected set; } = false;
    public GameObject ThisObject { get; protected set; } = null;
    public float TimeInstantiate { get; protected set; } = -TimePreAnimation;

    // Implement INote
    public NoteType Type { get; }
    public JudgeType JudgeType { get; }
    public float TimeJudge { get; }
    public JudgeInfo JudgeInfo { get; protected set; }

    // Self Properties
    public BaseNoteController Controller { get; protected set; } = null;
    public Track BelongingTrack { get; internal set; }
    public InputInfo InputInfo { get; set; } = null;

    // Private
    protected INoteMoveList moveList;

    // Defined Functions
    public BaseNote(int id, NoteType type, JudgeType judgeType, float timeJudge, Track belongingTrack, float speedRate = 1.0f)
    {
        Id = id;
        Type = type;
        JudgeType = judgeType;
        TimeJudge = timeJudge;
        BelongingTrack = belongingTrack;

        moveList = new BaseNoteMoveList(timeJudge, speedRate);
    }

    /// <summary> ���㲢�����ø�Note��speed�µ�<see cref="TimeInstantiate"/> </summary>
    internal abstract float CalcTimeInstantiate(float speed, bool isSet = false);

    public abstract void Initialize(MusicSetting setting);

    public abstract bool HandleInput(float timeInput);

    public abstract bool Instantiate();

    /// <summary> ��õ�ǰʱ���Note����Ϸ����yֵ�����ڵ���<see cref="Initialize(MusicSetting)"/>֮��ʹ�� </summary>
    public float GetY(float current)
    {
        Assert.IsTrue(IsInitialized);
        return moveList.GetPos(current).y;
    }

    public void SetMoveList(INoteMoveList moveList)
    {
        this.moveList = moveList;
    }

    public abstract InputInfo GetInput();

    public abstract BaseNote Clone(int id, float timeJudge, Track belongingTrack);
}
