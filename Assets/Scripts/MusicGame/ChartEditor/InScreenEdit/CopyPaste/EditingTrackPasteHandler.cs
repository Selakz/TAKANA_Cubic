using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class EditingTrackPasteHandler : IPasteHandler
	{
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);
		private readonly Camera levelCamera;
		private readonly ChartSelectDataset dataset;
		private readonly NotifiableProperty<ITimeRetriever> timeRetriever;
		private readonly NotifiableProperty<IWidthRetriever> widthRetriever;

		public CopyPastePlugin CopyPaste { get; }

		public EditingTrackPasteHandler(
			CopyPastePlugin copyPastePlugin,
			Camera levelCamera,
			ChartSelectDataset dataset,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			NotifiableProperty<IWidthRetriever> widthRetriever)
		{
			CopyPaste = copyPastePlugin;
			this.levelCamera = levelCamera;
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
		}

		public string GetDescription()
		{
			int count = CopyPaste.Clipboard.Count(c => c.Model is ITrack);
			return $"Edit_CopyPast_TrackClipboard|{count}";
		}

		public void Cut()
		{
			var toDelete = CopyPaste.Clipboard.Where(c => c.Model is ITrack).ToList();
			if (toDelete.Count == 0) return;
			CommandManager.Instance.Add(new BatchCommand(
				toDelete.Select(c => new DeleteComponentCommand(c)),
				"TrackCut"));
		}

		public bool Paste(out string message)
		{
			var mousePoint = Input.mousePosition;
			if (!levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				message = string.Empty;
				return false;
			}

			var tracks = CopyPaste.Clipboard.Where(c => c.Model is ITrack).ToList();
			if (tracks.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			tracks.Sort((a, b) => ((ITrack)a.Model).TimeStart.CompareTo(((ITrack)b.Model).TimeStart));
			T3Time baseTime = ((ITrack)tracks[0].Model).TimeStart;
			float baseLeft = ((ITrack)tracks[0].Model).Movement.GetLeftPos(baseTime);

			var commands = new List<ICommand>();
			T3Time time = timeRetriever.Value.GetTimeStart(gamePoint);
			float left = widthRetriever.Value.GetAttachedPosition(gamePoint);
			var distance = time - baseTime;
			var positionOffset = left - baseLeft;
			foreach (var track in tracks)
			{
				if (!track.IsWithinParentRange(distance))
				{
					message = string.Empty;
					return false;
				}

				commands.Add(new CloneComponentCommand(track, component =>
				{
					component.Parent = track.Parent;
					component.Nudge(distance);
					component.UpdateModel(model => ((ITrack)model).Shift(positionOffset));
				}));
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "TrackPaste"));
			message = "Edit_CopyPaste_PasteSuccess";
			return true;
		}

		public bool ExactPaste(out string message)
		{
			var mousePoint = Input.mousePosition;
			if (!levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				message = string.Empty;
				return false;
			}

			var tracks = CopyPaste.Clipboard.Where(c => c.Model is ITrack).ToList();
			if (tracks.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			tracks.Sort((a, b) => ((ITrack)a.Model).TimeStart.CompareTo(((ITrack)b.Model).TimeStart));
			T3Time baseTime = ((ITrack)tracks[0].Model).TimeStart;

			var commands = new List<ICommand>();
			T3Time time = timeRetriever.Value.GetTimeStart(gamePoint);
			var distance = time - baseTime;
			foreach (var track in tracks)
			{
				if (!track.IsWithinParentRange(distance))
				{
					message = string.Empty;
					return false;
				}

				commands.Add(new CloneComponentCommand(track, component =>
				{
					component.Parent = track.Parent;
					component.Nudge(distance);
				}));
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "TrackPaste"));
			message = "Edit_CopyPaste_PasteSuccess";
			return true;
		}
	}
}