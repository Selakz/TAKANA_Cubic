using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static Takana3.MusicGame.Values;

public class TrackLineManager : MonoBehaviour
{
	// Serializable and Public
	[SerializeField] Transform indicator;
	[SerializeField] Transform pointsParent;
	[SerializeField] GameObject headerCurveTextObject;
	[SerializeField] TMP_Text currentCurveText;

	public static TrackLineManager Instance => _instance;
	public Track Track { get; private set; }

	public float GamePos
	{
		get => Camera.main.W2GPosX(transform.localPosition.x);
		private set => transform.localPosition = new(0, Camera.main.G2WPosY(value));
	}

	public int CurrentEaseId { get; private set; } = 1;

	// 以下两个属性，只是用于在重新渲染的时候决定要选中哪个点
	public BaseTrackMoveList SelectedMoveList { get; set; }

	public (float time, float x, string curve) SelectedItem { get; set; }

	// Private
	private float Current => TimeProvider.Instance.ChartTime;

	private BaseNoteMoveList moveList;
	private readonly List<(float time, float x, string curve)> selectedLeftItems = new();
	private readonly List<(float time, float x, string curve)> selectedRightItems = new();
	private readonly List<ICommand> turningPointCommands = new();
	private bool isCheckPressing = false;

	// Static
	private static TrackLineManager _instance;

	public static Dictionary<InputAction, int> ActionToEaseId { get; private set; }
	public static Dictionary<int, string> EaseIdToName { get; private set; }

	// Defined Functions
	public void Decorate(Track track)
	{
		Track = track;
		if (track == null) return;
		moveList = new(track.TimeInstantiate);
		moveList.FixRaw(EditingLevelManager.Instance.MusicSetting.Speed);
		SpriteInit();
		for (int i = 0; i < pointsParent.childCount; i++)
		{
			Destroy(pointsParent.GetChild(i).gameObject);
		}

		for (int i = 0; i < track.LMoveList.Count; i++)
		{
			var point = TurningPoint.DirectInstantiate(track.LMoveList, i, pointsParent);
			if (selectedLeftItems.Contains(track.LMoveList[i])) point.IsSelected = true;
		}

		for (int i = 0; i < track.RMoveList.Count; i++)
		{
			var point = TurningPoint.DirectInstantiate(track.RMoveList, i, pointsParent);
			if (selectedRightItems.Contains(track.RMoveList[i])) point.IsSelected = true;
		}
	}

	public void Clear()
	{
		indicator.gameObject.SetActive(false);
		for (int i = 0; i < pointsParent.childCount; i++)
		{
			Destroy(pointsParent.GetChild(i).gameObject);
		}

		Track = null;
		selectedLeftItems.Clear();
		selectedRightItems.Clear();
		SelectedMoveList = null;
	}

	public void AddSelectedItem(BaseTrackMoveList moveList, (float time, float x, string curve) item)
	{
		if (moveList == Track.LMoveList && !selectedLeftItems.Contains(item)) selectedLeftItems.Add(item);
		else if (moveList == Track.RMoveList && !selectedRightItems.Contains(item)) selectedRightItems.Add(item);
	}

	public void RemoveSelectedItem(BaseTrackMoveList moveList, (float time, float x, string curve) item)
	{
		if (moveList == Track.LMoveList) selectedLeftItems.Remove(item);
		else if (moveList == Track.RMoveList) selectedRightItems.Remove(item);
	}

	/// <summary> 将TurningPoint中触发的指令集中起来，便于作为BatchCommand执行 </summary>
	public void AddCommand(ICommand command) => turningPointCommands.Add(command);

	private void SpriteInit()
	{
		float left = Track.GetX(Track.TimeInstantiate, true), right = Track.GetX(Track.TimeInstantiate, false);
		indicator.gameObject.SetActive(true);
		indicator.localPosition = new(Camera.main.G2WPosX((left + right) / 2), indicator.localPosition.y);
		indicator.localScale = new(Camera.main.G2WPosX(Mathf.Abs(left - right)), Camera.main.G2WPosY(0.3f));
	}

	public void UpdatePos()
	{
		GamePos = moveList.GetPos(Current).y;
	}

	// System Functions
	void Awake()
	{
		_instance = this;
		CurrentEaseId = EditingLevelManager.Instance.GlobalSetting.DefaultCurveSeries;
		ActionToEaseId = new()
		{
			{ InputManager.Instance.CurveSwitch.SwitchToSine, 1 },
			{ InputManager.Instance.CurveSwitch.SwitchToQuad, 2 },
			{ InputManager.Instance.CurveSwitch.SwitchToCubic, 3 },
			{ InputManager.Instance.CurveSwitch.SwitchToQuart, 4 },
			{ InputManager.Instance.CurveSwitch.SwitchToQuint, 5 },
			{ InputManager.Instance.CurveSwitch.SwitchToExpo, 6 },
			{ InputManager.Instance.CurveSwitch.SwitchToCirc, 7 },
			{ InputManager.Instance.CurveSwitch.SwitchToBack, 8 },
			{ InputManager.Instance.CurveSwitch.SwitchToElastic, 9 },
			{ InputManager.Instance.CurveSwitch.SwitchToBounce, 0 },
		};
		EaseIdToName = new()
		{
			{ 1, "Sine" },
			{ 2, "Quad" },
			{ 3, "Cubic" },
			{ 4, "Quart" },
			{ 5, "Quint" },
			{ 6, "Expo" },
			{ 7, "Circ" },
			{ 8, "Back" },
			{ 9, "Elastic" },
			{ 0, "Bounce" },
		};
	}

	void Update()
	{
		if (turningPointCommands.Count > 0)
		{
			CommandManager.Instance.Add(new BatchCommand(turningPointCommands.ToArray(), "批量修改结点信息"));
			turningPointCommands.Clear();
		}

		if (Track != null)
		{
			UpdatePos();
			if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.CreateTurningPoint))
			{
				var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
				float baseX = gamePoint.x;
				if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
				float baseTime = GameYToTime(Current, EditingLevelManager.Instance.MusicSetting.Speed, gamePoint.y);
				if (baseTime < Track.TimeInstantiate || baseTime > Track.TimeEnd)
				{
					HeaderMessage.Show("添加失败：目标时间在轨道时间范围外", HeaderMessage.MessageType.Warn);
					return;
				}

				float leftX = Track.GetX(baseTime, true), rightX = Track.GetX(baseTime, false);
				bool isLeft = baseX < (leftX + rightX) / 2;
				(float time, float x, string curve) newItem = (baseTime,
					GridManager.Instance.IsXGridShow ? gamePoint.x : baseX, "u");
				CommandManager.Instance.Add(new MoveListInsertCommand(isLeft ? Track.LMoveList : Track.RMoveList,
					newItem));
			}
			else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.Copy))
			{
				if (selectedLeftItems.Count > 0 || selectedRightItems.Count > 0)
				{
					CopyPasteManager.Instance.CopyTurningPoints(selectedLeftItems, selectedRightItems, Track);
				}
			}
			else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.Cut))
			{
				if (selectedLeftItems.Count > 0 || selectedRightItems.Count > 0)
				{
					CopyPasteManager.Instance.CopyTurningPoints(selectedLeftItems, selectedRightItems, Track);
					List<ICommand> commands = new();
					try
					{
						foreach (var leftItem in selectedLeftItems)
						{
							commands.Add(new MoveListDeleteCommand(Track.LMoveList, leftItem));
						}

						foreach (var rightItem in selectedRightItems)
						{
							commands.Add(new MoveListDeleteCommand(Track.RMoveList, rightItem));
						}
					}
					catch
					{
						HeaderMessage.Show("剪切失败", HeaderMessage.MessageType.Warn);
						return;
					}

					CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "剪切结点"));
				}
			}
		}

		if (InputManager.Instance.IsHotkeyActionPressing(InputManager.Instance.CurveSwitch.CheckCurve))
		{
			if (!isCheckPressing)
			{
				headerCurveTextObject.SetActive(true);
				currentCurveText.text = $"当前选中曲线族：{EaseIdToName[CurrentEaseId]}";
				isCheckPressing = true;
			}
		}
		else if (isCheckPressing)
		{
			headerCurveTextObject.SetActive(false);
			isCheckPressing = false;
		}

		foreach (var action in ActionToEaseId.Keys)
		{
			if (InputManager.Instance.IsHotkeyActionPressed(action))
			{
				CurrentEaseId = ActionToEaseId[action];
				HeaderMessage.Show($"切换至{EaseIdToName[CurrentEaseId]}类缓动曲线族", HeaderMessage.MessageType.Info);
				return;
			}
		}
	}
}