#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeCopyPastePlugin : T3MonoBehaviour, ISelfInstaller
	{
		public enum PasteMode
		{
			None,
			NormalPaste,
			ExactPaste
		}

		// Serializable and Public
		[SerializeField] private SequencePriority chartEditPriority = default!;

		public NotifiableProperty<PasteMode> Mode { get; private set; } = new(PasteMode.None);

		public IReadOnlyList<EdgeNodeComponent> Clipboard => clipboard;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Cut", chartEditPriority.Value,
				() =>
				{
					if (nodeSelectDataset.Count == 0) return true;
					var ret = CopyToClipboard();
					Cut();
					return ret;
				}),
			new InputRegistrar("InScreenEdit", "Copy", chartEditPriority.Value, CopyToClipboard),
			new InputRegistrar("InScreenEdit", "Paste", chartEditPriority.Value, () =>
			{
				if (clipboard.Count == 0) return true;
				Mode.Value = PasteMode.NormalPaste;
				return false;
			}),
			new InputRegistrar("InScreenEdit", "ExactPaste", chartEditPriority.Value, () =>
			{
				if (clipboard.Count == 0) return true;
				Mode.Value = PasteMode.ExactPaste;
				return false;
			}),
			new InputRegistrar("InScreenEdit", "CheckClipboard", chartEditPriority.Value,
				() =>
				{
					if (clipboard.Count == 0) return true;
					CheckClipboard();
					return false;
				}),
			new InputRegistrar("InScreenEdit", "Create", chartEditPriority.Value,
				() =>
				{
					switch (Mode.Value)
					{
						case PasteMode.None:
							return true;
						case PasteMode.NormalPaste:
							Paste();
							break;
						case PasteMode.ExactPaste:
							ExactPaste();
							break;
					}

					Mode.Value = PasteMode.None;
					return false;
				}),
			new InputRegistrar("General", "Escape", () => Mode.Value = PasteMode.None)
		};

		// Private
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);
		private readonly List<EdgeNodeComponent> clipboard = new();

		private Camera levelCamera = default!;
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		private ChartSelectDataset chartSelectDataset = default!;
		private EdgeNodeSelectDataset nodeSelectDataset = default!;
		private CommandManager commandManager = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			ChartSelectDataset chartSelectDataset,
			EdgeNodeSelectDataset nodeSelectDataset,
			CommandManager commandManager)
		{
			this.levelCamera = levelCamera;
			this.timeRetriever = timeRetriever;
			this.chartSelectDataset = chartSelectDataset;
			this.nodeSelectDataset = nodeSelectDataset;
			this.commandManager = commandManager;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void Cut()
		{
			if (nodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> tracks = new();
			foreach (var node in nodeSelectDataset)
			{
				var track = node.Locator.Track;
				if (!tracks.TryGetValue(track, out var args))
				{
					args = new();
					tracks[track] = args;
				}

				args.Add(new UpdateMoveListArg(node.Locator.IsLeft, node.Locator.Time, null));
			}

			List<ICommand> commands = (
					from pair in tracks
					let command = new UpdateMoveListCommand(pair.Value)
					where command.SetInit(pair.Key)
					select command)
				.Cast<ICommand>().ToList();
			commandManager.Add(new BatchCommand(commands, "Delete Nodes"));
		}

		public bool CopyToClipboard()
		{
			clipboard.Clear();
			if (nodeSelectDataset.Count == 0) return true;

			T3Logger.Log("Notice", "Edit_CopyPaste_CopySuccess", T3LogType.Success);
			clipboard.AddRange(nodeSelectDataset);
			return false;
		}

		public bool Paste()
		{
			// TODO: ModuleInfo
			var mousePoint = Input.mousePosition;
			if (!levelCamera.ContainsScreenPoint(mousePoint) ||
			    !levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				return true;
			}

			if (clipboard.Count == 0) return true;

			if (chartSelectDataset.CurrentSelecting.Value is not { Model: ITrack model } track)
			{
				T3Logger.Log("Notice", "TrackLine_PasteFailForTrack", T3LogType.Info);
				return false;
			}

			clipboard.Sort((a, b) => a.Locator.Time.CompareTo(b.Locator.Time));
			float baseTime = clipboard[0].Locator.Time;

			List<UpdateMoveListArg> cloneArgs = new();
			T3Time time = timeRetriever.Value.GetTimeStart(gamePoint);

			foreach (var component in clipboard)
			{
				var newTime = time + component.Locator.Time - baseTime;
				var newPos = model.Movement.GetPos(time) -
					((ITrack)component.Locator.Track.Model).Movement.GetPos(baseTime) + component.Model.Position;
				var newItem = component.Model.SetPosition(newPos);

				cloneArgs.Add(new UpdateMoveListArg(component.Locator.IsLeft, null, new(newTime, newItem)));
			}

			var command = new UpdateMoveListCommand(cloneArgs);
			if (command.SetInit(track))
			{
				CommandManager.Instance.Add(command);
				T3Logger.Log("Notice", "Edit_CopyPaste_PasteSuccess", T3LogType.Success);
			}
			else
			{
				T3Logger.Log("Notice", "TrackLine_PasteFailForNode", T3LogType.Warn);
			}

			return false;
		}

		public bool ExactPaste()
		{
			var mousePoint = Input.mousePosition;
			if (!levelCamera.ContainsScreenPoint(mousePoint) ||
			    !levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				return true;
			}

			if (clipboard.Count == 0) return true;

			if (chartSelectDataset.CurrentSelecting.Value is not { Model: ITrack } track)
			{
				T3Logger.Log("Notice", "TrackLine_ExactPasteFailForTrack", T3LogType.Info);
				return false;
			}

			clipboard.Sort((a, b) => a.Locator.Time.CompareTo(b.Locator.Time));
			float baseTime = clipboard[0].Locator.Time;

			List<UpdateMoveListArg> cloneArgs = new();
			T3Time time = timeRetriever.Value.GetTimeStart(gamePoint);

			foreach (var component in clipboard)
			{
				var newTime = time + component.Locator.Time - baseTime;
				var newPos = component.Model.Position;
				var newItem = component.Model.SetPosition(newPos);

				cloneArgs.Add(new UpdateMoveListArg(component.Locator.IsLeft, null, new(newTime, newItem)));
			}

			var command = new UpdateMoveListCommand(cloneArgs);
			if (command.SetInit(track))
			{
				CommandManager.Instance.Add(command);
				T3Logger.Log("Notice", "Edit_CopyPaste_PasteSuccess", T3LogType.Success);
			}
			else
			{
				T3Logger.Log("Notice", "TrackLine_PasteFailForNode", T3LogType.Warn);
			}

			return false;
		}

		public void CheckClipboard()
		{
			T3Logger.Log("Notice", $"TrackLine_NodeClipboard|{clipboard.Count}", T3LogType.Info);
		}
	}
}