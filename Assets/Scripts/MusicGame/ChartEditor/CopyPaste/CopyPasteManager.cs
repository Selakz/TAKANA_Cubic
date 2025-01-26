using System.Collections.Generic;
using UnityEngine;
using static Takana3.MusicGame.Values;

public class CopyPasteManager : MonoBehaviour
{
	// Serializable and Public
	public static CopyPasteManager Instance => _instance;

	// Private
	private List<BaseNote> templateBaseNotes = new();
	private List<Track> templateTracks = new();
	private List<(float time, float x, string curve)> templateLeftPoints = new();
	private List<(float time, float x, string curve)> templateRightPoints = new();
	private Track originalTrack = null;

	// Static
	private static CopyPasteManager _instance;

	// Defined Functions
	public void Clear()
	{
		templateBaseNotes.Clear();
		templateTracks.Clear();
		templateLeftPoints.Clear();
		templateRightPoints.Clear();
		originalTrack = null;
	}

	public void CopyBaseNotes(List<BaseNote> baseNotes)
	{
		Clear();
		templateBaseNotes = new(baseNotes);
		HeaderMessage.Show($"���Ƴɹ����Ѹ���{templateBaseNotes.Count}��Note", HeaderMessage.MessageType.Success);
	}

	public void CopyTracks(List<Track> tracks)
	{
		Clear();
		templateTracks = new(tracks);
		HeaderMessage.Show($"���Ƴɹ����Ѹ���{templateTracks.Count}�����", HeaderMessage.MessageType.Success);
	}

	public void CopyTurningPoints(List<(float time, float x, string curve)> leftPoints,
		List<(float time, float x, string curve)> rightPoints, Track fromTrack)
	{
		Clear();
		templateLeftPoints = new(leftPoints);
		templateRightPoints = new(rightPoints);
		originalTrack = fromTrack;
		HeaderMessage.Show($"���Ƴɹ����Ѹ���{templateLeftPoints.Count}�����㣬{templateRightPoints.Count}���ҽ��",
			HeaderMessage.MessageType.Success);
	}

	public void PasteBaseNotes(float time)
	{
		if (templateBaseNotes.Count == 0) return;
		if (SelectManager.Instance.SelectedTracks.Count != 1)
		{
			HeaderMessage.Show("��ѡ��һ������Խ���Note��ͨճ��", HeaderMessage.MessageType.Info);
			return;
		}

		Track track = SelectManager.Instance.SelectedTracks[0].Track;
		templateBaseNotes.Sort((a, b) => a.TimeJudge.CompareTo(b.TimeJudge));
		float baseTime = templateBaseNotes[0].TimeJudge;
		List<ICommand> commands = new();
		try
		{
			foreach (BaseNote note in templateBaseNotes)
			{
				var command = new CreateNoteCommand(
					note.Clone(EditingLevelManager.Instance.RawChartInfo.NewId, time + note.TimeJudge - baseTime,
						track));
				commands.Add(command);
			}
		}
		catch
		{
			HeaderMessage.Show("ճ��Ƭ�γ��ȳ������ʱ�䷶Χ", HeaderMessage.MessageType.Warn);
			return;
		}

		CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "ճ��Note"));
		HeaderMessage.Show("ճ���ɹ���", HeaderMessage.MessageType.Success);
	}

	public void PasteTracks(float time, float left)
	{
		if (templateTracks.Count == 0) return;
		templateTracks.Sort((a, b) => a.TimeInstantiate.CompareTo(b.TimeInstantiate));
		float baseTime = templateTracks[0].TimeInstantiate;
		float baseLeft = templateTracks[0].GetX(templateTracks[0].TimeInstantiate, true);
		List<ICommand> commands = new();
		int if1ThenTheId = -1;
		try
		{
			foreach (var track in templateTracks)
			{
				Track newTrack = track.Clone(EditingLevelManager.Instance.RawChartInfo.NewId,
					time + track.TimeInstantiate - baseTime, left + track.GetX(track.TimeInstantiate, true) - baseLeft);
				var command = new CreateTrackCommand(newTrack);
				commands.Add(command);
				if (templateTracks.Count == 1) if1ThenTheId = newTrack.Id;
			}
		}
		catch (System.Exception e)
		{
			HeaderMessage.Show($"ճ�����ʱ���ִ���{e.Message}", HeaderMessage.MessageType.Error);
			return;
		}

		CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "ճ�����"));
		if (if1ThenTheId > 0)
		{
			SelectManager.Instance.UnselectAll();
			SelectManager.Instance.SelectTarget = SelectTarget.Track;
			SelectManager.Instance.SelectTrack(if1ThenTheId);
		}

		HeaderMessage.Show("ճ���ɹ���", HeaderMessage.MessageType.Success);
	}

	public void PasteTurningPoints(float time)
	{
		if (templateLeftPoints.Count + templateRightPoints.Count == 0) return;
		if (SelectManager.Instance.SelectedTracks.Count != 1)
		{
			HeaderMessage.Show("��ѡ��һ������Խ��н����ͨճ��", HeaderMessage.MessageType.Info);
			return;
		}

		Track track = SelectManager.Instance.SelectedTracks[0].Track;
		templateLeftPoints.Sort((a, b) => a.time.CompareTo(b.time));
		templateRightPoints.Sort((a, b) => a.time.CompareTo(b.time));
		float baseTime = Mathf.Min(templateLeftPoints[0].time, templateRightPoints[0].time);
		List<ICommand> commands = new();
		try
		{
			float originalX = (originalTrack.GetX(baseTime, true) + originalTrack.GetX(baseTime, false)) / 2;
			float trackX = (track.GetX(time, true) + track.GetX(time, false)) / 2;
			foreach (var item in templateLeftPoints)
			{
				var command = new MoveListInsertCommand(track.LMoveList,
					(time + item.time - baseTime, trackX + item.x - originalX, item.curve));
				commands.Add(command);
			}

			foreach (var item in templateRightPoints)
			{
				var command = new MoveListInsertCommand(track.RMoveList,
					(time + item.time - baseTime, trackX + item.x - originalX, item.curve));
				commands.Add(command);
			}
		}
		catch
		{
			HeaderMessage.Show("ճ��Ƭ�γ��ȳ������ʱ�䷶Χ", HeaderMessage.MessageType.Warn);
			return;
		}

		CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "ճ�����"));
		HeaderMessage.Show("ճ���ɹ���", HeaderMessage.MessageType.Success);
	}

	public void PasteExactBaseNotes(float time)
	{
		if (templateBaseNotes.Count == 0) return;
		if (SelectManager.Instance.SelectedTracks.Count != 0)
		{
			HeaderMessage.Show("����ѡ�еĹ���Խ���Noteԭλճ��", HeaderMessage.MessageType.Info);
			return;
		}

		templateBaseNotes.Sort((a, b) => a.TimeJudge.CompareTo(b.TimeJudge));
		float baseTime = templateBaseNotes[0].TimeJudge;
		List<ICommand> commands = new();
		try
		{
			foreach (BaseNote note in templateBaseNotes)
			{
				var command = new CreateNoteCommand(
					note.Clone(EditingLevelManager.Instance.RawChartInfo.NewId, time + note.TimeJudge - baseTime,
						note.BelongingTrack));
				commands.Add(command);
			}
		}
		catch
		{
			HeaderMessage.Show("ճ��Ƭ�γ��ȳ������ʱ�䷶Χ", HeaderMessage.MessageType.Warn);
			return;
		}

		CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "ճ��Note"));
		HeaderMessage.Show("ճ���ɹ���", HeaderMessage.MessageType.Success);
	}

	public void PasteExactTracks(float time)
	{
		if (templateTracks.Count == 0) return;
		templateTracks.Sort((a, b) => a.TimeInstantiate.CompareTo(b.TimeInstantiate));
		float baseTime = templateTracks[0].TimeInstantiate;
		List<ICommand> commands = new();
		int if1ThenTheId = -1;
		try
		{
			foreach (var track in templateTracks)
			{
				Track newTrack = track.Clone(EditingLevelManager.Instance.RawChartInfo.NewId,
					time + track.TimeInstantiate - baseTime, track.GetX(track.TimeInstantiate, true));
				var command = new CreateTrackCommand(newTrack);
				commands.Add(command);
				if (templateTracks.Count == 1) if1ThenTheId = newTrack.Id;
			}
		}
		catch (System.Exception e)
		{
			HeaderMessage.Show($"ճ�����ʱ���ִ���{e.Message}", HeaderMessage.MessageType.Error);
			return;
		}

		CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "ճ�����"));
		if (if1ThenTheId > 0)
		{
			SelectManager.Instance.UnselectAll();
			SelectManager.Instance.SelectTarget = SelectTarget.Track;
			SelectManager.Instance.SelectTrack(if1ThenTheId);
		}

		HeaderMessage.Show("ճ���ɹ���", HeaderMessage.MessageType.Success);
	}

	public void PasteExactTurningPoints(float time)
	{
		if (templateLeftPoints.Count + templateRightPoints.Count == 0) return;
		if (SelectManager.Instance.SelectedTracks.Count != 1)
		{
			HeaderMessage.Show("��ѡ��һ������Խ��н����ͨճ��", HeaderMessage.MessageType.Info);
			return;
		}

		Track track = SelectManager.Instance.SelectedTracks[0].Track;
		templateLeftPoints.Sort((a, b) => a.time.CompareTo(b.time));
		templateRightPoints.Sort((a, b) => a.time.CompareTo(b.time));
		float baseTime = Mathf.Min(templateLeftPoints[0].time, templateRightPoints[0].time);
		List<ICommand> commands = new();
		try
		{
			foreach (var item in templateLeftPoints)
			{
				var command =
					new MoveListInsertCommand(track.LMoveList, (time + item.time - baseTime, item.x, item.curve));
				commands.Add(command);
			}

			foreach (var item in templateRightPoints)
			{
				var command =
					new MoveListInsertCommand(track.RMoveList, (time + item.time - baseTime, item.x, item.curve));
				commands.Add(command);
			}
		}
		catch
		{
			HeaderMessage.Show("ճ��Ƭ�γ��ȳ������ʱ�䷶Χ", HeaderMessage.MessageType.Warn);
			return;
		}

		CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "ճ�����"));
		HeaderMessage.Show("ճ���ɹ���", HeaderMessage.MessageType.Success);
	}

	/// <summary> ����������ǰ���������ݵ��ַ��� </summary>
	public string GetClipboardDescription()
	{
		if (templateBaseNotes.Count != 0)
		{
			return $"��ǰ���������ݣ�{templateBaseNotes.Count}��Note";
		}
		else if (templateTracks.Count != 0)
		{
			return $"��ǰ���������ݣ�{templateTracks.Count}�����";
		}
		else if (templateLeftPoints.Count + templateRightPoints.Count != 0)
		{
			return $"��ǰ���������ݣ�{templateLeftPoints.Count}�����㣬{templateRightPoints.Count}���ҽ��";
		}
		else return "��ǰ����������Ϊ��";
	}

	// System Functions
	void Awake()
	{
		_instance = this;
	}

	void Update()
	{
		if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.Paste))
		{
			var position = Input.mousePosition;
			if (Camera.main.ContainsScreenPoint(position))
			{
				var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
				float left = gamePoint.x;
				if (GridManager.Instance.IsXGridShow) left = GridManager.Instance.GetNearestXGridPos(left).left;
				if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
				var time = GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed,
					gamePoint.y);
				if (templateBaseNotes.Count != 0) PasteBaseNotes(time);
				else if (templateTracks.Count != 0) PasteTracks(time, left);
				else if (templateLeftPoints.Count + templateRightPoints.Count != 0) PasteTurningPoints(time);
			}
		}
		else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.ExactPaste))
		{
			var position = Input.mousePosition;
			if (Camera.main.ContainsScreenPoint(position))
			{
				var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
				if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
				var time = GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed,
					gamePoint.y);
				if (templateBaseNotes.Count != 0) PasteExactBaseNotes(time);
				else if (templateTracks.Count != 0) PasteExactTracks(time);
				else if (templateLeftPoints.Count + templateRightPoints.Count != 0) PasteExactTurningPoints(time);
			}
		}
		else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.CheckClipboard))
		{
			HeaderMessage.Show(GetClipboardDescription(), HeaderMessage.MessageType.Info);
		}
	}
}