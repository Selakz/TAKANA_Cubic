#nullable enable

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
	public class CopyPasteTrackInfoSystem : HierarchySystem<CopyPasteTrackInfoSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, id => IsEnabled = id == moduleId.Value)
		};

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<PasteMode>(pasteMode, () => dataset.Clear())
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private NotifiableProperty<PasteMode> pasteMode = default!;
		private IDataset<ClipboardItem> clipboard = default!;
		private IDataset<TrackRawInfo> dataset = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private StageMouseWidthRetriever widthRetriever = default!;

		private TrackRawInfo? baseInfo;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			NotifiableProperty<PasteMode> pasteMode,
			IDataset<ClipboardItem> clipboard,
			IDataset<TrackRawInfo> dataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever)
		{
			this.moduleInfo = moduleInfo;
			this.pasteMode = pasteMode;
			this.clipboard = clipboard;
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
		}

		// Defined Functions
		private static float GetLeftPos(TrackRawInfo info)
		{
			return info.Track.Model is ITrack model
				? model.Movement.GetLeftPos(model.TimeMin)
				: 5f;
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
			if (!timeRetriever.GetMouseTimeStart(out var timeStart) ||
			    !widthRetriever.GetMouseAttachedPosition(out var leftAnchor)) return;

			T3Time minTime = T3Time.MaxValue;
			float minLeft = float.MaxValue;
			if (dataset.Count == 0)
			{
				foreach (var item in clipboard)
				{
					if (TrackRawInfo.FromComponent(item.Component) is { } info)
					{
						info.Parent.Value = item.Parent;
						dataset.Add(info);
						if (info.Track.Model.TimeMin < minTime)
						{
							minTime = info.Track.Model.TimeMin;
							minLeft = GetLeftPos(info);
							baseInfo = info;
						}
						else if (info.Track.Model.TimeMin == minTime)
						{
							var left = GetLeftPos(info);
							if (left < minLeft)
							{
								minLeft = left;
								baseInfo = info;
							}
						}
					}
				}
			}

			if (baseInfo is null && dataset.Count > 0)
			{
				Debug.LogError("CopyPasteTrackInfoSystem: baseInfo is null");
				return;
			}

			foreach (var info in dataset)
			{
				var distance = timeStart - baseInfo!.Track.Model.TimeMin;
				info.Track.Nudge(distance);
				var offset = 0f;
				if (pasteMode.Value is PasteMode.NormalPaste)
				{
					offset = leftAnchor - GetLeftPos(baseInfo);
					(info.Track.Model as ITrack)?.Shift(offset);
				}

				if (distance != 0 || offset is not 0) info.UpdateNotify();
			}
		}
	}
}