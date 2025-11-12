using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Level;
using MusicGame.Components;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class EditingTrackPasteHandler : IPasteHandler
	{
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		public CopyPastePlugin CopyPaste { get; }

		public EditingTrackPasteHandler(CopyPastePlugin copyPastePlugin)
		{
			CopyPaste = copyPastePlugin;
		}

		public string GetDescription()
		{
			int count = CopyPaste.Clipboard.Count(c => c is EditingTrack);
			return $"已复制{count}个轨道";
		}

		public void Cut()
		{
			var toDelete = CopyPaste.Clipboard.Where(c => c is EditingTrack).ToList();
			if (toDelete.Count == 0) return;
			CommandManager.Instance.Add(new DeleteComponentsCommand(toDelete));
		}

		public bool Paste(out string message)
		{
			var mousePoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				message = string.Empty;
				return false;
			}

			var tracks = CopyPaste.Clipboard.Where(c => c is EditingTrack).Cast<EditingTrack>().ToList();
			if (tracks.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			tracks.Sort((a, b) => a.TimeInstantiate.CompareTo(b.TimeInstantiate));
			T3Time time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
			float left = InScreenEditManager.Instance.WidthRetriever.GetAttachedPosition(gamePoint);
			float baseTime = tracks[0].TimeInstantiate;
			float baseLeft = tracks[0].Track.Movement.GetLeftPos(baseTime);
			List<IComponent> cloneComponents = new();
			foreach (var track in tracks)
			{
				var newTrack = track.Clone(time + track.TimeInstantiate - baseTime, left - baseLeft);
				cloneComponents.Add(newTrack);

				if (IEditingChartManager.Instance != null)
				{
					var children = IEditingChartManager.Instance.GetChildrenComponents(track.Id);
					foreach (var note in children.Where(c => c is EditingNote).Cast<EditingNote>())
					{
						var newTime = time + note.Note.TimeJudge - baseTime;
						var newNote = note.Clone(newTime, newTrack.Track);
						cloneComponents.Add(newNote);
					}
				}
			}

			CommandManager.Instance.Add(new AddComponentsCommand(cloneComponents));
			message = "粘贴成功！";
			return true;
		}

		public bool ExactPaste(out string message)
		{
			var mousePoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				message = string.Empty;
				return false;
			}

			var tracks = CopyPaste.Clipboard.Where(c => c is EditingTrack).Cast<EditingTrack>().ToList();
			if (tracks.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			tracks.Sort((a, b) => a.TimeInstantiate.CompareTo(b.TimeInstantiate));
			T3Time time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
			float baseTime = tracks[0].TimeInstantiate;
			List<IComponent> cloneComponents = new();
			foreach (var track in tracks)
			{
				var newTrack = track.Track.Clone(time + track.TimeInstantiate - baseTime, 0);
				cloneComponents.Add(newTrack);

				if (IEditingChartManager.Instance != null)
				{
					var children = IEditingChartManager.Instance.GetChildrenComponents(track.Id);
					foreach (var note in children.Where(c => c is EditingNote).Cast<EditingNote>())
					{
						var newTime = time + note.Note.TimeJudge - baseTime;
						var newNote = note.Note.Clone(newTime, newTrack);
						cloneComponents.Add(newNote);
					}
				}
			}

			CommandManager.Instance.Add(new AddComponentsCommand(cloneComponents));
			message = "粘贴成功！";
			return true;
		}
	}
}