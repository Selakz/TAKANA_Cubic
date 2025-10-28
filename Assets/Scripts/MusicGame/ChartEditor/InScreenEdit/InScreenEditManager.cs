using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.Components.Movement;
using MusicGame.Components.Notes;
using MusicGame.Components.Tracks;
using MusicGame.Components.Tracks.Movement;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Easing;
using UnityEngine;

// ReSharper disable MemberCanBeMadeStatic.Local
namespace MusicGame.ChartEditor.InScreenEdit
{
	// TODO: Extract interface
	public class InScreenEditManager : MonoBehaviour
	{
		// Enum
		public enum CreateNoteType
		{
			Tap,
			Slide,
			Hold
		}

		// Serializable and Public
		public static InScreenEditManager Instance { get; private set; }
		public static ITimeRetriever FallbackTimeRetriever { get; } = new DefaultTimeRetriever();
		public static IWidthRetriever FallbackWidthRetriever { get; } = new DefaultWidthRetriever();

		public ITimeRetriever TimeRetriever { get; set; } = FallbackTimeRetriever;

		public IWidthRetriever WidthRetriever { get; set; } = FallbackWidthRetriever;

		public CreateNoteType NoteType { get; set; } = CreateNoteType.Tap;

		// Private

		// Static

		// Defined Functions
		private void CreateNote()
		{
			var mousePoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(mousePoint) ||
			    ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack)
				return;

			var track = editingTrack.Track;
			var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(mousePoint);
			var time = TimeRetriever.GetTimeStart(gamePoint);
			if (time < track.TimeInstantiate || time > track.TimeEnd)
			{
				HeaderMessage.Show("尝试创建的时间在所选轨道的时间范围外", HeaderMessage.MessageType.Warn);
				return;
			}

			INote note;
			switch (NoteType)
			{
				case CreateNoteType.Tap:
					note = new Tap(time, track);
					break;
				case CreateNoteType.Slide:
					note = new Slide(time, track);
					break;
				case CreateNoteType.Hold:
					T3Time timeEnd = Mathf.Min(TimeRetriever.GetHoldTimeEnd(gamePoint), track.TimeEnd);
					note = new Hold(time, timeEnd, track);
					break;
				default:
					throw new System.Exception("Unhandled create type");
			}

			if (!EventManager.Instance.InvokeVeto("Edit_QueryPlaceNote", note, out var reasons))
			{
				HeaderMessage.Show(reasons.FirstOrDefault(), HeaderMessage.MessageType.Warn);
				return;
			}

			CommandManager.Instance.Add(new AddComponentsCommand(note));
		}

		private void NoteToNext()
		{
			var notes = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingNote);
			var noteToNextDistance = ISingletonSetting<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
			List<UpdateComponentArg> args = new();
			foreach (var editingNote in notes)
			{
				var note = (editingNote as EditingNote)!.Note;
				UpdateComponentArg arg;
				if (note is Hold hold)
				{
					var actualDistance = Mathf.Min(hold.Parent.TimeEnd - hold.TimeEnd, noteToNextDistance);
					arg = new(hold,
						h =>
						{
							(h as Hold)!.TimeJudge += actualDistance;
							(h as Hold)!.TimeEnd += actualDistance;
						},
						h =>
						{
							(h as Hold)!.TimeJudge -= actualDistance;
							(h as Hold)!.TimeEnd -= actualDistance;
						});
				}
				else
				{
					var actualDistance = Mathf.Min(note.Parent.TimeEnd - note.TimeJudge, noteToNextDistance);
					arg = new(note,
						n => { (n as BaseNote)!.TimeJudge += actualDistance; },
						n => { (n as BaseNote)!.TimeJudge -= actualDistance; });
				}

				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void NoteToPrevious()
		{
			var notes = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingNote);
			var noteToPreviousDistance = ISingletonSetting<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
			List<UpdateComponentArg> args = new();
			foreach (var editingNote in notes)
			{
				var note = (editingNote as EditingNote)!.Note;
				var actualDistance = Mathf.Min(note.TimeJudge - note.Parent.TimeInstantiate, noteToPreviousDistance);
				UpdateComponentArg arg;
				if (note is Hold hold)
				{
					arg = new(hold,
						h =>
						{
							(h as Hold)!.TimeJudge -= actualDistance;
							(h as Hold)!.TimeEnd -= actualDistance;
						},
						h =>
						{
							(h as Hold)!.TimeJudge += actualDistance;
							(h as Hold)!.TimeEnd += actualDistance;
						});
				}
				else
				{
					arg = new(note,
						n => { (n as BaseNote)!.TimeJudge -= actualDistance; },
						n => { (n as BaseNote)!.TimeJudge += actualDistance; });
				}

				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void NoteToNextBeat()
		{
			if (TimeRetriever is not GridTimeRetriever timeRetriever) return;
			var notes = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingNote);
			List<UpdateComponentArg> args = new();
			foreach (var editingNote in notes)
			{
				var note = (editingNote as EditingNote)!.Note;
				UpdateComponentArg arg;
				if (note is Hold hold)
				{
					var updatedTime = Mathf.Min(note.Parent.TimeEnd, timeRetriever.GetCeilTime(hold.TimeEnd));
					var actualDistance = updatedTime - hold.TimeEnd;
					arg = new(hold,
						h =>
						{
							(h as Hold)!.TimeJudge += actualDistance;
							(h as Hold)!.TimeEnd += actualDistance;
						},
						h =>
						{
							(h as Hold)!.TimeJudge -= actualDistance;
							(h as Hold)!.TimeEnd -= actualDistance;
						});
				}
				else
				{
					var updatedTime = Mathf.Min(note.Parent.TimeEnd, timeRetriever.GetCeilTime(note.TimeJudge));
					var actualDistance = updatedTime - note.TimeJudge;
					arg = new(note,
						n => { (n as BaseNote)!.TimeJudge += actualDistance; },
						n => { (n as BaseNote)!.TimeJudge -= actualDistance; });
				}

				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void NoteToPreviousBeat()
		{
			if (TimeRetriever is not GridTimeRetriever timeRetriever) return;
			var notes = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingNote);
			List<UpdateComponentArg> args = new();
			foreach (var editingNote in notes)
			{
				var note = (editingNote as EditingNote)!.Note;
				var updatedTime =
					Mathf.Max(note.Parent.TimeInstantiate, timeRetriever.GetFloorTime(note.TimeJudge));
				var actualDistance = note.TimeJudge - updatedTime;
				UpdateComponentArg arg;
				if (note is Hold hold)
				{
					arg = new(hold,
						h =>
						{
							(h as Hold)!.TimeJudge -= actualDistance;
							(h as Hold)!.TimeEnd -= actualDistance;
						},
						h =>
						{
							(h as Hold)!.TimeJudge += actualDistance;
							(h as Hold)!.TimeEnd += actualDistance;
						});
				}
				else
				{
					arg = new(note,
						n => { (n as BaseNote)!.TimeJudge -= actualDistance; },
						n => { (n as BaseNote)!.TimeJudge += actualDistance; });
				}

				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void CreateTrack()
		{
			ISelectManager.Instance.UnselectAll();
			var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(Input.mousePosition);
			var timeStart = TimeRetriever.GetTimeStart(gamePoint);
			var timeEnd = TimeRetriever.GetTrackTimeEnd(gamePoint);
			float width = WidthRetriever.GetWidth(gamePoint), pos = WidthRetriever.GetPosition(gamePoint);
			float left = pos - width / 2, right = pos + width / 2;
			IEnumerable<V1EMoveItem> leftItems = new V1EMoveItem[]
				{ new(timeStart, left, Eases.Unmove), new(timeEnd, left, Eases.Unmove) };
			IEnumerable<V1EMoveItem> rightItems = new V1EMoveItem[]
				{ new(timeStart, right, Eases.Unmove), new(timeEnd, right, Eases.Unmove) };
			Track track = new(timeStart, timeEnd, IEditingChartManager.Instance.DefaultJudgeLine)
			{
				Movement = new TrackEdgeMovement(leftItems, rightItems)
			};
			if (!EventManager.Instance.InvokeVeto("InScreenEdit_QueryPlaceTrack", track, out var reasons))
			{
				HeaderMessage.Show(reasons.FirstOrDefault(), HeaderMessage.MessageType.Warn);
				return;
			}

			CommandManager.Instance.Add(new AddComponentsCommand(track));
			ISelectManager.Instance.Select(track.Id);
		}

		private void Delete()
		{
			if (!EventManager.Instance.InvokeVeto("Edit_QueryDelete", out _)) return;

			var toDelete = ISelectManager.Instance.SelectedTargets.Values.ToList();
			bool hasTrack = false;
			int noteCount = 0;
			foreach (var component in ISelectManager.Instance.SelectedTargets.Values)
			{
				if (!EventManager.Instance.InvokeVeto("Edit_QueryDelete", component, out _))
				{
					toDelete.Remove(component);
					continue;
				}

				if (component is EditingTrack)
				{
					hasTrack = true;
					noteCount += IEditingChartManager.Instance.GetChildrenComponents(component.Id)
						.Count(c => c is EditingNote);
				}
			}

			if (toDelete.Count == 0) return;

			CommandManager.Instance.Add(new DeleteComponentsCommand(toDelete));
			if (hasTrack && noteCount > 0)
			{
				HeaderMessage.Show($"同时删除了{noteCount}个Note", HeaderMessage.MessageType.Info);
			}
		}

		// System Functions
		void OnEnable()
		{
			Instance = this;
			InputManager.Instance.Register("InScreenEdit", "CreateNote", _ => CreateNote());
			InputManager.Instance.Register("InScreenEdit", "ToNext", _ => NoteToNext());
			InputManager.Instance.Register("InScreenEdit", "ToPrevious", _ => NoteToPrevious());
			InputManager.Instance.Register("InScreenEdit", "ToNextBeat", _ => NoteToNextBeat());
			InputManager.Instance.Register("InScreenEdit", "ToPreviousBeat", _ => NoteToPreviousBeat());
			InputManager.Instance.Register("InScreenEdit", "CreateTrack", _ => CreateTrack());
			InputManager.Instance.Register("InScreenEdit", "Delete", _ => Delete());
		}
	}
}