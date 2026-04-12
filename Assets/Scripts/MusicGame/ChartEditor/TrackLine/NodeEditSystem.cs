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
			new InputRegistrar("InScreenEdit", "Create",
				CreateNodes),
			new InputRegistrar("InScreenEdit", "Delete",
				() =>
				{
					Dictionary<ChartComponent, List<UpdateMoveListArg>> tracks = new();
					if (edgeNodeSelectDataset.Count == 0 && directNodeSelectDataset.Count == 0) return;
					foreach (var node in edgeNodeSelectDataset)
					{
						var track = node.Locator.Track;
						if (!tracks.TryGetValue(track, out var args))
						{
							args = new();
							tracks[track] = args;
						}

						args.Add(new UpdateMoveListArg(node.Locator.IsLeft, node.Locator.Time, null));
					}

					foreach (var node in directNodeSelectDataset)
					{
						var track = node.Locator.Track;
						if (!tracks.TryGetValue(track, out var args))
						{
							args = new();
							tracks[track] = args;
						}

						args.Add(new UpdateMoveListArg(node.Locator.IsPos, node.Locator.Time, null));
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
				if (edgeNodeSelectDataset.Count == 0 && directNodeSelectDataset.Count == 0) return;
				Dictionary<ChartComponent, List<UpdateMoveListArg>> tracks = new();
				foreach (var node in edgeNodeSelectDataset)
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

				foreach (var node in directNodeSelectDataset)
				{
					if (node.Model is not V1EMoveItem v1e) continue;
					var track = node.Locator.Track;
					if (!tracks.TryGetValue(track, out var args))
					{
						args = new();
						tracks[track] = args;
					}

					args.Add(new UpdateMoveListArg(node.Locator.IsPos, node.Locator.Time,
						new(node.Locator.Time, new V1EMoveItem(v1e.Position, GetNextEase(v1e.Ease)))));
				}

				List<ICommand> commands = (
						from pair in tracks
						let command = new UpdateMoveListCommand(pair.Value)
						where command.SetInit(pair.Key)
						select command)
					.Cast<ICommand>().ToList();
				commandManager.Add(new BatchCommand(commands, "Delete Nodes"));
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
		[Inject] private readonly EdgeNodeSelectDataset edgeNodeSelectDataset = default!;
		[Inject] private readonly DirectNodeSelectDataset directNodeSelectDataset = default!;
		[Inject] private readonly IDataset<NodeRawInfo> nodeDataset = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Defined Functions
		public void UpdatePosition(Func<float, float> newPositionFunc)
		{
			if (moduleInfo.CurrentModule.Value != updateModuleId.Value) return;
			if (edgeNodeSelectDataset.Count == 0 && directNodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in edgeNodeSelectDataset)
			{
				var item = node.Model;
				var newItem = item.SetPosition(newPositionFunc.Invoke(item.Position));
				var time = node.Locator.Time;
				var arg = new UpdateMoveListArg(node.Locator.IsLeft, time, new(time, newItem));
				var track = node.Locator.Track;
				if (!args.ContainsKey(track)) args[track] = new() { arg };
				else args[track].Add(arg);
			}

			foreach (var node in directNodeSelectDataset)
			{
				var item = node.Model;
				var newItem = item.SetPosition(newPositionFunc.Invoke(item.Position));
				if (!node.Locator.IsPos && newItem.Position < 0) continue;
				var time = node.Locator.Time;
				var arg = new UpdateMoveListArg(node.Locator.IsPos, time, new(time, newItem));
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
			if (moduleInfo.CurrentModule.Value != updateModuleId.Value) return;
			if (edgeNodeSelectDataset.Count == 0 && directNodeSelectDataset.Count == 0) return;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in edgeNodeSelectDataset)
			{
				var item = node.Model;
				var time = node.Locator.Time;
				var newTime = newTimeFunc.Invoke(time);
				var arg = new UpdateMoveListArg(node.Locator.IsLeft, time, new(newTime, item));
				var track = node.Locator.Track;
				if (!args.ContainsKey(track)) args[track] = new() { arg };
				else args[track].Add(arg);
			}

			foreach (var node in directNodeSelectDataset)
			{
				var item = node.Model;
				var time = node.Locator.Time;
				var newTime = newTimeFunc.Invoke(time);
				var arg = new UpdateMoveListArg(node.Locator.IsPos, time, new(newTime, item));
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
							NodeType.Pos or NodeType.Left or NodeType.Right =>
								new UpdateMoveListArg(true, null, new(time, newNode)),
							NodeType.Width =>
								new UpdateMoveListArg(false, null, new(time, newNode)),
							_ => throw new ArgumentOutOfRangeException()
						};

						break;
					case TrackEdgeMovement:
						switch (info.Type.Value)
						{
							case NodeType.Left or NodeType.Right:
								arg = new UpdateMoveListArg(info.Type == NodeType.Left, null, new(time, newNode));
								break;
							case NodeType.Pos:
								if (posIsLeft is null)
								{
									var (minTime, minTimePos) = nodeDataset.Min(node => (
										node.Type == NodeType.Pos ? node.Time.Value : T3Time.MaxValue,
										node.Node.Value.Position));
									float leftX = model.Movement.GetLeftPos(minTime);
									float rightX = model.Movement.GetRightPos(minTime);
									posIsLeft = minTimePos < (leftX + rightX) / 2;
								}

								arg = new UpdateMoveListArg(posIsLeft.Value, null, new(time, newNode));
								break;
							case NodeType.Width:
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						break;
				}

				if (arg is null) continue;
				if (!args.ContainsKey(track)) args[track] = new() { arg };
				else args[track].Add(arg);
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