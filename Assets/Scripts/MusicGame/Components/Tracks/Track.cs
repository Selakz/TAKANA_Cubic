using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using static Takana3.MusicGame.Values;

/// <summary>
/// 基本的Track类型：只能在x轴方向上移动，两边界分别独立运动
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

	/// <summary> 获得当前时间该Track的游戏坐标x值；需在调用<see cref="Initialize(MusicSetting)"/>之后使用 </summary>
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
			// 克隆运动列表
			LMoveList = LMoveList.Clone(timeStart, leftStart),
			RMoveList = RMoveList.Clone(timeStart,
				leftStart + GetX(TimeInstantiate, false) - GetX(TimeInstantiate, true))
		};
		// 克隆轨道上的所有Note
		foreach (var note in Notes)
		{
			track.Notes.AddItem(note.Clone(id++, note.TimeJudge + timeStart - TimeInstantiate, track));
		}

		// 调整RawChartInfo的id
		if (EditingLevelManager.Instance != null)
		{
			try
			{
				EditingLevelManager.Instance.RawChartInfo.NewId = id;
			}
			catch
			{
				throw new System.Exception("在复制轨道时发生不正确的id赋值情况");
			}
		}

		return track;
	}
}