#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Preset.Drag;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class NoteDragHelper : DragHelper
	{
		private readonly ChartSelectDataset dataset;
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly CommandManager commandManager;

		private readonly List<ChartComponent> draggingNotes = new();

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;
		public override Vector3 CurrentScreenPoint => Input.mousePosition;

		public IReadOnlyList<ChartComponent> DraggingNotes => draggingNotes;

		public T3Time BeginTime { get; private set; }

		public NoteDragHelper(
			StageMouseTimeRetriever timeRetriever,
			ChartSelectDataset dataset,
			CommandManager commandManager)
		{
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
			this.commandManager = commandManager;
		}

		public void Prepare(T3Time beginTime)
		{
			BeginTime = beginTime;
			draggingNotes.Clear();
			draggingNotes.AddRange(dataset.Where(c => c.Model is INote));
			draggingNotes.Sort((a, b) => a.Model.TimeMin.CompareTo(b.Model.TimeMin));
		}

		protected override void BeginDragLogic()
		{
			if (draggingNotes.Count == 0) CancelDrag();
		}

		protected override void OnDraggingLogic()
		{
		}

		protected override void EndDragLogic()
		{
			if (draggingNotes.Count > 0 && timeRetriever.GetMouseTimeStart(out var newTime))
			{
				var distance = newTime - BeginTime;
				if (Mathf.Abs(distance) < ISingleton<InScreenEditSetting>.Instance.TimeDragThreshold.Value)
				{
					return;
				}

				if (draggingNotes.Any(c => !c.IsWithinParentRange(distance)))
				{
					T3Logger.Log("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
					return;
				}

				var commands = draggingNotes.Select(c => new UpdateComponentCommand(
					component => component.Nudge(distance),
					component => component.Nudge(-distance), c));

				commandManager.Add(new BatchCommand(commands, "Dragging Notes"));
			}
		}

		protected override void CancelDragLogic() => draggingNotes.Clear();
	}

	public class NoteDragPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority dragPriority = default!;

		public NotifiableProperty<bool> IsDragging => dragHelper.IsDragging;

		public T3Time BeginTime => dragHelper.BeginTime;

		public IReadOnlyList<ChartComponent> DraggingNotes => dragHelper.DraggingNotes;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnBeginDragInput,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnEndDragInput,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape", OnCancelDragInput)
		};

		// Private
		private SelectInputSystem inputSystem = default!;
		private ChartSelectDataset dataset = default!;
		private NoteDragHelper dragHelper = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			SelectInputSystem inputSystem,
			ChartSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			CommandManager commandManager)
		{
			this.inputSystem = inputSystem;
			this.dataset = dataset;

			dragHelper = new NoteDragHelper(timeRetriever, dataset, commandManager);
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		private bool OnBeginDragInput()
		{
			if (dragHelper.IsDragging.Value) return true;
			if (!inputSystem.RaycastSystems.TryGetValue(T3Flag.Note, out var raycastSystem)) return true;

			var span = raycastSystem.DoRaycastNonSelect();
			foreach (var pair in span)
			{
				if (dataset.Contains(pair.Value))
				{
					dragHelper.Prepare(pair.Value.Model.TimeMin);
					return !dragHelper.BeginDrag();
				}
			}

			return true;
		}

		private bool OnEndDragInput() => !dragHelper.EndDrag();

		private void OnCancelDragInput() => dragHelper.CancelDrag();
	}
}