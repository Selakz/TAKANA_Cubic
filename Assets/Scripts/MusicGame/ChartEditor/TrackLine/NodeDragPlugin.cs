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
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeDragHelper : DragHelper
	{
		private readonly EdgeNodeSelectDataset dataset;
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly StageMouseWidthRetriever widthRetriever;
		private readonly CommandManager commandManager;

		private readonly List<EdgeNodeComponent> draggingNodes = new();

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;
		public override Vector3 CurrentScreenPoint => Input.mousePosition;

		public IReadOnlyList<EdgeNodeComponent> DraggingNodes => draggingNodes;

		public T3Time BeginTime { get; private set; }
		public float BeginPos { get; private set; }
		public bool IsBeginLeft { get; private set; }
		public bool IsSplit { get; private set; }

		public NodeDragHelper(
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

	public class NodeDragPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority dragPriority = default!;

		public NotifiableProperty<bool> IsDragging => dragHelper.IsDragging;

		public T3Time BeginTime => dragHelper.BeginTime;

		public IReadOnlyList<EdgeNodeComponent> DraggingNodes => dragHelper.DraggingNodes;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnBeginDragInput,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnEndDragInput,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape", OnCancelDragInput),
			new InputPressingRegistrar("General", "Shift", value => isShiftPressing = value),
		};

		// Private
		private EdgeNodeSelectInputSystem inputSystem = default!;
		private EdgeNodeSelectDataset dataset = default!;
		private NodeDragHelper dragHelper = default!;

		private bool isShiftPressing = false;

		// Defined Functions
		[Inject]
		private void Construct(
			EdgeNodeSelectInputSystem inputSystem,
			EdgeNodeSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			this.inputSystem = inputSystem;
			this.dataset = dataset;

			dragHelper = new NodeDragHelper(dataset, timeRetriever, widthRetriever, commandManager);
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		private bool OnBeginDragInput()
		{
			if (dragHelper.IsDragging.Value) return true;

			var span = inputSystem.System.DoRaycastNonSelect();
			foreach (var (hit, node) in span)
			{
				if (dataset.Contains(node) && hit.collider is not MeshCollider) // Exclude line mesh
				{
					dragHelper.Prepare(node.Locator.Time, node.Model.Position, node.Locator.IsLeft, isShiftPressing);
					return !dragHelper.BeginDrag();
				}
			}

			return true;
		}

		private bool OnEndDragInput() => !dragHelper.EndDrag();

		private void OnCancelDragInput() => dragHelper.CancelDrag();
	}
}