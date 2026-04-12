#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Preset.Drag;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class EdgeNodeDragHelper : DragHelper
	{
		private readonly EdgeNodeSelectDataset dataset;
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly StageMouseWidthRetriever widthRetriever;
		private readonly CommandManager commandManager;

		private readonly List<EdgeNodeComponent> draggingNodes = new();

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;
		public override Vector3 CurrentScreenPoint => Input.mousePosition;
		public T3Time BeginTime { get; private set; }
		public float BeginPos { get; private set; }
		public bool IsBeginLeft { get; private set; }
		public bool IsSplit { get; private set; }

		public EdgeNodeDragHelper(
			EdgeNodeSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			this.commandManager = commandManager;
		}

		public void Prepare(T3Time beginTime, float beginPos, bool isLeft, bool isSplit)
		{
			BeginTime = beginTime;
			BeginPos = beginPos;
			IsBeginLeft = isLeft;
			IsSplit = isSplit;
			draggingNodes.Clear();
			draggingNodes.AddRange(dataset);
			draggingNodes.Sort((a, b) => a.Locator.Time.CompareTo(b.Locator.Time));
		}

		protected override void BeginDragLogic()
		{
			if (draggingNodes.Count == 0) CancelDrag();
		}

		protected override void OnDraggingLogic()
		{
		}

		protected override void EndDragLogic()
		{
			if (draggingNodes.Count > 0 &&
			    timeRetriever.GetMouseTimeStart(out var newTime) &&
			    widthRetriever.GetMouseAttachedPosition(out var newPosition))
			{
				var distance = newTime - BeginTime;
				var offset = newPosition - BeginPos;
				Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
				foreach (var node in draggingNodes)
				{
					var track = node.Locator.Track;
					var newNodeTime = node.Locator.Time + distance;
					var newNodePos =
						node.Model.Position + offset * (IsSplit && node.Locator.IsLeft != IsBeginLeft ? -1 : 1);
					var newModel = node.Model.SetPosition(newNodePos);
					var arg = new UpdateMoveListArg(node.Locator.IsLeft, node.Locator.Time, new(newNodeTime, newModel));
					if (!args.ContainsKey(track)) args[track] = new() { arg };
					else args[track].Add(arg);
				}

				var commands = (
					from pair in args
					let command = new UpdateMoveListCommand(pair.Value)
					where command.SetInit(pair.Key)
					select command).Cast<ICommand>();
				commandManager.Add(new BatchCommand(commands, "Dragging Nodes"));
			}
		}

		protected override void CancelDragLogic() => draggingNodes.Clear();
	}

	public class DirectNodeDragHelper : DragHelper
	{
		public enum DragType
		{
			Time, // When mouse target is width node's box collider
			TimeAndPos, // When mouse target is pos node or edge node
			Width // When mouse target is width node's node collider
		}

		private readonly DirectNodeSelectDataset dataset;
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly StageMouseWidthRetriever widthRetriever;
		private readonly CommandManager commandManager;

		private readonly List<DirectNodeComponent> draggingNodes = new();

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;
		public override Vector3 CurrentScreenPoint => Input.mousePosition;
		public T3Time BeginTime { get; private set; }
		public float BeginPos { get; private set; }
		public DragType DraggingType { get; private set; }

		public DirectNodeDragHelper(
			DirectNodeSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			this.commandManager = commandManager;
		}

		public void Prepare(T3Time beginTime, float beginPos, DragType dragType)
		{
			BeginTime = beginTime;
			BeginPos = beginPos;
			DraggingType = dragType;
			draggingNodes.Clear();
			draggingNodes.AddRange(dataset);
			draggingNodes.Sort((a, b) => a.Locator.Time.CompareTo(b.Locator.Time));
		}

		protected override void BeginDragLogic()
		{
			if (draggingNodes.Count == 0) CancelDrag();
		}

		protected override void OnDraggingLogic()
		{
		}

		protected override void EndDragLogic()
		{
			if (draggingNodes.Count > 0 &&
			    timeRetriever.GetMouseTimeStart(out var newTime) &&
			    widthRetriever.GetMouseAttachedPosition(out var newPosition))
			{
				var distance = newTime - BeginTime;
				Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
				foreach (var node in draggingNodes)
				{
					var track = node.Locator.Track;
					var newNodeTime = node.Locator.Time + distance;
					UpdateMoveListArg? arg = null;
					switch (DraggingType)
					{
						case DragType.Time:
							arg = new UpdateMoveListArg(
								node.Locator.IsPos, node.Locator.Time, new(newNodeTime, node.Model));
							break;
						case DragType.TimeAndPos:
							if (node.Locator.IsPos)
							{
								var offset = newPosition - BeginPos;
								var newNodePos = node.Model.Position + offset;
								var newModel = node.Model.SetPosition(newNodePos);
								arg = new UpdateMoveListArg(
									node.Locator.IsPos, node.Locator.Time, new(newNodeTime, newModel));
							}
							else
							{
								arg = new UpdateMoveListArg(
									node.Locator.IsPos, node.Locator.Time, new(newNodeTime, node.Model));
							}

							break;
						case DragType.Width:
							if (node.Locator.IsPos) continue;
							var newWidth = node.Model.Position + (newPosition - BeginPos) * 2;
							if (newWidth <= 0) continue;
							var newWidthModel = node.Model.SetPosition(newWidth);
							arg = new UpdateMoveListArg(
								node.Locator.IsPos, node.Locator.Time, new(node.Locator.Time, newWidthModel));
							break;
					}

					if (arg is null) continue;
					if (!args.ContainsKey(track)) args[track] = new() { arg };
					else args[track].Add(arg);
				}

				var commands = (
					from pair in args
					let command = new UpdateMoveListCommand(pair.Value)
					where command.SetInit(pair.Key)
					select command).Cast<ICommand>();
				commandManager.Add(new BatchCommand(commands, "Dragging Nodes"));
			}
		}

		protected override void CancelDragLogic() => draggingNodes.Clear();
	}

	public class NodeDragPlugin : HierarchySystem<NodeDragPlugin>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority dragPriority = default!;

		public NotifiableProperty<bool> IsDragging { get; } = new(false);

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnBeginDragInput,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnEndDragInput,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape", OnCancelDragInput),
			new InputPressingRegistrar("General", "Shift", value => isShiftPressing = value),

			// If this is ReactiveProperty then we can use CombineLatest, but it's not. Fine.
			new PropertyRegistrar<bool>(edgeDragHelper.IsDragging, () =>
				IsDragging.Value = edgeDragHelper.IsDragging.Value || directDragHelper.IsDragging.Value),
			new PropertyRegistrar<bool>(directDragHelper.IsDragging, () =>
				IsDragging.Value = edgeDragHelper.IsDragging.Value || directDragHelper.IsDragging.Value),
		};

		// Private
		[Inject] private readonly NodeSelectInputSystem inputSystem = default!;
		[Inject] private readonly EdgeNodeSelectDataset edgeDataset = default!;
		[Inject] private readonly DirectNodeSelectDataset directDataset = default!;
		[Inject] private readonly IViewPool<EdgeNodeComponent> edgePool = default!;
		[Inject] private readonly IViewPool<DirectNodeComponent> directPool = default!;

		private EdgeNodeDragHelper edgeDragHelper = default!;
		private DirectNodeDragHelper directDragHelper = default!;
		private bool isShiftPressing = false;

		// Defined Functions
		[Inject]
		private void Construct(
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			edgeDragHelper = new EdgeNodeDragHelper(edgeDataset, timeRetriever, widthRetriever, commandManager);
			directDragHelper = new DirectNodeDragHelper(directDataset, timeRetriever, widthRetriever, commandManager);
		}

		private bool OnBeginDragInput()
		{
			if (edgeDragHelper.IsDragging.Value) return true;

			var result = true;
			var directPrepared = false;
			var edgeSpan = inputSystem.EdgeSystem.DoRaycastNonSelect();
			foreach (var (hit, node) in edgeSpan)
			{
				var view = edgePool[node]?.Script<PositionNodeView>();
				if (hit.collider == view?.NodeCollider) // Exclude line collider
				{
					edgeDragHelper.Prepare(
						node.Locator.Time, node.Model.Position, node.Locator.IsLeft, isShiftPressing);
					directDragHelper.Prepare(
						node.Locator.Time, node.Model.Position, DirectNodeDragHelper.DragType.TimeAndPos);
					directPrepared = true;
					result = result && !edgeDragHelper.BeginDrag() && !directDragHelper.BeginDrag();
					break;
				}
			}

			if (!directPrepared)
			{
				var directSpan = inputSystem.DirectSystem.DoRaycastNonSelect();
				foreach (var (hit, node) in directSpan)
				{
					if (!directDataset.Contains(node)) continue;
					if (node.Locator.IsPos)
					{
						directDragHelper.Prepare(
							node.Locator.Time, node.Model.Position, DirectNodeDragHelper.DragType.TimeAndPos);
					}
					else
					{
						var view = directPool[node]?.Script<WidthNodeView>();
						if (hit.collider == view?.LineCollider)
						{
							directDragHelper.Prepare(node.Locator.Time, node.Model.Position,
								DirectNodeDragHelper.DragType.Time);
						}
						else if (hit.collider == view?.NodeCollider)
						{
							var model = (node.Locator.Track.Model as ITrack)!;
							directDragHelper.Prepare(node.Locator.Time, model.Movement.GetRightPos(node.Locator.Time),
								DirectNodeDragHelper.DragType.Width);
						}
					}

					result = result && !directDragHelper.BeginDrag();
					break;
				}
			}

			return result;
		}

		private bool OnEndDragInput() => !edgeDragHelper.EndDrag() && !directDragHelper.EndDrag();

		private void OnCancelDragInput()
		{
			edgeDragHelper.CancelDrag();
			directDragHelper.CancelDrag();
		}
	}
}