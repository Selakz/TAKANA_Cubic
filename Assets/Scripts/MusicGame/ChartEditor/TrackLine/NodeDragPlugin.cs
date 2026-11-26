#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Drag;
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
	public class TrackNodeDragHelper : DragHelper
	{
		public enum DragType
		{
			Time, // When mouse target is width node's line collider
			TimeAndPos, // When mouse target is pos node or edge node
			Width // When mouse target is width node's node collider
		}

		private readonly TrackNodeSelectDataset dataset;
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly StageMouseWidthRetriever widthRetriever;
		private readonly CommandManager commandManager;

		private readonly List<TrackNodeComponent> draggingNodes = new();

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;
		public override Vector3 CurrentScreenPoint => Mouse.current.position.ReadValue();
		public T3Time BeginTime { get; private set; }
		public float BeginPos { get; private set; }
		public TrackNodeType BeginType { get; private set; }
		public bool IsSplit { get; private set; }
		public DragType DraggingType { get; private set; }

		public TrackNodeDragHelper(
			TrackNodeSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			this.commandManager = commandManager;
		}

		public void Prepare(T3Time beginTime, float beginPos, TrackNodeType beginType, bool isSplit, DragType dragType)
		{
			BeginTime = beginTime;
			BeginPos = beginPos;
			BeginType = beginType;
			IsSplit = isSplit;
			DraggingType = dragType;
			draggingNodes.Clear();
			draggingNodes.AddRange(dataset);
			draggingNodes.Sort((a, b) => a.Time.Value.CompareTo(b.Time.Value));
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
			if (draggingNodes.Count == 0 ||
			    !timeRetriever.GetMouseTimeStart(out var newTime) ||
			    !widthRetriever.GetMouseAttachedPosition(out var newPosition))
			{
				return;
			}

			var distance = newTime - BeginTime;
			var offset = newPosition - BeginPos;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in draggingNodes)
			{
				var track = node.Track;
				var oldTime = node.Time.Value;
				var newNodeTime = oldTime + distance;
				UpdateMoveListArg? arg = null;
				switch (DraggingType)
				{
					case DragType.Time:
						arg = new UpdateMoveListArg(node.IsFirst, oldTime, new(newNodeTime, node.Model));
						break;
					case DragType.TimeAndPos:
						if (node.IsPosition)
						{
							var sign = IsSplit && node.IsEdge && BeginType.IsEdge() && node.Type != BeginType ? -1 : 1;
							var newNodePos = node.Model.Position + offset * sign;
							var newModel = node.Model.SetPosition(newNodePos);
							arg = new UpdateMoveListArg(node.IsFirst, oldTime, new(newNodeTime, newModel));
						}
						else
						{
							arg = new UpdateMoveListArg(node.IsFirst, oldTime, new(newNodeTime, node.Model));
						}

						break;
					case DragType.Width:
						if (node.Type != TrackNodeType.Width) continue;
						var newWidth = node.Model.Position + offset * 2;
						if (newWidth <= 0) continue;
						var newWidthModel = node.Model.SetPosition(newWidth);
						arg = new UpdateMoveListArg(node.IsFirst, oldTime, new(oldTime, newWidthModel));
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

			var commands = (
				from pair in args
				let command = new UpdateMoveListCommand(pair.Value)
				where command.SetInit(pair.Key)
				select command).Cast<ICommand>();
			commandManager.Add(new BatchCommand(commands, "Dragging Nodes"));
		}

		protected override void CancelDragLogic() => draggingNodes.Clear();
	}

	public class NodeDragPlugin : HierarchySystem<NodeDragPlugin>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority dragPriority = default!;

		public NotifiableProperty<bool> IsDragging => dragHelper.IsDragging;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnBeginDragInput,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnEndDragInput,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape", OnCancelDragInput),
			new InputPressingRegistrar("General", "Shift", value => isShiftPressing = value)
		};

		// Private
		[Inject] private readonly NodeSelectInputSystem inputSystem = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;

		private TrackNodeDragHelper dragHelper = default!;
		private bool isShiftPressing = false;

		// Constructor
		[Inject]
		private void Construct(
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			dragHelper = new TrackNodeDragHelper(nodeSelectDataset, timeRetriever, widthRetriever, commandManager);
		}

		// Event Handlers
		private bool OnBeginDragInput()
		{
			if (dragHelper.IsDragging.Value) return true;

			var span = inputSystem.System.DoRaycastNonSelect();
			foreach (var (hit, node) in span)
			{
				if (!nodeSelectDataset.Contains(node)) continue;
				if (node.IsPosition)
				{
					var view = nodePool[node]?.Script<PositionNodeView>();
					if (hit.collider == view?.NodeCollider) // Exclude line collider
					{
						dragHelper.Prepare(
							node.Time.Value, node.Model.Position, node.Type, isShiftPressing,
							TrackNodeDragHelper.DragType.TimeAndPos);
						return !dragHelper.BeginDrag();
					}
				}
				else
				{
					var view = nodePool[node]?.Script<WidthNodeView>();
					if (hit.collider == view?.LineCollider)
					{
						dragHelper.Prepare(
							node.Time.Value, node.Model.Position, node.Type, false,
							TrackNodeDragHelper.DragType.Time);
						return !dragHelper.BeginDrag();
					}

					if (hit.collider == view?.NodeCollider)
					{
						dragHelper.Prepare(
							node.Time.Value, node.Movement.GetRightPos(node.Time.Value), node.Type, false,
							TrackNodeDragHelper.DragType.Width);
						return !dragHelper.BeginDrag();
					}
				}
			}

			return true;
		}

		private bool OnEndDragInput() => !dragHelper.EndDrag();

		private void OnCancelDragInput() => dragHelper.CancelDrag();
	}
}
