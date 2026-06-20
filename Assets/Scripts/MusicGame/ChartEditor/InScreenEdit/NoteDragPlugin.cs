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
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class NoteDragHelper : DragHelper, IComponentDragHelper
	{
		private readonly ChartSelectDataset selectDataset;
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly CommandManager commandManager;
		private readonly IDataset<NoteRawInfo> rawDataset;
		private readonly INoteRawInfoService service;

		private readonly List<NoteRawInfo> draggingInfos = new();
		private T3Time lastTime = 0;

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;
		public override Vector3 CurrentScreenPoint => Input.mousePosition;

		public virtual T3Time TimeDragThreshold => ISingleton<InScreenEditSetting>.Instance.TimeDragThreshold.Value;

		public IReadOnlyList<NoteRawInfo> DraggingInfos => draggingInfos;

		public T3Time BeginTime { get; private set; }

		public IChartModel? BaseModel { get; private set; }

		public NoteDragHelper(
			StageMouseTimeRetriever timeRetriever,
			ChartSelectDataset selectDataset,
			CommandManager commandManager,
			IDataset<NoteRawInfo> rawDataset,
			INoteRawInfoService service)
		{
			this.selectDataset = selectDataset;
			this.timeRetriever = timeRetriever;
			this.commandManager = commandManager;
			this.rawDataset = rawDataset;
			this.service = service;
		}

		public void Prepare(IChartModel model)
		{
			BaseModel = IChartSerializable.Clone(model);
			BeginTime = model.TimeMin;
		}

		protected virtual NoteRawInfo? FromComponent(ChartComponent note) => NoteRawInfo.FromComponent(note);

		protected override void BeginDragLogic()
		{
			var notes = selectDataset.Where(c => c.Model is INote).ToList();
			if (notes.Count == 0)
			{
				CancelDrag();
				return;
			}

			rawDataset.Clear();
			draggingInfos.Clear();
			foreach (var note in notes)
			{
				if (FromComponent(note) is { } info)
				{
					rawDataset.Add(info);
					draggingInfos.Add(info);
				}
			}

			lastTime = BeginTime;
		}

		protected override void OnDraggingLogic()
		{
			if (draggingInfos.Count == 0 || !timeRetriever.GetMouseTimeStart(out var timeJudge)) return;

			var distance = timeJudge - lastTime;
			foreach (var info in draggingInfos)
			{
				if (distance > 0)
				{
					info.TimeEnd.Value += distance;
					info.TimeJudge.Value += distance;
				}
				else
				{
					info.TimeJudge.Value += distance;
					info.TimeEnd.Value += distance;
				}
			}

			lastTime = timeJudge;
		}

		protected override void EndDragLogic()
		{
			if (draggingInfos.Count > 0 && timeRetriever.GetMouseTimeStart(out var newTime))
			{
				var distance = newTime - BeginTime;
				if (Mathf.Abs(distance) < TimeDragThreshold) return;

				if (draggingInfos.Any(info => service.IsValid(info) is not null))
				{
					T3Logger.Log("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
					return;
				}

				List<ICommand> commands = new();
				foreach (var info in draggingInfos)
				{
					var model = info.GenerateModel();
					if (info.Parent.Value is { BelongingChart: { } chart } parent)
						commands.Add(new AddComponentCommand(chart, model, parent));
				}

				commands.AddRange(selectDataset
					.Where(c => c.Model is INote)
					.Select(note => new DeleteComponentCommand(note)));

				commandManager.Add(new BatchCommand(commands, "Dragging Notes"));
			}
		}

		protected override void CancelDragLogic()
		{
			rawDataset.Clear();
			draggingInfos.Clear();
		}
	}

	public class NoteDragPlugin : HierarchySystem<NoteDragPlugin>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;
		[SerializeField] private SequencePriority dragPriority = default!;

		public NotifiableProperty<bool> IsDragging => DragHelper.IsDragging;

		public T3Time BeginTime => DragHelper.BaseModel?.TimeMin ?? 0;

		public IReadOnlyList<NoteRawInfo> DraggingInfos => ((NoteDragHelper)DragHelper.Current.Value).DraggingInfos;

		public OverridableComponentDragHelper DragHelper { get; private set; } = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnBeginDragInput,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnEndDragInput,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape", OnCancelDragInput),

			new PropertyRegistrar<bool>(DragHelper.IsDragging, value =>
			{
				if (value) moduleInfo.Register(moduleId);
				else moduleInfo.Unregister(moduleId);
			})
		};

		// Private
		[Inject] private ModuleInfo moduleInfo = default!;
		[Inject] private SelectInputSystem inputSystem = default!;
		[Inject] private ChartSelectDataset dataset = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			ChartSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			CommandManager commandManager,
			IDataset<NoteRawInfo> rawDataset,
			INoteRawInfoService service)
		{
			var defaultDragHelper = new NoteDragHelper(timeRetriever, dataset, commandManager, rawDataset, service);
			DragHelper = new OverridableComponentDragHelper(defaultDragHelper);
		}

		private bool OnBeginDragInput()
		{
			if (DragHelper.IsDragging.Value) return true;
			if (!inputSystem.RaycastSystems.TryGetValue(T3Flag.Note, out var raycastSystem)) return true;

			var span = raycastSystem.DoRaycastNonSelect();
			foreach (var pair in span)
			{
				if (dataset.Contains(pair.Value))
				{
					DragHelper.Prepare(pair.Value.Model);
					return !DragHelper.BeginDrag();
				}
			}

			return true;
		}

		private bool OnEndDragInput() => !DragHelper.EndDrag();

		private void OnCancelDragInput() => DragHelper.CancelDrag();
	}
}