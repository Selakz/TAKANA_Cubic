#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class CopyPasteDraftNoteInfoSystem : HierarchySystem<CopyPasteDraftNoteInfoSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode,
				value =>
				{
					IsEnabled = value && moduleInfo.CurrentModule == moduleId;
					clipboard.Clear();
				}),
			new PropertyRegistrar<int>(moduleInfo.CurrentModule,
				value => IsEnabled = value == moduleId && draftContainer.IsInDraftMode.Value)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<PasteMode>(pasteMode, () => dataset.Clear())
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private ModuleInfo moduleInfo = default!;
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private NotifiableProperty<PasteMode> pasteMode = default!;
		[Inject] private IDataset<ClipboardItem> clipboard = default!;
		[Inject] private IDataset<NoteRawInfo> dataset = default!;
		[Inject] private StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private IDraftNoteService draftNoteService = default!;

		private DraftNoteRawInfo? baseInfo;

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
			baseInfo = null;
		}

		void Update()
		{
			if (!timeRetriever.GetMouseTimeStart(out var timeJudge)) return;

			T3Time minTime = T3Time.MaxValue;
			if (dataset.Count == 0)
			{
				foreach (var item in clipboard)
				{
					if (DraftNoteRawInfo.FromComponent(item.Component) is { } info)
					{
						info.Parent.Value = levelInfo.Value?.Chart.DefaultJudgeLine();
						dataset.Add(info);
						if (info.TimeJudge < minTime)
						{
							minTime = info.TimeJudge;
							baseInfo = info;
						}
					}
				}
			}

			if (baseInfo is null)
			{
				if (dataset.Count > 0) Debug.LogError("CopyPasteDraftNoteInfoSystem: baseInfo is null");
				return;
			}

			var distance = timeJudge - baseInfo!.TimeJudge;
			var offset = draftNoteService.GetMouseAttachedPosition() - baseInfo!.Position;
			foreach (var info in dataset)
			{
				// If working as expected, either execution order should be ok. But actually it doesn't, IDK why.
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

				if (info is DraftNoteRawInfo draftInfo && pasteMode.Value == PasteMode.NormalPaste)
					draftInfo.Position.Value += offset;
			}
		}
	}
}