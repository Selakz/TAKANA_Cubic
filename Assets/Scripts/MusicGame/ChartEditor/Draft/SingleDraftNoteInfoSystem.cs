#nullable enable

using System.Linq;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class SingleDraftNoteInfoSystem : HierarchySystem<SingleDraftNoteInfoSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode,
				value => IsEnabled = value && moduleInfo.CurrentModule == moduleId),
			new PropertyRegistrar<int>(moduleInfo.CurrentModule,
				value => IsEnabled = value == moduleId && draftContainer.IsInDraftMode.Value)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<T3Flag>(noteType, flag =>
			{
				foreach (var info in dataset) info.NoteFlag.Value = flag;
			}),
			new InputRegistrar("InScreenEdit", "Widen", Widen),
			new InputRegistrar("InScreenEdit", "WidenToGrid", WidenToGrid),
			new InputRegistrar("InScreenEdit", "Narrow", Narrow),
			new InputRegistrar("InScreenEdit", "NarrowToGrid", NarrowToGrid),
			new InputRegistrar("InScreenEdit", "ToLeft", Narrow),
			new InputRegistrar("InScreenEdit", "ToRight", Widen),
			new InputRegistrar("InScreenEdit", "ToLeftGrid", NarrowToGrid),
			new InputRegistrar("InScreenEdit", "ToRightGrid", WidenToGrid)
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private ModuleInfo moduleInfo = default!;
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private ChartSelectDataset selectDataset = default!;
		[Inject] private NotifiableProperty<T3Flag> noteType = default!;
		[Inject] private IDataset<NoteRawInfo> dataset = default!;
		[Inject] private StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private IDraftNoteService draftNoteService = default!;

		// Event Handlers
		private void Widen()
		{
			if (selectDataset.Count > 0) return;
			var note = dataset.First();
			if (note is DraftNoteRawInfo draft)
				draft.Width.Value += ISingleton<DraftSetting>.Instance.DraftNoteWidthIncrement;
		}

		private void WidenToGrid()
		{
			if (selectDataset.Count > 0) return;
			var note = dataset.First();
			if (note is DraftNoteRawInfo draft)
				draft.Width.Value = draftNoteService.GetWidenedGridWidth(draft.Position.Value, draft.Width.Value);
		}

		private void Narrow()
		{
			if (selectDataset.Count > 0) return;
			var note = dataset.First();
			if (note is DraftNoteRawInfo draft)
				draft.Width.Value -= ISingleton<DraftSetting>.Instance.DraftNoteWidthIncrement;
		}

		private void NarrowToGrid()
		{
			if (selectDataset.Count > 0) return;
			var note = dataset.First();
			if (note is DraftNoteRawInfo draft)
				draft.Width.Value = draftNoteService.GetNarrowedGridWidth(draft.Position.Value, draft.Width.Value);
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
			float pos = draftNoteService.GetMouseAttachedPosition();

			if (dataset.Count == 0)
			{
				var width =
					draftNoteService.GetRightAttachedPosition(pos) - draftNoteService.GetLeftAttachedPosition(pos);
				width = Mathf.Approximately(width, 0) ? ISingleton<DraftSetting>.Instance.DefaultDraftNoteWidth : width;
				dataset.Add(new DraftNoteRawInfo(timeJudge, timeEnd, noteType.Value,
					levelInfo.Value?.Chart.DefaultJudgeLine(), pos, width));
			}

			var info = dataset.First();
			info.TimeJudge.Value = timeJudge;
			info.TimeEnd.Value = timeEnd;
			if (info is DraftNoteRawInfo draft) draft.Position.Value = pos;
		}
	}
}