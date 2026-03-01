#nullable enable

using System;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class CopyPasteNoteInfoSystem : HierarchySystem<CopyPasteNoteInfoSystem>
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
			new PropertyRegistrar<PasteMode>(pasteMode, () => dataset.Clear()),
			new PropertyRegistrar<ChartComponent?>(selectDataset.CurrentSelecting, component =>
			{
				if (pasteMode.Value is not PasteMode.NormalPaste) return;
				foreach (var info in dataset)
				{
					info.Parent.Value = component is { Model: ITrack } ? component : null;
				}
			})
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private NotifiableProperty<PasteMode> pasteMode = default!;
		private IDataset<ClipboardItem> clipboard = default!;
		private IDataset<NoteRawInfo> dataset = default!;
		private ChartSelectDataset selectDataset = default!;
		private StageMouseTimeRetriever timeRetriever = default!;

		private NoteRawInfo? baseInfo;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			NotifiableProperty<PasteMode> pasteMode,
			IDataset<ClipboardItem> clipboard,
			IDataset<NoteRawInfo> dataset,
			ChartSelectDataset selectDataset,
			StageMouseTimeRetriever timeRetriever)
		{
			this.moduleInfo = moduleInfo;
			this.pasteMode = pasteMode;
			this.clipboard = clipboard;
			this.dataset = dataset;
			this.selectDataset = selectDataset;
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
					if (NoteRawInfo.FromComponent(item.Component) is { } info)
					{
						info.Parent.Value = pasteMode.Value switch
						{
							PasteMode.ExactPaste => item.Parent,
							PasteMode.NormalPaste => selectDataset.CurrentSelecting.Value is { Model: ITrack } track
								? track
								: null,
							_ => throw new ArgumentOutOfRangeException(nameof(pasteMode), pasteMode.Value, null)
						};
						dataset.Add(info);
						if (info.TimeJudge < minTime)
						{
							minTime = info.TimeJudge;
							baseInfo = info;
						}
					}
				}
			}

			if (baseInfo is null && dataset.Count > 0)
			{
				Debug.LogError("CopyPasteNoteInfoSystem: baseInfo is null");
				return;
			}

			foreach (var info in dataset)
			{
				var distance = timeJudge - baseInfo!.TimeJudge;
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
			}
		}
	}
}