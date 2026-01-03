#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class EdgeNodeEditSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority nodePreviewModuleId = default!;
		[SerializeField] private SequencePriority chartEditPriority = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "ToLeft",
				() => UpdatePosition(pos => pos - ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToRight",
				() => UpdatePosition(pos => pos + ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToLeftGrid",
				() =>
				{
					if (widthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdatePosition(pos => retriever.GetLeftAttachedPosition(pos));
				}),
			new InputRegistrar("InScreenEdit", "ToRightGrid",
				() =>
				{
					if (widthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdatePosition(pos => retriever.GetRightAttachedPosition(pos));
				}),
			new InputRegistrar("InScreenEdit", "ToNext",
				() => UpdateTime(time => time + ISingleton<TrackLineSetting>.Instance.NodeTimeNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToPrevious",
				() => UpdateTime(time => time - ISingleton<TrackLineSetting>.Instance.NodeTimeNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToNextBeat",
				() =>
				{
					if (timeRetriever.Value is not GridTimeRetriever retriever) return;
					UpdateTime(time => retriever.GetCeilTime(time));
				}),
			new InputRegistrar("InScreenEdit", "ToPreviousBeat",
				() =>
				{
					if (timeRetriever.Value is not GridTimeRetriever retriever) return;
					UpdateTime(time => retriever.GetFloorTime(time));
				}),
			new InputRegistrar("InScreenEdit", "Create", chartEditPriority.Value,
				() =>
				{
					if (moduleInfo.CurrentModule != nodePreviewModuleId) return true;
					if (chartSelectDataset.CurrentSelecting.Value is not { Model: ITrack model } track) return true;

					var mousePoint = Input.mousePosition;
					if (!levelCamera.ContainsScreenPoint(mousePoint) ||
					    !levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
					{
						return true;
					}

					float baseTime = timeRetriever.Value.GetTimeStart(gamePoint);
					float basePosition = gamePoint.x;
					float actualPosition = widthRetriever.Value.GetAttachedPosition(gamePoint);

					float leftX = model.Movement.GetLeftPos(baseTime), rightX = model.Movement.GetRightPos(baseTime);
					bool isLeft = basePosition < (leftX + rightX) / 2;
					UpdateMoveListArg arg = new(
						isLeft, null, new(baseTime, new V1EMoveItem(actualPosition, Eases.Unmove)));
					var command = new UpdateMoveListCommand(arg);
					if (command.SetInit(track)) commandManager.Add(command);
					return false;
				}),
			new InputRegistrar("InScreenEdit", "Delete", "delete", chartEditPriority.Value,
				() =>
				{
					if (nodeSelectDataset.Count == 0) return true;
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
					return false;
				}),
			new InputRegistrar("CurveSwitch", "ChangeNodeState", () =>
			{
				if (nodeSelectDataset.Count == 0) return;
				Dictionary<ChartComponent, List<UpdateMoveListArg>> tracks = new();
				foreach (var node in nodeSelectDataset)
				{
					if (node.Model is not V1EMoveItem v1e) continue;
					var track = node.Locator.Track;
					if (!tracks.TryGetValue(track, out var args))
					{
						args = new();
						tracks[track] = args;
					}

					args.Add(new UpdateMoveListArg(node.Locator.IsLeft, node.Locator.Time,
						new(node.Locator.Time, new V1EMoveItem(v1e.Position, GetNextEase(v1e.Ease)))));
				}

				List<ICommand> commands = (
						from pair in tracks
						let command = new UpdateMoveListCommand(pair.Value)
						where command.SetInit(pair.Key)
						select command)
					.Cast<ICommand>().ToList();
				commandManager.Add(new BatchCommand(commands, "Delete Nodes"));
			})
		};

		// Private
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		private Camera levelCamera = default!;
		private ModuleInfo moduleInfo = default!;
		private NotifiableProperty<int> currentEaseId = default!;
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		private NotifiableProperty<IWidthRetriever> widthRetriever = default!;
		private ChartSelectDataset chartSelectDataset = default!;
		private EdgeNodeSelectDataset nodeSelectDataset = default!;
		private CommandManager commandManager = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			ModuleInfo moduleInfo,
			[Key("ease-id")] NotifiableProperty<int> currentEaseId,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			NotifiableProperty<IWidthRetriever> widthRetriever,
			ChartSelectDataset chartSelectDataset,
			EdgeNodeSelectDataset nodeSelectDataset,
			CommandManager commandManager)
		{
			this.levelCamera = levelCamera;
			this.moduleInfo = moduleInfo;
			this.currentEaseId = currentEaseId;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			this.chartSelectDataset = chartSelectDataset;
			this.nodeSelectDataset = nodeSelectDataset;
			this.commandManager = commandManager;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public void UpdatePosition(Func<float, float> newPositionFunc)
		{
			if (nodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset)
			{
				var item = node.Model;
				var newItem = item.SetPosition(newPositionFunc.Invoke(item.Position));
				var time = node.Locator.Time;
				var arg = new UpdateMoveListArg(node.Locator.IsLeft, time, new(time, newItem));
				var track = node.Locator.Track;
				if (!args.ContainsKey(track)) args[track] = new() { arg };
				else args[track].Add(arg);
			}

			BatchCommand command = new(args
					.Select(pair => (Track: pair.Key, Command: new UpdateMoveListCommand(pair.Value)))
					.Where(tuple => tuple.Command.SetInit(tuple.Track))
					.Select(tuple => tuple.Command),
				"UpdateMoveList");
			commandManager.Add(command);
		}

		public void UpdateTime(Func<T3Time, T3Time> newTimeFunc)
		{
			if (nodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset)
			{
				var item = node.Model;
				var time = node.Locator.Time;
				var newTime = newTimeFunc.Invoke(time);
				var arg = new UpdateMoveListArg(node.Locator.IsLeft, time, new(newTime, item));
				var track = node.Locator.Track;
				if (!args.ContainsKey(track)) args[track] = new() { arg };
				else args[track].Add(arg);
			}

			BatchCommand command = new(args
					.Select(pair => (Track: pair.Key, Command: new UpdateMoveListCommand(pair.Value)))
					.Where(tuple => tuple.Command.SetInit(tuple.Track))
					.Select(tuple => tuple.Command),
				"UpdateMoveList");
			commandManager.Add(command);
		}

		public Eases GetNextEase(Eases ease)
		{
			var label = ease.GetString();
			var newLabel = label switch
			{
				"u" => "s",
				"s" => CurveCalculator.GetEaseById(currentEaseId.Value * 10 + 2).GetString(),
				_ => label[1] switch
				{
					'i' => label[0] + "o",
					'o' => label[0] + "b",
					'b' => label[0] + "a",
					'a' => "u",
					_ => label switch
					{
						"u" => "s",
						"s" => "si",
						"si" => "so",
						"so" => "sb",
						"sb" => "sa",
						"sa" => "u",
						_ => "u"
					}
				}
			};
			return CurveCalculator.GetEaseByName(newLabel);
		}
	}
}