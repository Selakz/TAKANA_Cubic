using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using static Takana3.MusicGame.Values;

/// <summary>
/// ������Track���ͣ�ֻ����x�᷽�����ƶ������߽�ֱ�����˶�
/// </summary>
[System.Serializable]
public class Track : ITrack
{
	// Implement IComponent
	public int Id { get; }
	public bool IsInitialized { get; private set; } = false;
	public GameObject ThisObject { get; private set; } = null;
	public float TimeInstantiate { get; set; } = -TimePreAnimation;

	// Implement ITrack
	public TrackType Type { get; }
	public float TimeEnd { get; set; }
	public bool IsVisible { get; }
	public bool IsPreAnimate { get; }
	public bool IsPostAnimate { get; }

	// Self Properties
	public TrackController Controller { get; private set; } = null;
	public JudgeLine BelongingLine { get; }
	public MultiSortList<BaseNote> Notes { get; set; }
	public BaseTrackMoveList LMoveList { get; set; }
	public BaseTrackMoveList RMoveList { get; set; }

	public Track(int id, TrackType trackType, float timeStart, float timeEnd, float leftX, float rightX, bool isVisible,
		bool isPreAnimate, bool isPostAnimate, JudgeLine belongingLine)
	{
		Id = id;
		Type = trackType;
		TimeInstantiate = timeStart;
		TimeEnd = timeEnd;
		IsVisible = isVisible;
		IsPreAnimate = isPreAnimate;
		IsPostAnimate = isPostAnimate;
		BelongingLine = belongingLine;

		LMoveList = new(leftX, timeStart, timeEnd);
		RMoveList = new(rightX, timeStart, timeEnd);
		Notes = new();
	}

	public void Initialize(MusicSetting setting)
	{
		if (ThisObject != null) Object.Destroy(ThisObject);
		Notes.AddSort("ID", (BaseNote x, BaseNote y) => x.Id.CompareTo(y.Id));
		Notes.AddSort("Judge", (BaseNote x, BaseNote y) => x.TimeJudge.CompareTo(y.TimeJudge));
		IsInitialized = true;
	}

	public bool Instantiate()
	{
		Assert.IsTrue(IsInitialized);
		if (ThisObject != null) return true;
		ThisObject = Object.Instantiate(Type.GetPrefab(), BelongingLine.ThisObject.transform, false);
		Controller = ThisObject.GetComponent<TrackController>();
		Controller.InfoInit(this);
		return true;
	}

	/// <summary> ��õ�ǰʱ���Track����Ϸ����xֵ�����ڵ���<see cref="Initialize(MusicSetting)"/>֮��ʹ�� </summary>
	public float GetX(float current, bool isLeft)
	{
		Assert.IsTrue(IsInitialized);
		if (isLeft) return LMoveList.GetPos(current).x;
		else return RMoveList.GetPos(current).x;
	}

	public Track Clone(int id, float timeStart, float leftStart)
	{
		Track track = new(id++, Type, timeStart, timeStart + TimeEnd - TimeInstantiate, GetX(TimeEnd, true),
			GetX(TimeEnd, false), true, false, false, BelongingLine)
		{
			// ��¡�˶��б�
			LMoveList = LMoveList.Clone(timeStart, leftStart),
			RMoveList = RMoveList.Clone(timeStart,
				leftStart + GetX(TimeInstantiate, false) - GetX(TimeInstantiate, true))
		};
		// ��¡����ϵ�����Note
		foreach (var note in Notes)
		{
			track.Notes.AddItem(note.Clone(id++, note.TimeJudge + timeStart - TimeInstantiate, track));
		}

		// ����RawChartInfo��id
		if (EditingLevelManager.Instance != null)
		{
			try
			{
				EditingLevelManager.Instance.RawChartInfo.NewId = id;
			}
			catch
			{
				throw new System.Exception("�ڸ��ƹ��ʱ��������ȷ��id��ֵ���");
			}
		}

		return track;
	}
}