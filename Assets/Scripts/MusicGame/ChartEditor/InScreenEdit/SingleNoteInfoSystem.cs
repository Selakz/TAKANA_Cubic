#nullable enable

using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public interface INoteParentService
	{
		public ChartComponent? GetParent(SingleNotePlaceType notePlaceType);
	}

	public class SingleNoteInfoSystem : HierarchySystem<SingleNoteInfoSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, id => IsEnabled = id == moduleId.Value)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<T3Flag>(noteType, flag =>
			{
				foreach (var info in dataset) info.NoteFlag.Value = flag;
			})
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private IDataset<NoteRawInfo> dataset = default!;
		private NotifiableProperty<T3Flag> noteType = default!;
		private NotifiableProperty<SingleNotePlaceType> notePlaceType = default!;
		private INoteParentService noteParentService = default!;
		private StageMouseTimeRetriever timeRetriever = default!;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			IDataset<NoteRawInfo> dataset,
			NotifiableProperty<T3Flag> noteType,
			NotifiableProperty<SingleNotePlaceType> notePlaceType,
			INoteParentService noteParentService,
			StageMouseTimeRetriever timeRetriever)
		{
			this.moduleInfo = moduleInfo;
			this.dataset = dataset;
			this.noteType = noteType;
			this.notePlaceType = notePlaceType;
			this.noteParentService = noteParentService;
			this.timeRetriever = timeRetriever;
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			dataset.Clear();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			dataset.Clear();
		}

		void Update()
		{
			if (!timeRetriever.GetMouseTimeStart(out var timeJudge) ||
			    !timeRetriever.GetMouseHoldTimeEnd(out var timeEnd)) return;
			timeEnd = Mathf.Max(timeJudge + 1, timeEnd);

			if (dataset.Count == 0)
			{
				dataset.Add(new(timeJudge, timeEnd, noteType.Value, noteParentService.GetParent(notePlaceType.Value)));
			}

			var info = dataset.First();
			info.Parent.Value = noteParentService.GetParent(notePlaceType.Value);
			if (info.Parent.Value?.Model is { } model)
				timeEnd = Mathf.Clamp(timeEnd, model.TimeMin, model.TimeMax);

			info.TimeJudge.Value = timeJudge;
			info.TimeEnd.Value = timeEnd;
		}
	}
}