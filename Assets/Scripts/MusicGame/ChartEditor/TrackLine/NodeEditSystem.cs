#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeEditSystem : HierarchySystem<NodeEditSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority updateModuleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "ToLeft",
				() => UpdatePosition(pos => pos - ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToRight",
				() => UpdatePosition(pos => pos + ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToLeftGrid",
				() =>
				{
					if (widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdatePosition(pos => retriever.GetLeftAttachedPosition(pos));
				}),
			new InputRegistrar("InScreenEdit", "ToRightGrid",
				() =>
				{
					if (widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdatePosition(pos => retriever.GetRightAttachedPosition(pos));
				}),
			new InputRegistrar("InScreenEdit", "ToNext",
				() => UpdateTime(time => time + ISingleton<TrackLineSetting>.Instance.NodeTimeNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToPrevious",
				() => UpdateTime(time => time - ISingleton<TrackLineSetting>.Instance.NodeTimeNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToNextBeat",
				() =>
				{
					if (timeRetriever.TimeRetriever.Value is not GridTimeRetriever retriever) return;
					UpdateTime(time => retriever.GetCeilTime(time));
				}),
			new InputRegistrar("InScreenEdit", "ToPreviousBeat",
				() =>
				{
					if (timeRetriever.TimeRetriever.Value is not GridTimeRetriever retriever) return;
					UpdateTime(time => retriever.GetFloorTime(time));
				}),
			new InputRegistrar("InScreenEdit", "Widen",
				() => UpdateWidth(width => width + ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "WidenToGrid",
				() =>
				{
					if (widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdateWidth(width => retriever.GetRightAttachedPosition(width));
				}),
			new InputRegistrar("InScreenEdit", "Narrow",
				() => UpdateWidth(width => width - ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "NarrowToGrid",
				() =>
				{
					if (widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdateWidth(width => retriever.GetLeftAttachedPosition(width));
				}),
			new InputRegistrar("InScreenEdit", "Create",
				CreateNodes),
			new InputRegistrar("InScreenEdit", "Delete",
				() =>
				{
					if (nodeSelectDataset.Count == 0) return;
					Dictionary<ChartComponent, List<UpdateMoveListArg>> tracks = new();
					foreach (var node in nodeSelectDataset)
					{
						var track = node.Track;
						if (!tracks.TryGetValue(track, out var args))
						{
							args = new();
							tracks[track] = args;
						}

						args.Add(new UpdateMoveListArg(node.IsFirst, node.Time.Value, null));
					}

					List<ICommand> commands = (
							from pair in tracks
							let command = new UpdateMoveListCommand(pair.Value)
							where command.SetInit(pair.Key)
							select command)
						.Cast<ICommand>().ToList();
					commandManager.Add(new BatchCommand(commands, "Delete Nodes"));
				}),
			new InputRegistrar("CurveSwitch", "ChangeNodeState", () =>
			{
				if (nodeSelectDataset.Count == 0) return;
				Dictionary<ChartComponent, List<UpdateMoveListArg>> tracks = new();
				foreach (var node in nodeSelectDataset)
				{
					if (node.Model is not V1EMoveItem v1e) continue;
					var track = node.Track;
					if (!tracks.TryGetValue(track, out var args))
					{
						args = new();
						tracks[track] = args;
					}

					var time = node.Time.Value;
					args.Add(new UpdateMoveListArg(node.IsFirst, time,
						new(time, new V1EMoveItem(v1e.Position, GetNextEase(v1e.Ease)))));
				}

				List<ICommand> commands = (
						from pair in tracks
						let command = new UpdateMoveListCommand(pair.Value)
						where command.SetInit(pair.Key)
						select command)
					.Cast<ICommand>().ToList();
				commandManager.Add(new BatchCommand(commands, "Change Node State"));
			}),
			new PropertyRegistrar<int>(currentEaseId, id =>
			{
				ISingletonSetting<TrackLineSetting>.Instance.DefaultEaseId.Value = id;
				ISingletonSetting<TrackLineSetting>.SaveInstance();
			})
		};

		// Private
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] private readonly StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private readonly StageMouseWidthRetriever widthRetriever = default!;
		[Inject] [Key("ease-id")] private readonly NotifiableProperty<int> currentEaseId = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly IDataset<NodeRawInfo> nodeDataset = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Defined Functions
		public void UpdatePosition(Func<float, float> newPositionFunc)
		{
			if (moduleInfo.CurrentModule.Value != updateModuleId.Value) return;
			if (nodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset)
			{
				var item = node.Model;
				var newItem = item.SetPosition(newPositionFunc.Invoke(item.Position));
				if (node.Type == TrackNodeType.Width && newItem.Position < 0) continue;
				var time = node.Time.Value;
				var arg = new UpdateMoveListArg(node.IsFirst, time, new(time, newItem));
				var track = node.Track;
				if (!args.TryGetValue(track, out var list))
				{
					list = new();
					args[track] = list;
				}

				list.Add(arg);
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
			if (moduleInfo.CurrentModule.Value != updateModuleId.Value) return;
			if (nodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset)
			{
				var item = node.Model;
				var time = node.Time.Value;
				var newTime = newTimeFunc.Invoke(time);
				var arg = new UpdateMoveListArg(node.IsFirst, time, new(newTime, item));
				var track = node.Track;
				if (!args.TryGetValue(track, out var list))
				{
					list = new();
					args[track] = list;
				}

				list.Add(arg);
			}

			BatchCommand command = new(args
					.Select(pair => (Track: pair.Key, Command: new UpdateMoveListCommand(pair.Value)))
					.Where(tuple => tuple.Command.SetInit(tuple.Track))
					.Select(tuple => tuple.Command),
				"UpdateMoveList");
			commandManager.Add(command);
		}

		public void UpdateWidth(Func<float, float> newWidthFunc)
		{
			if (moduleInfo.CurrentModule.Value != updateModuleId.Value) return;
			if (nodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset.Where(node => node.Type == TrackNodeType.Width))
			{
				var item = node.Model;
				var newWidth = newWidthFunc.Invoke(item.Position);
				if (newWidth < 0) continue;
				var newItem = item.SetPosition(newWidth);
				var time = node.Time.Value;
				var arg = new UpdateMoveListArg(node.IsFirst, time, new(time, newItem));
				var track = node.Track;
				if (!args.TryGetValue(track, out var list))
				{
					list = new();
					args[track] = list;
				}

				list.Add(arg);
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

		public void CreateNodes()
		{
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			bool? posIsLeft = null;
			foreach (var info in nodeDataset)
			{
				if (info.Parent.Value is not { Model: ITrack model } track) continue;
				UpdateMoveListArg? arg = null;
				var newNode = info.Node.Value.Clone();
				var time = info.Time.Value;
				switch (model.Movement)
				{
					case TrackDirectMovement:
						arg = info.Type.Value switch
						{
							TrackNodeType.Pos or TrackNodeType.Left or TrackNodeType.Right =>
								new UpdateMoveListArg(true, null, new(time, newNode)),
							TrackNodeType.Width =>
								new UpdateMoveListArg(false, null, new(time, newNode)),
							_ => throw new ArgumentOutOfRangeException()
						};

						break;
					case TrackEdgeMovement:
						switch (info.Type.Value)
						{
							case TrackNodeType.Left or TrackNodeType.Right:
								arg = new UpdateMoveListArg(info.Type == TrackNodeType.Left, null, new(time, newNode));
								break;
							case TrackNodeType.Pos:
								if (posIsLeft is null)
								{
									var (minTime, minTimePos) = nodeDataset.Min(node => (
										node.Type == TrackNodeType.Pos ? node.Time.Value : T3Time.MaxValue,
										node.Node.Value.Position));
									float leftX = model.Movement.GetLeftPos(minTime);
									float rightX = model.Movement.GetRightPos(minTime);
									posIsLeft = minTimePos < (leftX + rightX) / 2;
								}

								arg = new UpdateMoveListArg(posIsLeft.Value, null, new(time, newNode));
								break;
							case TrackNodeType.Width:
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						break;
				}

				if (arg is null) continue;
				if (!args.TryGetValue(track, out var list))
				{
					list = new();
					args[track] = list;
				}

				list.Add(arg);
			}

			BatchCommand command = new(args
					.Select(pair => (Track: pair.Key, Command: new UpdateMoveListCommand(pair.Value)))
					.Where(tuple => tuple.Command.SetInit(tuple.Track))
					.Select(tuple => tuple.Command),
				"CreateNodes");
			commandManager.Add(command);
		}
	}
}
